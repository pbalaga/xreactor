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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    delegate INotifyCollectionChanged FluentCollectionMethod(INotifyCollectionChanged input);
    delegate INotifyCollectionChanged FluentParametricCollectionMethod<ParameterT>(INotifyCollectionChanged input, ParameterT parameter);
    delegate SubjectT FluentParametricMethod<SubjectT, ParameterT>(SubjectT input, ParameterT parameter);
    delegate INotifyPropertyChanged FluentTargetMethod(INotifyPropertyChanged input);
    delegate TraversalOptions MarkerMethodEffect(TraversalOptions options);

    static class MarkerMethods
    {
        internal class Delegates
        {
            public static Func<TraversalOptions, LambdaExpression[], TraversalOptions> TrackItems
            {
                get { return MarkerMethods.TrackItems; }
            }

            public static MarkerMethodEffect TrackItemsAllProperties
            {
                get { return MarkerMethods.TrackItems; }
            }

            public static Func<TraversalOptions, TraversalOptions> TrackLastChild
            {
                get { return MarkerMethods.TrackLastChild; }
            }

            public static Func<TraversalOptions, TraversalOptions> TrackFields
            {
                get { return MarkerMethods.TrackFields; }
            }

            public static Func<TraversalOptions, TraversalOptions> DoNotTrackLastChild
            {
                get { return MarkerMethods.DoNotTrackLastChild; }
            }

            public static Func<TraversalOptions, TraversalOptions> DoNotTrackFields
            {
                get { return MarkerMethods.DoNotTrackFields; }
            }
        }

        public static TraversalOptions TrackItems(TraversalOptions options, PropertyTrackingInfo propertiesTracked)
        {
            options.CollectionTrackOptions = CollectionTrackOptions.TrackItems;
            options.Properties = propertiesTracked;
            options.TrackLastChild = true; //this is usually required by the logic of expressions
            return options;
        }

        public static TraversalOptions TrackItems(TraversalOptions options, string[] propertiesTracked)
        {
            return TrackItems(options, PropertyTrackingInfo.FromStringArray(propertiesTracked));
        }

        public static TraversalOptions TrackItems<TItem>(TraversalOptions options, Expression<Func<TItem, object>>[] propertiesTracked)
        {
            //The line beneath is also a place, where the array of tracked properties is copied (it must be,
            //so that tracked properties cannot be changed from outside).
            string[] propertiesTrackedAsStrings = propertiesTracked.Select(ExpressionHelper.GetNameFromExpression).ToArray();
            return TrackItems(options, propertiesTrackedAsStrings);
        }

        public static TraversalOptions TrackItems(TraversalOptions options, LambdaExpression[] propertiesTracked)
        {
            //The line beneath is also a place, where the array of tracked properties is copied (it must be,
            //so that tracked properties cannot be changed from outside).
            string[] propertiesTrackedAsStrings = propertiesTracked.Select(ExpressionHelper.GetNameFromUntypedParameterExpression).ToArray();
            return TrackItems(options, propertiesTrackedAsStrings);
        }

        public static TraversalOptions TrackItems(TraversalOptions options)
        {
            return TrackItems(options, PropertyTrackingInfo.TrackAll);
        }

        public static TraversalOptions TrackLastChild(TraversalOptions options)
        {
            options.TrackLastChild = true;
            return options;
        }

        public static TraversalOptions TrackFields(TraversalOptions options)
        {
            options.TrackFields = true;
            return options;
        }

        public static TraversalOptions DoNotTrackLastChild(TraversalOptions options)
        {
            options.TrackLastChild = false;
            return options;
        }

        public static TraversalOptions DoNotTrackFields(TraversalOptions options)
        {
            options.TrackFields = false;
            return options;
        }
    }

    class MarkerMethodsRegistry
    {
        readonly static object[] emptyArgs = new object[0];

        IDictionary<MethodInfo, Delegate> register = new Dictionary<MethodInfo, Delegate>();

        public void Register(MethodInfo methodInfo, Delegate markerEffectApplier)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");
            if (markerEffectApplier == null)
                throw new ArgumentNullException("markerEffectApplier");

            register[methodInfo] = markerEffectApplier;
        }

        public void Apply(MethodInfo methodCalledInExpression, ref TraversalOptions options, IEnumerable<object> arguments)
        {
            if (!ApplyIfRegistered(methodCalledInExpression, ref options, arguments))
                throw new ArgumentException("Specified marker method is not registered",
                    "methodCalledInExpression");
        }

        public bool ApplyIfRegistered(MethodInfo methodCalledInExpression, ref TraversalOptions options, IEnumerable<object> arguments)
        {
            //Delegate effect;
            //if (register.TryGetValue(methodCalledInExpression, out effect))
            //    options = InvokeMarkerMethod(effect, options, arguments);
            foreach (var pair in register)
            {
                if (AreMethodInfosPointingTheSameMethod(pair.Key, methodCalledInExpression))
                {
                    Delegate effect = pair.Value;
                    options = InvokeMarkerMethod(effect, options, arguments);
                    return true;
                }
            }
            return false;
        }

        public bool ApplyIfRegistered(MethodInfo methodCalledInExpression, ref TraversalOptions options, IEnumerable<Expression> argumentExpressions)
        {
            //Skip the first argument, which is the 'this' parameter of a marker extensio method.
            var arguments = argumentExpressions.Skip(1).Materialize();
            return ApplyIfRegistered(methodCalledInExpression, ref options, arguments);
        }

        private TraversalOptions InvokeMarkerMethod(Delegate effect, TraversalOptions inputOptions, IEnumerable<object> arguments)
        {
            var allArguments = Enumerable.Concat(new object[] { inputOptions }, arguments ?? emptyArgs).ToArray();
            return (TraversalOptions)effect.DynamicInvoke(allArguments);
        }

        private bool AreMethodInfosPointingTheSameMethod(MethodInfo methodA,
            MethodInfo methodB)
        {
            return methodA.MetadataToken == methodB.MetadataToken
                && methodA.Module == methodB.Module;
        }
    }

}
