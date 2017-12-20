#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xReactor.Common;

namespace xReactor.Tests
{
    /// <summary>
    /// This is base class for xReactor test classes.
    /// It supports INPC notifications, because 
    /// top-level expression elements must be instance 
    /// properties on an object implementing INPC.
    /// </summary>
    public class ReactiveTestBase : ReactiveBase
    {
        PropertyChangedEventHandler PropertyChangedDelegate;

        protected Room CreateRoom()
        {
            var room = new Room();
            return room;
        }

        internal void ClearPropertyChangedHandlers()
        {
            PropertyChangedDelegate = null;
        }

        public override event PropertyChangedEventHandler PropertyChanged
        {
            add { PropertyChangedDelegate += value; }
            remove { PropertyChangedDelegate -= value; }
        }

        protected override void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = PropertyChangedDelegate;
            if (handler != null)
            {
                handler(sender, e);
            }
        }
    }
}

