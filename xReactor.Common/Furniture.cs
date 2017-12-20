#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Common
{
    public class Furniture : CommonBase
    {

    }

    public class Sitable : Furniture
    {
        private Property<int> numSeatsProperty;
        public int NumSeats
        {
            get { return numSeatsProperty.Value; }
            set { numSeatsProperty.Value = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sitable"/> class.
        /// </summary>
        public Sitable(int numSeats)
        {
            numSeatsProperty = this.Create(() => NumSeats, numSeats);
        }
    }

    public class Chair : Sitable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Chair"/> class.
        /// </summary>
        public Chair()
            : base(1)
        {
        }
    }

    public class Sofa : Sitable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sofa"/> class.
        /// </summary>
        public Sofa()
            : base(4)
        {

        }
    }

}
