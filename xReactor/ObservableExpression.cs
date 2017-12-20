#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public interface IContextfulObservableExpression<out T> { }

    public interface ILazyObservableExpression<out T> : IObservable<Func<T>>
    {
        IObservableExpression<T> AsData();
    }

    public interface IObservableExpression<out T> : IObservable<T>
    {
        IObservable<Unit> ChangeStream { get; }
        //Expression<Func<T>> ObservedExpression { get; }
        Func<T> ObservedExpressionCompiled { get; }
        ILazyObservableExpression<T> AsLazy();
    }

    /// <summary>
    /// A wrapper class that forwards subscription to an internal
    /// <see cref="IObservable{T}"/> object, but additionally
    /// implements <see cref="IObservableExpression{T}"/>
    /// allowing for customized setup and use of tailored
    /// extension methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ObservableExpression<T> : IObservableExpression<T>
    {
        class LazyChannel : ILazyObservableExpression<T>
        {
            ObservableExpression<T> owner;

            /// <summary>
            /// Initializes a new instance of the <see cref="T:LazyChannel"/> class.
            /// </summary>
            public LazyChannel(ObservableExpression<T> owner)
            {
                this.owner = owner;
            }

            public IDisposable Subscribe(IObserver<Func<T>> observer)
            {
                IDisposable subscription = this.owner.lazyPropertyChangeStream.Subscribe(observer);
                ObservableExpression.PushValueInUpToDateContext(() => observer.OnNext(this.owner.observedExpressionCompiled));
                return subscription;
            }

            public IObservableExpression<T> AsData()
            {
                return owner;
            }
        }

        private IObservable<Unit> changeStream;
        private IObservable<T> propertyChangeStream;
        private IObservable<Func<T>> lazyPropertyChangeStream;
        private UsedPropertyChain[] properties;
        private Func<T> observedExpressionCompiled;
        private Expression<Func<T>> observedExpression;

        private LazyChannel lazyChannel;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ObservableExpression"/> class.
        /// </summary>
        public ObservableExpression(Expression<Func<T>> expressionToBeChanged)
            : this(expressionToBeChanged, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ObservableExpression"/> class.
        /// </summary>
        internal ObservableExpression(Expression<Func<T>> expressionToBeChanged,
            ExpressionPreconfiguration preconfiguration)
        {
            if (expressionToBeChanged == null)
                throw new ArgumentNullException("expressionToBeChanged");

            this.observedExpression = expressionToBeChanged;
            this.observedExpressionCompiled = expressionToBeChanged.Compile();
            this.properties = ExpressionHelper.GetUsedProperties(expressionToBeChanged, preconfiguration, true);
            this.changeStream = React.WhenAnyPropertyChanges(properties);
            this.propertyChangeStream = changeStream.Select(unit => this.observedExpressionCompiled());
            this.lazyPropertyChangeStream = changeStream.Select(unit => this.observedExpressionCompiled);
        }

        public ILazyObservableExpression<T> AsLazy()
        {
            if (lazyChannel == null)
                lazyChannel = new LazyChannel(this);
            return lazyChannel;
        }

        IObservable<Unit> IObservableExpression<T>.ChangeStream
        {
            get { return this.changeStream; }
        }

        //Expression<Func<T>> IObservableExpression<T>.ObservedExpression
        //{
        //    get { return this.observedExpression; }
        //}

        Func<T> IObservableExpression<T>.ObservedExpressionCompiled
        {
            get { return this.observedExpressionCompiled; }
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            //First, subscribe the observer, so that property handlers are attached.
            //This is important in case of a cyclic reference. Without handlers in
            //place cyclic referencing would not occur, even though it should.

            //Then, publish a value immediately, so that an initial value can be set.
            //Notice the current value is only passed to the new observer.
            //Subscribers present until now are NOT given this value again.

            IDisposable subscription = this.propertyChangeStream.Subscribe(observer);

            ObservableExpression.PushValueInUpToDateContext(() => observer.OnNext(this.observedExpressionCompiled()));
            //try
            //{
            //observer.OnNext(this.observedExpressionCompiled());
            //}
            //catch (Exception ex)
            //{
            //}
            return subscription;
        }

        //IDisposable IObservable<Func<T>>.Subscribe(IObserver<Func<T>> observer)
        //{
        //    IDisposable subscription = this.lazyPropertyChangeStream.Subscribe(observer);
        //    //try
        //    observer.OnNext(this.observedExpression);
        //    return subscription;
        //}
    }

    //class LazyObservableExpression<T> : ILazyObservableExpression<T>
    //{
    //    public IDisposable Subscribe(IObserver<Lazy<T>> observer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


    /// <summary>
    /// Provides extension methods for the 
    /// <see cref="IObservableExpression{T}"/> instances.
    /// </summary>
    public static class ObservableExpression
    {
        #region Specialized subscription

        class FailingFastObserver<T> : IObserver<T>
        {
            private Action<T> setter;

            public FailingFastObserver(Action<T> setter)
            {
                if (setter == null)
                    throw new ArgumentNullException("setter");

                this.setter = setter;
            }

            public void OnCompleted()
            {
                //Underlying stream of property changes should never end in this sense.
            }

            public void OnError(Exception error)
            {
                //TODO: log this
                throw new SubscriptionFailedException(error);
            }

            public void OnNext(T value)
            {
                setter(value);
            }
        }

        internal static void PushValueInUpToDateContext(Action pushAction)
        {
            PushValueInSpecifiedContext(pushAction, DiagnosticSettings.Custom.ExceptionHandlingPolicy);
        }

        internal static void PushValueInSpecifiedContext(Action pushAction, ExceptionHandlingPolicy exceptionHandlingPolicy)
        {
            if (exceptionHandlingPolicy == xReactor.ExceptionHandlingPolicy.FailFast)
                //this is already handled by the fail fast observer
                pushAction();
            else
            {
                try
                {
                    pushAction();
                }
                catch (Exception inner)
                {
                    if (inner is SubscriptionFailedException)
                        throw;
                    else
                        throw new SubscriptionFailedException(inner);
                }
            }
        }

        #endregion

        #region Set / SetAndNotify

        public static IDisposable Set<T>(
            this IObservable<T> expression,
            Action<T> setter)
        {
            if (DiagnosticSettings.Custom.ExceptionHandlingPolicy == ExceptionHandlingPolicy.FailFast)
                return expression.SubscribeSafe(new FailingFastObserver<T>(setter));
            else
            {
                ExceptionHandlingPolicy context = ExceptionHandlingPolicy.TurnOffTheSubscription;
                return expression.Subscribe(value => PushValueInSpecifiedContext(() => setter(value), context));
            }
        }

        private static IDisposable Set<T>(
            this IObservable<T> expression,
            Expression<Func<T>> propertyToSetExpression,
            Action customChangeNotification)
        {
            Action<T> setter = ExpressionHelper.MakeSetter<T>(propertyToSetExpression);

            if (customChangeNotification == null)
                return expression.Set(setter);
            else
            {
                Action<T> notifyingSetter = (value) => { setter(value); customChangeNotification(); };
                return expression.Set(notifyingSetter);
            }
        }

        public static IDisposable Set<T>(
            this IObservable<T> expression,
            Expression<Func<T>> propertyToSetExpression)
        {
            return expression.Set(propertyToSetExpression, null);
        }

        public static IDisposable SetAndNotify<T>(
            this IObservable<T> expression,
            Expression<Func<T>> propertyToSetExpression)
        {
            UsedPropertyChain property = ExpressionHelper.GetUsedPropertiesAndAttachListeners(propertyToSetExpression).
                Single();

            Action notification = () => CompatibilitySettings.NotifyPropertyChainChanged(property);
            return expression.Set(propertyToSetExpression, notification);
        }

        public static IDisposable SetAndNotify<T>(
            this IObservable<T> expression,
            Expression<Func<T>> propertyToSetExpression,
            Action customNotification)
        {
            if (customNotification == null)
                throw new ArgumentNullException("customNotification");

            return expression.Set(propertyToSetExpression, customNotification);
        }

        #endregion

        #region Switch

        public static IObservable<TOut> Switch<TIn, TOut>(
            this IObservableExpression<TIn> expression,
            params Predicate<TIn>[] cases)
        {
            throw new NotImplementedException();
        }

        public static IObservable<TOut> Switch<TIn, TOut>(
            this IObservableExpression<TIn> expression,
            TIn @case,
            TOut then, TOut @else)
        {
            return expression.Select(value => object.Equals(@case, value) ? then : @else);
        }

        public static IObservable<TOut> Switch<TIn, TOut>(
            this IObservableExpression<TIn> expression,
            TIn @case,
            Func<TOut> then, Func<TOut> @else)
        {
            return expression.Select(value => object.Equals(@case, value) ? then() : @else());
        }

        public static IObservable<TOut> Switch<TIn, TOut>(
            this IObservableExpression<TIn> expression,
            Predicate<TIn> @case,
            TOut then, TOut @else)
        {
            return expression.Select(value => @case(value) ? then : @else);
        }

        public static IObservable<TOut> Switch<TIn, TOut>(
            this IObservableExpression<TIn> expression,
            Predicate<TIn> @case,
            Func<TOut> then, Func<TOut> @else)
        {
            return expression.Select(value => @case(value) ? then() : @else());
        }

        #endregion

        #region SkipInitial

        /// <summary>
        /// Skips the first, initial value published the observable
        /// expression. Set or Subscribe will be called only after
        /// the next change to objects used in the expression tracked.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static IObservable<T> SkipInitial<T>(
            this IObservableExpression<T> expression)
        {
            return expression.Skip(1);
        }

        #endregion

        #region Delay

        public static IObservableExpression<T> DelayListeners<T>(
            this IObservableExpression<T> expression)
        {
            throw new NotImplementedException();
        }

        public static IObservableExpression<T> DoNotDelayListeners<T>(
            this IObservableExpression<T> expression)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Marker Methods

        /// <summary>
        /// Marker extension method that is recognized by the
        /// expression tree parser. Marks a collection object
        /// that all contained items implementing INPC should
        /// be tracked as well.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns>The same object as input</returns>
        public static T TrackItems<T>(
            this T collection)
            where T : class, INotifyCollectionChanged
        {
            return collection;
        }

        /// <summary>
        /// Marker extension method that is recognized by the
        /// expression tree parser. Marks a collection object
        /// that all contained items implementing INPC should
        /// be tracked as well.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="collection"></param>
        /// <returns>The same object as input</returns>
        public static IEnumerable<TItem> TrackItems<TItem>(
            this IEnumerable<TItem> collection,
            params Expression<Func<TItem, object>>[] propertiesToTrack)
        {
            return collection;
        }

        /// <summary>
        /// Marker extension method that is recognized by the
        /// expression tree parser. Marks a notifiable INPC object
        /// that changes applied to any subproperty of the last 
        /// child property occuring in an expression chain should 
        /// be tracked as well. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns>The same object as input</returns>
        public static T TrackLastChild<T>(
            this T target)
            where T : class, INotifyPropertyChanged
        {
            return target;
        }

        /// <summary>
        /// Marker extension method that is recognized by the
        /// expression tree parser. Marks a notifiable INPC object
        /// that changes applied to any subproperty of the last 
        /// child property occuring in an expression chain should 
        /// NOT be tracked as well. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T DoNotTrackLastChild<T>(
            this T target)
            where T : class, INotifyPropertyChanged
        {
            return target;
        }

        /// <summary>
        /// Marks all subsequent fields as trackable,
        /// given that they implement the 
        /// <see cref="INotifyPropertyChanged"/> interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T TrackFields<T>(
            this T target)
            where T : class, INotifyPropertyChanged
        {
            return target;
        }

        /// <summary>
        /// Marks all subsequent fields as NOT trackable,
        /// given that they implement the 
        /// <see cref="INotifyPropertyChanged"/> interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T DoNotTrackFields<T>(
            this T target)
            where T : class, INotifyPropertyChanged
        {
            return target;
        }

        #endregion

        #region AsProperty

        public interface IPropertyStub<T>
        {
            [EditorBrowsable(EditorBrowsableState.Never)]
            string Name { get; }

            [EditorBrowsable(EditorBrowsableState.Never)]
            IObservable<T> ValueStream { get; }

            [EditorBrowsable(EditorBrowsableState.Never)]
            bool IsLazy { get; }
        }

        class PropertyStub<T> : IPropertyStub<T>
        {
            public PropertyStub(string name, IObservable<T> valueStream, bool isLazy)
            {
                this.Name = name;
                this.ValueStream = valueStream;
                this.IsLazy = isLazy;
            }

            public string Name
            {
                get;
                private set;
            }

            public IObservable<T> ValueStream
            {
                get;
                private set;
            }

            public bool IsLazy
            {
                get;
                private set;
            }
        }

        public static IPropertyStub<T> AsProperty<T>(this IObservable<Lazy<T>> stream, Expression<Func<T>> nameExpression)
        {
            throw new NotImplementedException();
            //string name = ExpressionHelper.GetNameFromExpression<T>(nameExpression);
            //return new PropertyStub<T>(name, stream);
        }

        public static IPropertyStub<T> ViaProperty<T>(this IObservable<T> stream, Expression<Func<T>> nameExpression)
        {
            string name = ExpressionHelper.GetNameFromExpression<T>(nameExpression);
            return new PropertyStub<T>(name, stream, false);
        }

        public static IPropertyStub<T> ViaProperty<T>(this IObservable<T> stream, string name)
        {
            return new PropertyStub<T>(name, stream, false);
        }

        public static IPropertyStub<T> ViaLazyProperty<T>(this IObservableExpression<T> stream, Expression<Func<T>> nameExpression)
        {
            string name = ExpressionHelper.GetNameFromExpression<T>(nameExpression);
            return new PropertyStub<T>(name, stream, true);
        }

        public static IPropertyStub<T> ViaLazyProperty<T>(this IObservableExpression<T> stream, string name)
        {
            return new PropertyStub<T>(name, stream, true);
        }

        public static IProperty<T> On<T>(this IPropertyStub<T> propertyStub, IReactiveObject reactive)
        {
            PropertyBase<T> property;

            if (propertyStub.IsLazy)
            {
                var root = (IObservableExpression<T>)propertyStub.ValueStream;
                property = reactive.Reactor.CreateLazy<T>(propertyStub.Name, root.ChangeStream, root.ObservedExpressionCompiled);
            }
            else
            {
                property = reactive.Create<T>(propertyStub.Name);
                propertyStub.ValueStream.Set(value => property.Value = value);
            }
            return property;
        }

        #endregion
    }
}
