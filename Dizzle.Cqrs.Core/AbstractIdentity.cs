//----------------------------------------------------------------------- 
// <copyright file="AbstractIdentity.cs" company="4Deep Technologies LLC"> 
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
    /// <summary>
    /// Base implementation of <see cref="IIdentity"/>, which implements
    /// equality and ToString once and for all.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public abstract class AbstractIdentity<TKey> : IIdentity
    {
        public abstract TKey Id { get; protected set; }

        public string GetId()
        {
            return Id.ToString();
        }

        public abstract string GetTag();

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            var identity = obj as AbstractIdentity<TKey>;

            if (identity != null)
            {
                return Equals(identity);
            }

            return false;
        }

        public override string ToString()
        {
            return string.Format("{0}-{1}", GetType().Name.Replace("Id", ""), Id);
        }

        public override int GetHashCode()
        {
            return (Id.GetHashCode());
        }

        public int GetStableHashCode()
        {
            // same as hash code, but works across multiple architectures 
            var type = typeof(TKey);
            if (type == typeof(string))
            {
                return CalculateStringHash(Id.ToString());
            }
            return Id.GetHashCode();
        }

        static AbstractIdentity()
        {
            var type = typeof(TKey);
            if (type == typeof(int) || type == typeof(long) || type == typeof(uint) || type == typeof(ulong))
                return;
            if (type == typeof(Guid) || type == typeof(string))
                return;
            throw new InvalidOperationException("Abstract identity inheritors must provide stable hash. It is not supported for:  " + type);
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

        public bool Equals(AbstractIdentity<TKey> other)
        {
            if (other != null)
            {
                return other.Id.Equals(Id) && other.GetTag() == GetTag();
            }

            return false;
        }

        public static bool operator ==(AbstractIdentity<TKey> left, AbstractIdentity<TKey> right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AbstractIdentity<TKey> left, AbstractIdentity<TKey> right)
        {
            return !Equals(left, right);
        }
    }
}
