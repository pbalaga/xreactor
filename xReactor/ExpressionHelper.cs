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
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Wintellect.PowerCollections;

namespace xReactor
{
    public static class ExpressionHelper
    {
        static readonly MarkerMethodsRegistry markerMethods;

        static ExpressionHelper()
        {
            markerMethods = new MarkerMethodsRegistry();
            Delegate trackItemsAllPropertiesDelegate = (FluentCollectionMethod)ObservableExpression.TrackItems;
            markerMethods.Register(trackItemsAllPropertiesDelegate.Method, MarkerMethods.Delegates.TrackItemsAllProperties);

            Delegate trackItemsDelegate = (FluentParametricMethod<IEnumerable<object>, Expression<Func<object, object>>[]>)ObservableExpression.TrackItems;
            markerMethods.Register(trackItemsDelegate.Method, MarkerMethods.Delegates.TrackItems);

            Delegate trackLastChildDelegate = (FluentTargetMethod)ObservableExpression.TrackLastChild;
            markerMethods.Register(trackLastChildDelegate.Method, MarkerMethods.Delegates.TrackLastChild);

            Delegate trackFieldsDelegate = (FluentTargetMethod)ObservableExpression.TrackFields;
            markerMethods.Register(trackFieldsDelegate.Method, MarkerMethods.Delegates.TrackFields);

            Delegate doNotTrackLastChildDelegate = (FluentTargetMethod)ObservableExpression.DoNotTrackLastChild;
            markerMethods.Register(doNotTrackLastChildDelegate.Method, MarkerMethods.Delegates.DoNotTrackLastChild);

            Delegate doNotTrackFieldsDelegate = (FluentTargetMethod)ObservableExpression.DoNotTrackFields;
            markerMethods.Register(doNotTrackFieldsDelegate.Method, MarkerMethods.Delegates.DoNotTrackFields);
        }

        private static bool CheckForMarkerMethodCalls(MethodCallExpression methodCall, ref TraversalOptions options)
        {
            if (methodCall.Object != null && !methodCall.Method.CustomAttributes.Any(attr => attr.AttributeType == typeof(ExtensionAttribute)))
                //assume that marker methods are always static 
                //(i.e. extensions methods)
                return false;

            return markerMethods.ApplyIfRegistered(methodCall.Method, ref options, methodCall.Arguments);
        }

        private static void ApplyMarkerCall(FluentTargetMethod markerMethod, ref TraversalOptions options, IEnumerable<object> arguments = null)
        {
            markerMethods.Apply(markerMethod.Method, ref options, arguments);
        }

        internal static IEnumerable<object> Materialize(this IEnumerable<Expression> arguments)
        {
            foreach (var arg in arguments)
            {
                var argInstanceGetter = LambdaExpression.Lambda<Func<object>>(arg).Compile();
                yield return argInstanceGetter();
            }
        }

        public static object GetTargetFromExpression<T>(Expression<Func<T>> nameExpression)
        {
            try
            {
                return (ConstantExpression)GetMeaningfulBodyFromExpression(nameExpression).Expression;
            }
            catch (Exception inner)
            {
                throw BuildInvalidNameExpressionException(inner, "()=>PropertyName");
            }
        }

        public static string GetNameFromExpression<T>(Expression<Func<T>> nameExpression)
        {
            return GetNameFromExpression(nameExpression, "()=>PropertyName");
        }

        public static string GetNameFromExpression<TObject, TProperty>(Expression<Func<TObject, TProperty>> nameExpression)
        {
            return GetNameFromUntypedParameterExpression(nameExpression);
        }

        internal static string GetNameFromUntypedParameterExpression(LambdaExpression nameExpression)
        {
            return GetNameFromExpression(nameExpression, "(obj)=>obj.PropertyName");
        }

        private static string GetNameFromExpression(LambdaExpression nameExpression, string expectedFormat)
        {
            try
            {
                return GetMeaningfulBodyFromExpression(nameExpression).Member.Name;
            }
            catch (Exception inner)
            {
                throw BuildInvalidNameExpressionException(inner, expectedFormat);
            }
        }

        private static MemberExpression GetMeaningfulBodyFromExpression(LambdaExpression nameExpression)
        {
            MemberExpression body = nameExpression.Body as MemberExpression;

            if (body == null)
            {
                UnaryExpression ubody = (UnaryExpression)nameExpression.Body;
                body = ubody.Operand as MemberExpression;
            }

            return body;
        }

