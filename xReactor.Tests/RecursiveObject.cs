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
    /// Resursive test class that contains all sensible 
    /// member types that could be used in expressions.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RecursiveObject<T> : NotifyPropertyChangedBase
    {
        public T FieldA;
        public T FieldB;
        public RecursiveObject<T> InnerField;

        public T PropertyA
        {
            get { return FieldA; }
            set
            {
                if (!object.ReferenceEquals(FieldA, value))
                {
                    FieldA = value;
                    RaisePropertyChanged(() => PropertyA);
                }
            }
        }

        public T PropertyB
        {
            get { return FieldB; }
            set
            {
                if (!object.ReferenceEquals(FieldB, value))
                {
                    FieldB = value;
                    RaisePropertyChanged(() => PropertyB);
                }
            }
        }

        public RecursiveObject<T> Inner
        {
            get { return InnerField; }
            set
            {
                if (InnerField != value)
                {
                    InnerField = value;
                    RaisePropertyChanged(() => Inner);
                    RaisePropertyChanged(() => InnerAsFunc);
                }
            }
        }

        public Func<RecursiveObject<T>> InnerAsFunc
        {
            get { return () => Inner; }
        }

        public T MethodA()
        {
            return FieldA;
        }

        public T MethodB()
        {
            return FieldB;
        }

        public RecursiveObject<T> GetInner()
        {
            return this.Inner;
        }

        public RecursiveObject<T> FluentMethod()
        {
            return this;
        }

        public static RecursiveObject<T> CreateOfDepth(int depth)
        {
            if (depth < 0)
                throw new ArgumentOutOfRangeException("depth", "Must be non-negative");
            RecursiveObject<T> topMostParent = new RecursiveObject<T>();
            RecursiveObject<T> currentParent = topMostParent;
            while (depth-- > 0)
            {
                currentParent.Inner = new RecursiveObject<T>();
                currentParent = currentParent.Inner;
            }

            return topMostParent;
        }
    }
}
