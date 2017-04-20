//----------------------------------------------------------------------- 
// <copyright file="IDocumentStore.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dizzle.Cqrs.Core
{
    public interface IDocumentStore
    {
        IDocumentWriter<TKey, TEntity> GetWriter<TKey, TEntity>();
        IDocumentReader<TKey, TEntity> GetReader<TKey, TEntity>();
        IDocumentStrategy Strategy { get; }
        List<DocumentRecord> EnumerateContents(string bucket);
        void WriteContents(string bucket, IEnumerable<DocumentRecord> records);
        void Reset(string bucket);
        void ResetAll();
    }

    public sealed class DocumentRecord
    {
        /// <summary>
        /// Path of the view in the subfolder, using '/' as split on all platforms
        /// </summary>
        public readonly string Key;

        public readonly Func<byte[]> Read;

        public DocumentRecord(string key, Func<byte[]> read)
        {
            Key = key;
            Read = read;
        }
    }
}
