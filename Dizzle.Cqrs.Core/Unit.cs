//----------------------------------------------------------------------- 
// <copyright file="Unit.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dizzle.Cqrs.Core
{
    /// <summary>
    /// Equivalent to System.Void which is not allowed to be used in the code for some reason.
    /// </summary>
    [ComVisible(true)]
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct unit
    {
        public static readonly unit it = default(unit);
    }

}
