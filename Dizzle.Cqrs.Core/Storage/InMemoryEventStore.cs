//----------------------------------------------------------------------- 
// <copyright file="InMemoryEventStore.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using System;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;

namespace Dizzle.Cqrs.Core
{
    public class StorageStream
    {
        public List<StorageFrame> Events;
        public long Version { get; set; }
    }

    public sealed class StorageFrame
    {
        public List<IEvent> Events;
        public long Version { get; set; }

        public StorageFrame()
        {
        }

        public StorageFrame(IEnumerable<IEvent> events, long version)
        {
            Events = events.ToList();
            Version = version;
        }
    }

    public class InMemoryEventStore : IEventStore
    {
        private ConcurrentDictionary<string, StorageStream> store =
            new ConcurrentDictionary<string, StorageStream>();

        public IEnumerable<IEvent> LoadEventsFor<TAggregate>(string id)
        {
            // Get the current event stream; note that we never mutate the
            // Events array so it's safe to return the real thing.
            StorageStream s;
            if (store.TryGetValue(id, out s))
                return s.Events.OrderBy(t => t.Version).SelectMany(t => t.Events);
            else
                return new List<IEvent>();
        }

        public IEnumerable<IEvent> LoadEventsFor<TAggregate>(string id, long afterVersion)
        {
            // Get the current event stream; note that we never mutate the
            // Events array so it's safe to return the real thing.
            StorageStream s;
            if (store.TryGetValue(id, out s))
                return s.Events.Where(t => t.Version > afterVersion).OrderBy(t => t.Version).SelectMany(t => t.Events);
            else
                return new List<IEvent>();
        }

        public void SaveEventsFor<TAggregate>(string aggregateId, long eventsLoaded, IEnumerable<IEvent> newEvents)
        {
            // Get or create stream.
            var s = store.GetOrAdd(aggregateId, _ => new StorageStream());

            // We'll use a lock-free algorithm for the update.
            while (true)
            {
                // Read the current event list.
                var eventList = s.Events;

                // Ensure no events persisted since us.
                var prevEvents = eventList == null ? 0 : eventList.Sum(t => t.Events.Count);
                if (prevEvents != eventsLoaded)
                    throw new Exception("Concurrency conflict; cannot persist these events");

                // Create a new event list with existing ones plus our new
                // ones (making new important for lock free algorithm!)
                var newEventList = eventList == null
                    ? new List<StorageFrame>()
                    : (List<StorageFrame>)eventList.Clone();
                newEventList.Add(new StorageFrame(newEvents, eventsLoaded + 1L));

                // Try to put the new event list in place atomically.
                if (Interlocked.CompareExchange(ref s.Events, newEventList, eventList) == eventList)
                    break;
            }
        }

        private AbstractIdentity<Guid> GetAggregateIdFromEvent(object e)
        {
            var idField = e.GetType().GetTypeInfo().DeclaredFields.Single(t => t.Name.Equals("Id"));
            if (idField == null)
                throw new Exception("Event type " + e.GetType().Name + " is missing an Id field");
            return (AbstractIdentity<Guid>)idField.GetValue(e);
        }


    }

    public static class Extensions
    {
        public static List<IEvent> Clone(this List<IEvent> currentList)
        {
            List<IEvent> newList = new List<IEvent>();
            newList.AddRange(currentList);
            return newList;
        }

        public static List<StorageFrame> Clone(this List<StorageFrame> currentList)
        {
            List<StorageFrame> newList = new List<StorageFrame>();
            foreach (StorageFrame frame in currentList)
            {
                StorageFrame frm = new StorageFrame();
                frm.Version = frame.Version;
                frm.Events = frame.Events.Clone();
            }
            newList.AddRange(currentList);
            return newList;
        }
    }
}
