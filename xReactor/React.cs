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
using System.Reactive.Linq;
using System.Reactive;

namespace xReactor
{
    /// <summary>
    /// Provides entry point to the fluent interface.
    /// </summary>
    public static class React
    {
        public static ExpressionPreconfiguration Preconfig()
        {
            return new ExpressionPreconfiguration();
        }

        public static IObservableExpression<T> To<T>(Expression<Func<T>> expressionToBeChanged)
        {
            return new ObservableExpression<T>(expressionToBeChanged);
        }

        public static IObservableExpression<T> To<T>(
            this ExpressionPreconfiguration preconfiguration,
            Expression<Func<T>> expressionToBeChanged)
        {
            return new ObservableExpression<T>(expressionToBeChanged, preconfiguration);
        }

        public static IObservableExpression<T> LazilyTo<T>(Expression<Func<T>> expressionToBeChanged)
        {
            throw new NotImplementedException();
        }

        public static IObservableExpression<T> LazilyTo<T>(
            this ExpressionPreconfiguration preconfiguration,
            Expression<Func<T>> expressionToBeChanged)
        {
            throw new NotImplementedException();
        }

        internal static IObservable<Unit> WhenAnyPropertyChangesOrNever(params UsedPropertyChain[] properties)
        {
            if (properties.Any())
                return WhenAnyPropertyChanges(properties);
            else
                return Observable.Never<Unit>();
        }

        internal static IObservable<Unit> WhenAnyPropertyChanges(params UsedPropertyChain[] properties)
        {
            if (!properties.Any())
            {
                throw new ArgumentException(
                    "Collection of provided properties is empty, so " +
                    "no notifications would be propagated.", "properties"
                    );
            }
            var changeStream = properties.Select(p =>
                Observable.FromEvent(
                h => p.Changed += h,
                h => p.Changed -= h)).Merge();
            return changeStream;
        }

    }
}
