#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public interface IReactor
    {
        object Target { get; }
        event Action<IProperty> RxPropertyChanged;
        PropertyChangedEventHandler PropertyChangedHandler { get; }
        PropertyChangingEventHandler PropertyChangingHandler { get; }
        void RaisePropertyChanged<T>(IProperty property, T oldValue, T newValue);
    }

    /// <summary>
    /// The heart of xReactor responsible for tracking
    /// property changes and invoking update logic.
    /// </summary>
    public class Reactor : IReactor
    {
        public event Action<IProperty> RxPropertyChanged;

        public PropertyChangedEventHandler PropertyChangedHandler
        {
            get;
            private set;
        }

        public PropertyChangingEventHandler PropertyChangingHandler
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the target object for which the reactor is created.
        /// </summary>
        /// <value>The target object for which the reactor is created.</value>
        public object Target
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Reactor"/> class.
        /// </summary>
        public Reactor(object target, PropertyChangedEventHandler propertyChangedHandler,
            PropertyChangingEventHandler propertyChangingHandler)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            this.Target = target;
            this.PropertyChangedHandler = propertyChangedHandler;
            this.PropertyChangingHandler = propertyChangingHandler;
        }

        internal void RaisePropertyChanged<T>(IProperty property, T oldValue, T newValue)
        {
            if (property == null)
                throw new ArgumentNullException("property");
            if (property.Reactor != this)
                throw new ArgumentException("Specified property's owner must be this instance.", "property");

            var target = Target;

            var changing = PropertyChangingHandler;
            if (changing != null)
            {
                changing(target, new PropertyChangingEventArgs(property.Name));
            }

            var changed = PropertyChangedHandler;
            if (changed != null)
            {
                changed(target, new PropertyChangedEventArgs(property.Name));
            }

            var rxChanged = RxPropertyChanged;
            if (rxChanged != null)
            {
                rxChanged(property);
            }
        }

        void IReactor.RaisePropertyChanged<T>(IProperty property, T oldValue, T newValue)
        {
            this.RaisePropertyChanged<T>(property, oldValue, newValue);
        }
    }

    public static class ReactorExtensions
    {
        public static Property<T> Create<T>(this IReactor reactor, string name, T defaultValue = default(T))
        {
            return new Property<T>(reactor, name) { Value = defaultValue };
        }

        internal static Property<T> Create<T>(this IReactor reactor, string name, Func<T> valueFunc, params IProperty[] fromProperties)
        {
            var property = reactor.Create<T>(name, valueFunc());
            reactor.WhenPropertiesChange(fromProperties).Subscribe(coll => property.Value = valueFunc());
            return property;
        }

        public static Property<T> Create<T>(this IReactor reactor, string name, Expression<Func<T>> valueExpression)
        {
            Func<T> compiled = valueExpression.Compile();

            //Get value from compiled now to provide the default value:
            var property = reactor.Create<T>(name, compiled());

            UsedPropertyChain[] properties = ExpressionHelper.GetUsedPropertiesAndAttachListeners(valueExpression);
            React.WhenAnyPropertyChanges(properties).Subscribe(unit => property.Value = compiled());

            return property;
        }

        public static LazyProperty<T> CreateLazy<T>(this IReactor reactor, string name, Expression<Func<T>> valueExpression)
        {
            Func<T> compiled = valueExpression.Compile();

            UsedPropertyChain[] properties = ExpressionHelper.GetUsedPropertiesAndAttachListeners(valueExpression);
            IObservable<Unit> changeStream = React.WhenAnyPropertyChanges(properties);
            return CreateLazy<T>(reactor, name, changeStream, compiled);
        }

        internal static LazyProperty<T> CreateLazy<T>(this IReactor reactor, string name, IObservable<Unit> changeStream, Func<T> valueGetter)
        {
            var property = new LazyProperty<T>(reactor, name, valueGetter);
            changeStream.Subscribe(unit => property.Invalidate());

            return property;
        }

        public static Property<T> Create<T>(this IReactor reactor, Expression<Func<T>> nameExpression, T defaultValue = default(T))
        {
            string propertyName = ExpressionHelper.GetNameFromExpression(nameExpression);
            return reactor.Create<T>(propertyName, defaultValue);
        }

        public static Property<T> Create<T>(this IReactor reactor, Expression<Func<T>> nameExpression, Expression<Func<T>> valueExpression)
        {
            string propertyName = ExpressionHelper.GetNameFromExpression(nameExpression);
            return reactor.Create<T>(propertyName, valueExpression);
        }

        public static LazyProperty<T> CreateLazy<T>(this IReactor reactor, Expression<Func<T>> nameExpression, Expression<Func<T>> valueExpression)
        {
            string propertyName = ExpressionHelper.GetNameFromExpression(nameExpression);
            return reactor.CreateLazy<T>(propertyName, valueExpression);
        }

        public static IObservable<T> WhenPropertyChanges<T>(this IReactor reactor, IProperty property)
        {
            var stream = Observable.FromEvent<IProperty>(
                h => reactor.RxPropertyChanged += h,
                h => reactor.RxPropertyChanged -= h);

            return stream.Where(p => p == property).Select(p => (T)p.Value);
        }

        public static IObservable<object> WhenPropertiesChange(this IReactor reactor, params IProperty[] properties)
        {
            var stream = Observable.FromEvent<IProperty>(
                h => reactor.RxPropertyChanged += h,
                h => reactor.RxPropertyChanged -= h);

            return stream.Where(p => Array.IndexOf(properties, p) >= 0);
        }
    }
}
