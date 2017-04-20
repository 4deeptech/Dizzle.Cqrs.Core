//----------------------------------------------------------------------- 
// <copyright file="Aggregate.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;

namespace Dizzle.Cqrs.Core
{
    /// <summary>
    /// Aggregate base class, which factors out some common infrastructure that
    /// all aggregates have (ID and event application).
    /// </summary>
    public class Aggregate : IAggregate
    {
        /// <summary>
        /// The number of events loaded into this aggregate.
        /// </summary>
        public long EventsLoaded { get; private set; }

        public static Dictionary<Type, MethodInfo> Cache = new Dictionary<Type, MethodInfo>();

        public AbstractIdentity<Guid> Id { get; set; }

        /// <summary>
        /// Enumerates the supplied events and applies them in order to the aggregate.
        /// </summary>
        /// <param name="events"></param>
        public void ApplyEvents(IEnumerable<IEvent> events)
        {
            //this is a slow way to go about this
            //the method should be cached by type so you don't have to muck with reflection over and over
            foreach (var e in events)
            {
                Type typ = e.GetType();
                if (Cache.ContainsKey(typ))
                {
                    //fire the action!
                    Cache[typ].Invoke(this, new object[] { e });
                }
                else
                {
                    var method = 
                    GetType().GetRuntimeMethods().Single(t => t.Name.Equals("ApplyOneEvent"))
                        .MakeGenericMethod(typ);
                    method.Invoke(this, new object[] { e });
                    //invoke first because if the invoke fails then caching it does no good because it doesn't work!
                    Cache[typ] = method;
                }
            }
        }

        //public void ApplyEvents(IEnumerable events)
        //{
        //    //this is a slow way to go about this
        //    //the method should be cached by type so you don't have to muck with reflection over and over
        //    foreach (var e in events)
        //        GetType().GetRuntimeMethods().Single(t => t.Name.Equals("ApplyOneEvent"))
        //            .MakeGenericMethod(e.GetType())
        //            .Invoke(this, new object[] { e });
        //}

        /// <summary>
        /// Applies a single event to the aggregate.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="ev"></param>
        public void ApplyOneEvent<TEvent>(TEvent ev)
        {
            var applier = this as IApplyEvent<TEvent>;
            if (applier == null)
                throw new InvalidOperationException(string.Format(
                    "Aggregate {0} does not know how to apply event {1}",
                    GetType().Name, ev.GetType().Name));
            applier.Apply(ev);
            EventsLoaded++;
        }
    }
}