        private static Exception BuildInvalidNameExpressionException(Exception innerReason, string expectedFormat)
        {
            string message = string.Format("Must be an expression in form of \"{0}\".", expectedFormat);
            throw new ArgumentException(message, "nameExpression", innerReason);
        }

        /// <summary>
        /// Given an function expression of form: '() => PropertyName', converts it
        /// to an action expression of form: '(value) => PropertyName = value'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyToSetExpression"></param>
        /// <returns></returns>
        internal static Action<T> MakeSetter<T>(Expression<Func<T>> propertyToSetExpression)
        {
            try
            {
                var valueParameter = Expression.Parameter(typeof(T), "value");
                var expr = Expression.Assign(propertyToSetExpression.Body, valueParameter);
                return Expression.Lambda<Action<T>>(expr, valueParameter).Compile();
            }
            catch (Exception inner)
            {
                throw new ArgumentException(
                    "Expression must represent a settable member: " +
                    "property with a setter or a field.",
                    "propertyToSetExpression",
                    inner);
            }
        }

        private static Func<INotifyPropertyChanged> CompileObjectFactoryExpressionINPC(Expression expr)
        {
            return Expression.Lambda(expr).Compile() as Func<INotifyPropertyChanged>;
        }

        private static Func<object> CompileMemberExpression(MemberExpression expr)
        {
            return Expression.Lambda(expr).Compile() as Func<object>;
        }

        private static Func<INotifyPropertyChanged> CompileMemberExpressionINPC(MemberExpression expr)
        {
            var compiled = Expression.Lambda(expr).Compile();
            return compiled as Func<INotifyPropertyChanged>;
        }

        public static UsedPropertyChain[] GetUsedProperties<T>(Expression<Func<T>> valueExpression)
        {
            return GetUsedProperties<T>(valueExpression, null, false);
        }

        public static UsedPropertyChain[] GetUsedPropertiesAndAttachListeners<T>(Expression<Func<T>> valueExpression)
        {
            return GetUsedProperties<T>(valueExpression, null, true);
        }

        internal static UsedPropertyChain[] GetUsedProperties<T>(
            Expression<Func<T>> valueExpression,
            ExpressionPreconfiguration preconfiguration,
            bool attachHandlersImmediately
            )
        {
            var set = new Set<UsedPropertyChain>();
            TraversalOptions options = preconfiguration != null ?
                preconfiguration.Options : TraversalOptions.Default();

            GetUsedProperties(set, valueExpression, ChildInformation.Null, options);
            UsedPropertyChain[] array = set.ToArray();
            foreach (var up in array)
            {
                up.TopLevelParent = up;
                if (attachHandlersImmediately)
                    up.EnsureHandlersAreAttached();
            }
            return array;
        }

