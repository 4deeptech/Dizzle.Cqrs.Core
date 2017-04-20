//----------------------------------------------------------------------- 
// <copyright file="IDocumentWriter.cs" company="4Deep Technologies LLC"> 
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
    public enum AddOrUpdateHint
    {
        ProbablyExists,
        ProbablyDoesNotExist
    }


    /// <summary>
    /// View writer interface, used by the event handlers
    /// </summary>
    /// <typeparam name="TEntity">The type of the view.</typeparam>
    /// <typeparam name="TKey">type of the key</typeparam>
    public interface IDocumentWriter<in TKey, TEntity>
    {
        TEntity AddOrUpdate(TKey key, Func<TEntity> addFactory, Func<TEntity, TEntity> update, AddOrUpdateHint hint = AddOrUpdateHint.ProbablyExists);
        bool TryDelete(TKey key);
    }
}
