#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public class SwitchCase<T>
    {
        public Predicate<T> Predicate
        {
            get;
            private set;
        }

        public T SelectedValue
        {
            get;
            private set;
        }   
    }

    public interface ISwitchCaseBuilder
    {
    }

    public class SwitchCaseBuilder : ISwitchCaseBuilder
    {

    }

    public class SwitchCaseObservable<T> : IObservable<T>
    {
        public IDisposable Subscribe(IObserver<T> observer)
        {
            throw new NotImplementedException();
        }
    }
}
