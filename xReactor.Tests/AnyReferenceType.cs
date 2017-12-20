#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Tests
{
    /// <summary>
    /// Precisely any reference type, not implementing
    /// custom Equals method override, specifically 
    /// not a string.
    /// </summary>
    class AnyReferenceType
    {
        public double Angle;

        public AnyReferenceType(double angle)
        {
            this.Angle = angle;
        }
    }
}
