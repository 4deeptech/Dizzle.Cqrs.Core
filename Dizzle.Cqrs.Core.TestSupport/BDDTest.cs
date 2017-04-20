//----------------------------------------------------------------------- 
// <copyright file="BDDTest.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using NUnit.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Dizzle.Cqrs.Core.TestSupport
{
    /// <summary>
    /// Provides infrastructure for a set of tests on a given aggregate.
    /// </summary>
    /// <typeparam name="TAggregate"></typeparam>
    public class BDDTest<TAggregate>
        where TAggregate : Aggregate, new()
    {
        private TAggregate sut;

        [SetUp]
        public void BDDTestSetup()
        {
            sut = new TAggregate();
        }

        protected void Test(IEnumerable<IEvent> given, Func<TAggregate, object> when, Action<object> then)
        {
            then(when(ApplyEvents(sut, given)));
        }

        protected IEnumerable<IEvent> Given(List<IEvent> events)
        {
            return events as IEnumerable<IEvent>;
        }

        protected Func<TAggregate, object> When<TCommand>(TCommand command)
        {
            return agg =>
            {
                try
                {
                    return DispatchCommand(command).Cast<object>().ToArray();
                }
                catch (Exception e)
                {
                    return e;
                }
            };
        }

        protected Action<object> Then(params object[] expectedEvents)
        {
            return got =>
            {
                var gotEvents = got as object[];
                if (gotEvents != null)
                {
                    if (gotEvents.Length == expectedEvents.Length)
                        for (var i = 0; i < gotEvents.Length; i++)
                            if (gotEvents[i].GetType() == expectedEvents[i].GetType())
                                Assert.AreEqual(Serialize(expectedEvents[i]), Serialize(gotEvents[i]));
                            else
                                Assert.Fail(string.Format(
                                    "Incorrect event in results; expected a {0} but got a {1}",
                                    expectedEvents[i].GetType().Name, gotEvents[i].GetType().Name));
                    else if (gotEvents.Length < expectedEvents.Length)
                        Assert.Fail(string.Format("Expected event(s) missing: {0}",
                            string.Join(", ", EventDiff(expectedEvents, gotEvents))));
                    else
                        Assert.Fail(string.Format("Unexpected event(s) emitted: {0}",
                            string.Join(", ", EventDiff(gotEvents, expectedEvents))));
                }
                else if (got is CommandHandlerNotDefinedException)
                    Assert.Fail((got as Exception).Message);
                else
                    Assert.Fail("Expected events, but got exception {0}",
                        got.GetType().Name);
            };
        }

        private string[] EventDiff(object[] a, object[] b)
        {
            var diff = a.Select(e => e.GetType().Name).ToList();
            foreach (var remove in b.Select(e => e.GetType().Name))
                diff.Remove(remove);
            return diff.ToArray();
        }

        protected Action<object> ThenFailWith<TException>()
        {
            return got =>
            {
                if (got is TException)
                    Assert.Pass("Got correct exception type");
                else if (got is CommandHandlerNotDefinedException)
                    Assert.Fail((got as Exception).Message);
                else if (got is Exception)
                    Assert.Fail(string.Format(
                        "Expected exception {0}, but got exception {1}",
                        typeof(TException).Name, got.GetType().Name));
                else
                    Assert.Fail(string.Format(
                        "Expected exception {0}, but got event result",
                        typeof(TException).Name));
            };
        }

        private IEnumerable<IEvent> DispatchCommand<TCommand>(TCommand c)
        {
            var handler = sut as IHandleCommand<TCommand>;
            if (handler == null)
                throw new CommandHandlerNotDefinedException(string.Format(
                    "Aggregate {0} does not yet handle command {1}",
                    sut.GetType().Name, c.GetType().Name));
            return handler.Handle(c);
        }

        private TAggregate ApplyEvents(TAggregate agg, IEnumerable<IEvent> events)
        {
            agg.ApplyEvents(events);
            return agg;
        }

        private string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private class CommandHandlerNotDefinedException : Exception
        {
            public CommandHandlerNotDefinedException(string msg) : base(msg) { }
        }
    }
}
