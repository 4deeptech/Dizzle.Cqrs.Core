//----------------------------------------------------------------------- 
// <copyright file="InMemoryDocumentStore.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dizzle.Cqrs.Core.Storage
{
    public sealed class MemoryDocumentStore : IDocumentStore
    {
        ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>> _store;
        readonly IDocumentStrategy _strategy;

        public MemoryDocumentStore(ConcurrentDictionary<string, ConcurrentDictionary<string, byte[]>> store, IDocumentStrategy strategy)
        {
            _store = store;
            _strategy = strategy;
        }

        public IDocumentWriter<TKey, TEntity> GetWriter<TKey, TEntity>()
        {
            var bucket = _strategy.GetEntityBucket<TEntity>();
            var store = _store.GetOrAdd(bucket, s => new ConcurrentDictionary<string, byte[]>());
            return new MemoryDocumentReaderWriter<TKey, TEntity>(_strategy, store);
        }


        public void WriteContents(string bucket, IEnumerable<DocumentRecord> records)
        {
            var pairs = records.Select(r => new KeyValuePair<string, byte[]>(r.Key, r.Read())).ToArray();
            _store[bucket] = new ConcurrentDictionary<string, byte[]>(pairs);
        }

        public void ResetAll()
        {
            _store.Clear();
        }

        public void Reset(string bucketNames)
        {
            ConcurrentDictionary<string, byte[]> deletedValue;
            _store.TryRemove(bucketNames, out deletedValue);
        }


        public IDocumentReader<TKey, TEntity> GetReader<TKey, TEntity>()
        {
            var bucket = _strategy.GetEntityBucket<TEntity>();
            var store = _store.GetOrAdd(bucket, s => new ConcurrentDictionary<string, byte[]>());
            return new MemoryDocumentReaderWriter<TKey, TEntity>(_strategy, store);
        }

        public IDocumentStrategy Strategy
        {
            get { return _strategy; }
        }

        public List<DocumentRecord> EnumerateContents(string bucket)
        {
            var store = _store.GetOrAdd(bucket, s => new ConcurrentDictionary<string, byte[]>());
            return store.Select(p => new DocumentRecord(p.Key, () => p.Value)).ToList();
        }
    }

}
