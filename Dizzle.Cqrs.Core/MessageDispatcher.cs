//----------------------------------------------------------------------- 
// <copyright file="MessageDispatcher.cs" company="4Deep Technologies LLC"> 
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
    /// This implements a basic message dispatcher, driving the overall command handling
    /// and event application/distribution process. It is suitable for a simple, single
    /// node application that can safely build its subscriber list at startup and keep
    /// it in memory. Depends on some kind of event storage mechanism.
    /// </summary>
    public class MessageDispatcher
    {
        private Dictionary<Type, Action<object>> commandHandlers =
            new Dictionary<Type, Action<object>>();      
        private Dictionary<Type, List<Action<object>>> eventSubscribers =
            new Dictionary<Type, List<Action<object>>>();
        private IEventStore eventStore;
        private IDocumentStore docStore;

        /// <summary>
        /// Initializes a message dispatcher, which will use the specified event store
        /// implementation.
        /// </summary>
        /// <param name="es"></param>
        /// /// <param name="viewStore"></param>
        public MessageDispatcher(IEventStore es, IDocumentStore viewStore)
        {
            eventStore = es;
            docStore = viewStore;
        }

        /// <summary>
        /// Tries to send the specified command to its handler. Throws an exception
        /// if there is no handler registered for the command.
        /// </summary>
        /// <typeparam name="TCommand"></typeparam>
        /// <param name="c"></param>
        public void SendCommand<TCommand>(TCommand c)
        {
            if (commandHandlers.ContainsKey(typeof(TCommand)))
                commandHandlers[typeof(TCommand)](c);
            else
                throw new Exception("No command handler registered for " + typeof(TCommand).Name);
        }

        /// <summary>
        /// Publishes the specified event to all of its subscribers.
        /// </summary>
        /// <param name="e"></param>
        private void PublishEvent(object e)
        {
            var eventType = e.GetType();
            if (eventSubscribers.ContainsKey(eventType))
                foreach (var sub in eventSubscribers[eventType])
                    sub(e);
        }

        /// <summary>
        /// Registers an aggregate as being the handler for a particular
        /// command.
        /// </summary>
        /// <typeparam name="TAggregate"></typeparam>
        /// <param name="handler"></param>
        public void AddHandlerFor<TCommand, TAggregate>()
            where TAggregate : Aggregate, new()
        {
            if (commandHandlers.ContainsKey(typeof(TCommand)))
                throw new Exception("Command handler already registered for " + typeof(TCommand).Name);
            
            commandHandlers.Add(typeof(TCommand), c =>
                {
                    // Create an empty aggregate.
                    var agg = new TAggregate();

                    // Load the aggregate with events.
                    agg.Id = ((dynamic)c).Id;
                    agg.ApplyEvents(eventStore.LoadEventsFor<TAggregate>(agg.Id.ToString()));
                    
                    // With everything set up, we invoke the command handler, collecting the
                    // events that it produces.
                    var resultEvents = new List<IEvent>();
                    foreach (var e in (agg as IHandleCommand<TCommand>).Handle((TCommand)c))
                        resultEvents.Add(e);
                    
                    // Store the events in the event store.
                    if (resultEvents.Count > 0)
                        eventStore.SaveEventsFor<TAggregate>(agg.Id.ToString(),
                            agg.EventsLoaded, resultEvents);

                    // Publish them to all subscribers.
                    foreach (var e in resultEvents)
                        PublishEvent(e);
                });
        }

        /// <summary>
        /// Adds an object that subscribes to the specified event, by virtue of implementing
        /// the ISubscribeTo interface.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="subscriber"></param>
        public void AddSubscriberFor<TEvent>(ISubscribeTo<TEvent> subscriber)
        {
            if (!eventSubscribers.ContainsKey(typeof(TEvent)))
                eventSubscribers.Add(typeof(TEvent), new List<Action<object>>());
            eventSubscribers[typeof(TEvent)].Add(e =>
                subscriber.Handle((TEvent)e));
        }

        /// <summary>
        /// Looks through the specified assembly for all public types that implement
        /// the IHandleCommand or ISubscribeTo generic interfaces. Registers each of
        /// the implementations as a command handler or event subscriber.
        /// </summary>
        /// <param name="ass"></param>
        public void ScanAssembly(Assembly ass)
        {
            // Scan for and register handlers.
            var handlers = 
                from t in ass.DefinedTypes
                from i in t.ImplementedInterfaces
                where i.IsConstructedGenericType
                where i.GetGenericTypeDefinition() == typeof(IHandleCommand<>)
                let args = i.GenericTypeArguments
                select new
                {
                    CommandType = args[0],
                    AggregateType = t.AsType()
                };
            foreach (var h in handlers)
                this.GetType().GetTypeInfo().DeclaredMethods.Single(t=>t.Name.Equals("AddHandlerFor"))
                    .MakeGenericMethod(new Type[] {h.CommandType, h.AggregateType})
                    .Invoke(this, new object[] { });

            // Scan for and register subscribers.
            var subscriber =
                from t in ass.DefinedTypes
                from i in t.ImplementedInterfaces
                where i.IsConstructedGenericType
                where i.GetGenericTypeDefinition() == typeof(ISubscribeTo<>)
                select new
                {
                    Type = t,
                    EventType = i.GenericTypeArguments[0]
                };
            foreach (var s in subscriber)
            {
                Type typ = s.Type.AsType();
                object instance = CreateInstanceOf(typ);
                this.GetType().GetTypeInfo().DeclaredMethods.Single(t => t.Name.Equals("AddSubscriberFor"))
                    .MakeGenericMethod(s.EventType)
                    .Invoke(this, new object[] { instance });
                if (typ.GetTypeInfo().BaseType.Name.Equals("AbstractBaseProjection"))
                {
                    //we have our instance of the projection
                    //now we need to call the GetWriter method on the doc store to get the defined writer for the type defined SetWriter call on the projection
                    var m = docStore.GetType().GetTypeInfo().DeclaredMethods.Single(t => t.Name.Equals("GetWriter"));
                    var x = m.GetGenericArguments();
                    var m2 = instance.GetType().GetTypeInfo().DeclaredMethods.Single(t => t.Name.Equals("SetWriter"));
                    var x2 = m2.GetParameters()[0].ParameterType.GenericTypeArguments;
                    MethodInfo method = m.MakeGenericMethod(new Type[] { x2[0], x2[1] });

                    var writer = method.Invoke(docStore, new object[] { });
                    m2.Invoke(instance, new object[] { writer });
                }
            }
        }

        /// <summary>
        /// Looks at the specified object instance, examples what commands it handles
        /// or events it subscribes to, and registers it as a receiver/subscriber.
        /// </summary>
        /// <param name="instance"></param>
        public void ScanInstance(object instance)
        {
            // Scan for and register handlers.
            var handlers =
                from i in instance.GetType().GetTypeInfo().ImplementedInterfaces
                where i.IsConstructedGenericType
                where i.GetGenericTypeDefinition() == typeof(IHandleCommand<>)
                let args = i.GenericTypeArguments
                select new
                {
                    CommandType = args[0],
                    AggregateType = instance.GetType()
                };
            foreach (var h in handlers)
                this.GetType().GetTypeInfo().DeclaredMethods.Single(t => t.Name.Equals("AddHandlerFor"))
                    .MakeGenericMethod(h.CommandType, h.AggregateType)
                    .Invoke(this, new object[] { });

            // Scan for and register subscribers.
            var subscriber =
                from i in instance.GetType().GetTypeInfo().ImplementedInterfaces
                where i.IsConstructedGenericType
                where i.GetGenericTypeDefinition() == typeof(ISubscribeTo<>)
                select i.GenericTypeArguments[0];
            foreach (var s in subscriber)
                this.GetType().GetTypeInfo().DeclaredMethods.Single(t=>t.Name.Equals("AddSubscriberFor"))
                    .MakeGenericMethod(s)
                    .Invoke(this, new object[] { instance });
        }

        /// <summary>
        /// Creates an instance of the specified type. If you are using some kind
        /// of DI container, and want to use it to create instances of the handler
        /// or subscriber, you can plug it in here.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private object CreateInstanceOf(Type t)
        {
            return Activator.CreateInstance(t);
        }
    }
}
