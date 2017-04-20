//----------------------------------------------------------------------- 
// <copyright file="IApplyEvent.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dizzle.Cqrs.Core
{
    /// <summary>
    /// Implemented by an aggregate once for each event type it can apply.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IApplyEvent<TEvent>
    {
        void Apply(TEvent e);
    }
}
