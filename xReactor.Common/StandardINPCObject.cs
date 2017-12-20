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
    /// <summary>
    /// Object that implements the <see cref="INotifyPropertyChanged"/> 
    /// in a standard way - without introducing reactive concepts.
    /// This class is used to test whether such objects can be 
    /// integrated with xReactor as easily as reactive properties.
    /// This is especially important for working with legacy systems
    /// or third-party libraries.
    /// </summary>
    abstract public class StandardINPCObject : NotifyPropertyChangedBase
    {
      
    }

    /// <summary>
    /// Imagine this class is located in an external third-party
    /// library.
    /// </summary>
    public class ProductInfo : StandardINPCObject
    {
        private bool isAvailable;

        /// <summary>
        /// Is the product currently available in the shop?
        /// </summary>
        public bool IsAvailable
        {
            get { return isAvailable; }
            set
            {
                if (isAvailable != value)
                {
                    isAvailable = value;
                    RaisePropertyChanged(() => IsAvailable);
                }
            }
        }
    }
}
