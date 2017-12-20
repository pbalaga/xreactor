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
    public interface IReactiveObject : IRaisePropertyChanged
    {
        IReactor Reactor { get; }
    }

    public static class ReactiveObjectExtensions
    {
        public static Property<T> Create<T>(this IReactiveObject reactive, string name, T defaultValue = default(T))
        {
            return reactive.Reactor.Create<T>(name, defaultValue);
        }

        public static Property<T> Create<T>(this IReactiveObject reactive, string name, Expression<Func<T>> valueExpression)
        {
            return reactive.Reactor.Create<T>(name, valueExpression);
        }

        public static Property<T> Create<T>(this IReactiveObject reactive, Expression<Func<T>> nameExpression, T defaultValue = default(T))
        {
            return reactive.Reactor.Create<T>(nameExpression, defaultValue);
        }

        public static Property<T> Create<T>(this IReactiveObject reactive, Expression<Func<T>> nameExpression, Expression<Func<T>> valueExpression)
        {
            return reactive.Reactor.Create<T>(nameExpression, valueExpression);
        }

        public static LazyProperty<T> CreateLazy<T>(this IReactiveObject reactive, string name, Expression<Func<T>> valueExpression)
        {
            return reactive.Reactor.CreateLazy<T>(name, valueExpression);
        }

        public static LazyProperty<T> CreateLazy<T>(this IReactiveObject reactive, Expression<Func<T>> nameExpression, Expression<Func<T>> valueExpression)
        {
            return reactive.Reactor.CreateLazy<T>(nameExpression, valueExpression);
        }
    }
}