        private static void GetUsedProperties(Set<UsedPropertyChain> container,
            MemberExpression memberExpression,
            ChildInformation child,
            TraversalOptions options)
        {
            if (memberExpression == null)
                return;
            MemberTypes memberType = memberExpression.Member.MemberType;
            if (memberType != MemberTypes.Property)
            {
                if (memberType == MemberTypes.Field)
                {
                    if (options.TrackFields)
                    {
                        var constant = memberExpression.Expression as ConstantExpression;
                        if (constant != null)
                        {
                            HandleFieldConstant(container, memberExpression, child, options);
                            return;
                        }

                        if (IsFieldChain(memberExpression) && child.IsTrackable)
                        {
                            //Occurs with a.b.c.A kind of expression, where
                            //the field chain is the leading part of the
                            //expression and the constant can be extracted from
                            //it. The constant is taken from evaluation of a.b.c

                            HandleFieldConstant(container, memberExpression, child, options);
                            return;
                        }
                        else
                        {
                            //Dead end. It happens, when a field occurs
                            //after a non-constant element, e.g. after 
                            //a property reference as in 'A.B.field.C'
                            //'A.B' is not ConstantExpression and therefore,
                            //even with TrackFields on, the field cannot be
                            //tracked any further. However, the preceding
                            //part of the expression can and should be 
                            //tracked.

                            //Break the chain (without continuation)
                            GetUsedProperties(container, memberExpression.Expression, ChildInformation.FieldBreak, options);
                            return;
                        }
                    }
                    else
                    {
                        //Break the chain (without continuation)
                        GetUsedProperties(container, memberExpression.Expression, ChildInformation.FieldBreak, options);
                        return;
                    }
                }

                throw new InvalidOperationException("Unhandled case. Revise the code");
            }

            var parentMember = memberExpression.Expression as MemberExpression;
            if (parentMember != null)
            {
                UsedSubproperty usedSubProperty = GetContinuationChainLink(memberExpression, child, options);
                var usedChild = new ChildInformation(usedSubProperty);
                //Continue the chain
                GetUsedProperties(container, parentMember, usedChild, options);
            }
            else
            {
                if (memberExpression.Expression is ParameterExpression)
                {
                    //Presumably, we are within a nested lambda parameter block.
                    //Skip, just for now. If there's a nested lambda like "c=>c.property" it 
                    //is pretty complicated to know when to update.
                    //TODO: 
                    return;
                }

                var constant = memberExpression.Expression as ConstantExpression;
                if (constant != null)
                {
                    var notifiable = constant.Value as INotifyPropertyChanged;
                    if (notifiable != null)
                    {
                        string propertyName = memberExpression.Member.Name;
                        var chain = new UsedPropertyChain(notifiable, propertyName, child.Property, options);
                        chain.TargetRetriever = CompileMemberExpression(memberExpression);
                        container.Add(chain);
                    }
                    else
                    {
                        //Even now add a chain to the container. Fields are not traced, but their
                        //properties can be. This applies only to cases where a the left-hand part
                        //of an expression is a field or a field/constant chain.
                        HandleFieldConstant(container, memberExpression, child, options);
                    }
                    return;
                }
                else
                {
                    UsedSubproperty usedSubProperty = GetContinuationChainLink(memberExpression, child, options);
                    var usedChild = new ChildInformation(usedSubProperty);

                    //This should break the notification chain or continue, if it's marker method.
                    GetUsedProperties(container, memberExpression.Expression, usedChild, options);
                }
            }
        }

        private static bool IsFieldChain(MemberExpression memberExpression)
        {
            Expression current = memberExpression;
            while (true)
            {
                if (current is ConstantExpression || IsStaticField(memberExpression))
                    return true;
                memberExpression = current as MemberExpression;
                if (memberExpression == null || memberExpression.Member.MemberType != MemberTypes.Field)
                    return false;

                current = memberExpression.Expression;
            }
        }

        private static bool IsStaticField(MemberExpression memberExpression)
        {
            FieldInfo fieldInfo = memberExpression.Member as FieldInfo;
            return fieldInfo != null && fieldInfo.IsStatic;
        }

        private static UsedSubproperty GetContinuationChainLink(
            MemberExpression memberExpression,
            ChildInformation child,
            TraversalOptions options)
        {
            Func<INotifyPropertyChanged> parentRetriever =
                                   CompileObjectFactoryExpressionINPC(memberExpression.Expression);
            if (parentRetriever == null)
            {
                //This happens when parentMember refers to a non-observable collection 
                return null;
            }
            var usedSub = new UsedSubproperty(memberExpression.Member.Name, parentRetriever,
                child.Property, options);
            usedSub.TargetRetriever = CompileMemberExpression(memberExpression);
            return usedSub;
        }

        private static void HandleFieldConstant(Set<UsedPropertyChain> container,
            MemberExpression memberExpression,
            ChildInformation child,
            TraversalOptions options)
        {
            Func<object> constantObjectRetriever = CompileMemberExpression(memberExpression);
            HandleFieldConstant(container, constantObjectRetriever, child, options);
        }

