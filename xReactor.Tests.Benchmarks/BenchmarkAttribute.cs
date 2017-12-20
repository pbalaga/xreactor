#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Tests.Benchmarks
{
    [AttributeUsage(AttributeTargets.Method)]
    class BenchmarkAttribute:Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:BenchmarkAttribute"/> class.
        /// </summary>
        public BenchmarkAttribute()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:BenchmarkAttribute"/> class.
        /// </summary>
        public BenchmarkAttribute(string title = null)
        {
            this.Title = title;
        }

        public string Title
        {
            get;
            set;
        }
    }
}
