//----------------------------------------------------------------------- 
// <copyright file="DocumentStrategy.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Dizzle.Cqrs.Core.Storage
{
    static class NameCache<T>
    {
        // ReSharper disable StaticFieldInGenericType
        public static readonly string Name;
        public static readonly string Namespace;
        // ReSharper restore StaticFieldInGenericType
        static NameCache()
        {
            var type = typeof(T);

            Name = new string(Splice(type.Name).ToArray()).TrimStart('-');
            var dcs = type.GetTypeInfo().CustomAttributes.OfType<DataContractAttribute>().ToArray();


            if (dcs.Length <= 0) return;
            var attribute = dcs.First();

            if (!string.IsNullOrEmpty(attribute.Name))
            {
                Name = attribute.Name;
            }

            if (!string.IsNullOrEmpty(attribute.Namespace))
            {
                Namespace = attribute.Namespace;
            }
        }

        static IEnumerable<char> Splice(string source)
        {
            foreach (var c in source)
            {
                if (char.IsUpper(c))
                {
                    yield return '-';
                }
                yield return char.ToLower(c);
            }
        }
    }

    public sealed class ViewStrategy : IDocumentStrategy
    {
        public string GetEntityBucket<TEntity>()
        {
            return Conventions.ViewsFolder + "\\" + NameCache<TEntity>.Name;
        }

        public string GetEntityLocation<TEntity>(object key)
        {
            if (key is unit)
                return NameCache<TEntity>.Name + ".json";

            var hashed = key as IIdentity;
            if (hashed != null)
            {
                var stableHashCode = hashed.GetStableHashCode();
                var b = (byte)((uint)stableHashCode % 251);
                return b + "\\" + hashed.GetTag() + "-" + hashed.GetId() + ".json";
            }
            if (key is Guid)
            {
                var b = (byte)((uint)((Guid)key).GetHashCode() % 251);
                return b + "\\" + key.ToString().ToLowerInvariant() + ".json";
            }
            if (key is string)
            {
                var corrected = ((string)key).ToLowerInvariant().Trim();
                var b = (byte)((uint)CalculateStringHash(corrected) % 251);
                return b + "\\" + corrected + ".json";
            }
            return key.ToString().ToLowerInvariant() + ".json";
        }

        static int CalculateStringHash(string value)
        {
            if (value == null) return 42;
            unchecked
            {
                var hash = 23;
                foreach (var c in value)
                {
                    hash = hash * 31 + c;
                }
                return hash;
            }
        }
        public void Serialize<TEntity>(TEntity entity, Stream stream)
        {
            // ProtoBuf must have non-zero files
            stream.WriteByte(42);
            StreamWriter writer = new StreamWriter(stream);
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            JsonSerializer ser = new JsonSerializer();
            ser.NullValueHandling = NullValueHandling.Ignore;
            ser.Formatting = Formatting.Indented;
            ser.Serialize(jsonWriter, entity);
            jsonWriter.Flush();
        }

        public TEntity Deserialize<TEntity>(Stream stream)
        {
            var signature = stream.ReadByte();

            if (signature != 42)
                throw new InvalidOperationException("Unknown view format");

            StreamReader reader = new StreamReader(stream);
            JsonTextReader jsonReader = new JsonTextReader(reader);
            JsonSerializer ser = new JsonSerializer();
            return ser.Deserialize<TEntity>(jsonReader);
        }
    }

    public sealed class DocumentStrategy : IDocumentStrategy
    {
        public void Serialize<TEntity>(TEntity entity, Stream stream)
        {
            // ProtoBuf must have non-zero files
            stream.WriteByte(42);
            StreamWriter writer = new StreamWriter(stream);
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            JsonSerializer ser = new JsonSerializer();
            ser.Serialize(jsonWriter, entity);
            jsonWriter.Flush();
        }

        public TEntity Deserialize<TEntity>(Stream stream)
        {
            var signature = stream.ReadByte();

            if (signature != 42)
                throw new InvalidOperationException("Unknown view format");

            StreamReader reader = new StreamReader(stream);
            JsonTextReader jsonReader = new JsonTextReader(reader);
            JsonSerializer ser = new JsonSerializer();
            return ser.Deserialize<TEntity>(jsonReader);
        }

        public string GetEntityBucket<TEntity>()
        {
            return Conventions.DocsFolder + "/" + NameCache<TEntity>.Name;
        }

        public string GetEntityLocation<TEntity>(object key)
        {
            if (key is unit)
                return NameCache<TEntity>.Name + ".json";

            return key.ToString().ToLowerInvariant() + ".json";
        }
    }
}