        private static void HandleFieldConstant(Set<UsedPropertyChain> container,
            Func<object> constantObjectRetriever,
            ChildInformation child,
            TraversalOptions options)
        {
            INotifyPropertyChanged notifiable;
            Func<object> targetRetriever;

            if (child.Property != null)
            {
                //This occurs in case of expression like:
                //React.To(() => recursive.PropertyA).Set(...);
                //or with marker methods:
                //React.To(() => recursive.TrackFields().PropertyA).Set(...);
                notifiable = child.Property.GetCurrentParent();
                targetRetriever = constantObjectRetriever;
            }
            else
            {
                //This case occurs, when a method is called on a tracked field, e.g.:
                //React.To(() => recursive.TrackFields().GetInner().PropertyA).Set(...);
                //The call to GetInner() breaks the chain and there is no tracked child
                //for the 'recursive' object. However, this is still a viable case; 
                //if recursive is INPC, track all properties on it, otherwise track nothing.
                Func<object> parentRetriever = constantObjectRetriever;
                notifiable = parentRetriever() as INotifyPropertyChanged;
                targetRetriever = parentRetriever;
            }

            if (notifiable != null && child.IsTrackable) //property and method children are trackable
            {
                //Do not allow fields here, because they cannot be tracked anyhow.

                //string.Empty here as the property name does not mean all properties
                //of the associated object will be tracked. It only means that associated
                //object is stored in a field and changes to that field cannot be tracked,
                //just because it's not a property. Thus, string.Empty indicates lack
                //of any property name.
                var chain = new UsedPropertyChain(notifiable, string.Empty, child.Property, options);
                chain.TargetRetriever = targetRetriever;
                container.Add(chain);
            }

            //Otherwise, track nothing
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="expression"></param>
        /// <param name="options"></param>
        /// <param name="continuedChild">The sub-chain that might be broken by
        /// a marker method call.</param>
        private static void GetUsedProperties(Set<UsedPropertyChain> container,
            Expression expression, ChildInformation continuedChild, TraversalOptions options)
        {
            if (expression == null)
                return; //e.g. in case of static method calls object is null

            var lambda = expression as LambdaExpression;
            if (lambda != null)
            {
                GetUsedProperties(container, lambda.Body, continuedChild, options);
                return;
            }

            var member = expression as MemberExpression;
            if (member != null)
            {
                GetUsedProperties(container, member, continuedChild, options);
                return;
            }

            if (expression is ConstantExpression)
            {
                //Not traceable, nothing to do / case handled elsewhere
                return;
            }

            var binary = expression as BinaryExpression;
            if (binary != null)
            {
                GetUsedProperties(container, binary.Left, continuedChild, options);
                GetUsedProperties(container, binary.Right, continuedChild, options);
                return;
            }

            var typeBinaryExpression = expression as TypeBinaryExpression;
            if (typeBinaryExpression != null) //is operator
            {
                GetUsedProperties(container, typeBinaryExpression.Expression, continuedChild, options);
                return;
            }

            var unary = expression as UnaryExpression;
            if (unary != null)
            {
                GetUsedProperties(container, unary.Operand, continuedChild, options);
                return;
            }

            if (expression is ParameterExpression)
            {
                //Presumably, we are within a nested lambda parameter block.
                //Skip, just for now. If there's a nested lambda like "c=>c.property" it 
                //is pretty complicated to know when to update.
                //TODO: 
                return;
            }

            var methodCall = expression as MethodCallExpression;
            if (methodCall != null)
            {
                IEnumerable<Expression> arguments;
                bool isMarkerMethod = CheckForMarkerMethodCalls(methodCall, ref options);
                if (!isMarkerMethod) //required by expression logic
                {
                    //ApplyMarkerCall(ObservableExpression.TrackLastChild, ref options);

                    //disallow continuation if it is not a marker method
                    continuedChild = ChildInformation.MethodBreak;
                    //take all arguments
                    arguments = methodCall.Arguments;
                }
                else
                {
                    //Take only the first argument, which is the 'this' parameter
                    //of the (marker) extension method. Do *not* further process other
                    //arguments of marker methods, because they can be also expressions
                    //in general, but as for now their meaning is totally different than
                    //in React.To(...).
                    arguments = methodCall.Arguments.Take(1);
                }

                GetUsedProperties(container, methodCall.Object, continuedChild, options);
                foreach (var arg in arguments)
                {
                    GetUsedProperties(container, arg, continuedChild, options);
                }
                return;
            }

            var conditional = expression as ConditionalExpression;
            if (conditional != null)
            {
                GetUsedProperties(container, conditional.Test, continuedChild, options);
                GetUsedProperties(container, conditional.IfFalse, continuedChild, options);
                GetUsedProperties(container, conditional.IfTrue, continuedChild, options);
                return;
            }

            var invocation = expression as InvocationExpression;
            if (invocation != null)
            {
                continuedChild = ChildInformation.MethodBreak;

                GetUsedProperties(container, invocation.Expression, continuedChild, options);
                foreach (var arg in invocation.Arguments)
                {
                    GetUsedProperties(container, arg, continuedChild, options);
                }
                return;
            }

            //var newArray = expression as NewArrayExpression;
            //if (newArray != null)
            //{
            //    return;
            //}

            string msg = string.Format(
                    "Expressions of type {0} are not supported.", expression.GetType());
            throw new NotSupportedException(msg);
        }

    }
}
