#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public class ReactiveBase : NotifyPropertyChangedBase, IReactiveObject, ILoadable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReactiveBase"/> class.
        /// </summary>
        public ReactiveBase()
        {
            this.Reactor = new Reactor(this, RaisePropertyChanged, null);
        }

        #region Creation methods / Reactive properties stuff

        public IReactor Reactor
        {
            get;
            private set;
        }

        #endregion

        #region ILoadable Implementation

        virtual public void Load()
        {
            
        }

        virtual public void Unload()
        {
            
        } 

        #endregion

    }
}
