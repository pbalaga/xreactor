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
    abstract public class NotifyPropertyChangedBase : INotifyPropertyChanged, IRaisePropertyChanged
    {
        #region INPC implementation

        virtual public event PropertyChangedEventHandler PropertyChanged;

        virtual protected void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        internal protected void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(this, e);
        }

        internal protected void RaisePropertyChanged<T>(Expression<Func<T>> nameExpression)
        {
            string propertyName = ExpressionHelper.GetNameFromExpression(nameExpression);
            RaisePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        void IRaisePropertyChanged.RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            this.RaisePropertyChanged(this, args);
        }
    }
}
