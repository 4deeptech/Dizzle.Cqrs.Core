//----------------------------------------------------------------------- 
// <copyright file="FileStorageDocumentStore.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using Dizzle.Cqrs.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dizzle.Cqrs.Core.Storage
{
    public sealed class FileStorageDocumentStore : IDocumentStore
    {
        readonly string _folderPath;
        readonly IDocumentStrategy _strategy;

        public FileStorageDocumentStore(string folderPath, IDocumentStrategy strategy)
        {
            _folderPath = folderPath;
            _strategy = strategy;
        }

        public override string ToString()
        {
            return new Uri(Path.GetFullPath(_folderPath)).AbsolutePath;
        }

        readonly HashSet<Tuple<Type, Type>> _initialized = new HashSet<Tuple<Type, Type>>();

        public IDocumentWriter<TKey, TEntity> GetWriter<TKey, TEntity>()
        {
            var container = new FileStorageDocumentReaderWriter<TKey, TEntity>(_folderPath, _strategy);
            if (_initialized.Add(Tuple.Create(typeof(TKey), typeof(TEntity))))
            {
                container.InitIfNeeded();
            }
            return container;
        }

        public IDocumentReader<TKey, TEntity> GetReader<TKey, TEntity>()
        {
            return new FileStorageDocumentReaderWriter<TKey, TEntity>(_folderPath, _strategy);
        }

        public IDocumentStrategy Strategy
        {
            get { return _strategy; }
        }


        public List<DocumentRecord> EnumerateContents(string bucket)
        {
            List<DocumentRecord> contents = new List<DocumentRecord>();
            var full = Path.Combine(_folderPath, bucket);
            var dir = new DirectoryInfo(full);
            if (!dir.Exists) 
                return contents;

            var fullFolder = dir.FullName;
            foreach (var info in dir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                var fullName = info.FullName;
                var path = fullName.Remove(0, fullFolder.Length + 1).Replace(Path.DirectorySeparatorChar, '/');
                contents.Add( new DocumentRecord(path, () => File.ReadAllBytes(fullName)));
            }
            return contents;
        }

        public void WriteContents(string bucket, IEnumerable<DocumentRecord> records)
        {
            var buck = Path.Combine(_folderPath, bucket);
            if (!Directory.Exists(buck))
                Directory.CreateDirectory(buck);
            foreach (var pair in records)
            {
                var recordPath = Path.Combine(buck, pair.Key);

                var path = Path.GetDirectoryName(recordPath) ?? "";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                File.WriteAllBytes(recordPath, pair.Read());
            }
        }

        public void ResetAll()
        {
            if (Directory.Exists(_folderPath))
                Directory.Delete(_folderPath, true);
            Directory.CreateDirectory(_folderPath);
        }

        public void Reset(string bucket)
        {
            var path = Path.Combine(_folderPath, bucket);
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            Directory.CreateDirectory(path);
        }
    }
}
