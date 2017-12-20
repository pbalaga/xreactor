#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public interface IProperty
    {
        IReactor Reactor { get; }
        Type Type { get; }
        object Value { get; }
        string Name { get; }
    }

    public interface IProperty<T> : IProperty
    {
        T Value { get; set; }
    }

    abstract public class PropertyBase<T> : IProperty<T>
    {
        abstract public T Value { get; set; }

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the owner of this property.
        /// </summary>
        /// <value>The owner of this property.</value>
        public IReactor Reactor
        {
            get;
            private set;
        }

        public Type Type
        {
            get { return typeof(T); }
        }

        object IProperty.Value
        {
            get { return this.Value; }
        }

        private IList<IRequirement<T>> Requirements
        {
            get;
            set;
        }

        public PropertyBase(IReactor owner, string name)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            this.Reactor = owner;
            this.Name = name;
            this.Requirements = new List<IRequirement<T>>();
        }

        protected void RaisePropertyChangeNotifications(T oldValue, T newValue)
        {
            Reactor.RaisePropertyChanged(this, oldValue, newValue);
        }

        public IObservable<T> GetStream()
        {
            return Observable.FromEvent<IProperty>(
                h => Reactor.RxPropertyChanged += h,
                h => Reactor.RxPropertyChanged -= h).
                Where(prop => prop == this).Select(prop => (T)prop.Value);
        }

        /// <summary>
        /// Applies a requirement to the value of the current property.
        /// If it is not met, when the property's value is set, an exception
        /// is thrown.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="failureMessage"></param>
        /// <returns></returns>
        public PropertyBase<T> Require(Predicate<T> condition, string failureMessage)
        {
            var requirement = new LambdaRequirement<T>(condition, failureMessage, null);
            AddRequirement(requirement);
            return this;
        }

        /// <summary>
        /// Applies a requirement to the value of the current property.
        /// If it is not met, when the property's value is set,
        /// the value is coerced into an acceptable domain.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="coercion"></param>
        /// <param name="logMessage"></param>
        /// <returns></returns>
        public PropertyBase<T> Coerce(Func<T, T> coercion,
            string logMessage = "")
        {
            if (coercion == null)
                throw new ArgumentNullException("coercion");

            //The condition is set to always false, because coercion must
            //be run in all cases (for every value, even if it is correct).
            var requirement = new LambdaRequirement<T>(v => false, logMessage, coercion);
            AddRequirement(requirement);
            return this;
        }

        private void AddRequirement(IRequirement<T> requirement)
        {
            this.Requirements.Add(requirement);
            this.ValidateInitialValue();
        }

        internal void ValidateInitialValue()
        {
            this.Value = Validate(this.Value);
        }

        protected T Validate(T value)
        {
            foreach (var requirement in Requirements)
            {
                IValidationResult result = requirement.Validate(value);
                if (!result.IsValid)
                {
                    if (requirement.IsSilent)
                    {
                        value = requirement.Coerce(value);
                        Debug.WriteLine("On property {0} {1} on instance {2} was coerced: {3} changed into {4} " +
                            "with message: {5}.",
                            this.Type, this.Name, this.Reactor.Target, this.Value, value, result.Message);
                    }
                    else
                    {
                        string msg = string.Format("Cannot set the value {3} on property {0} {1} on instance {2} " +
                            "because: ", this.Type, this.Name, this.Reactor.Target, this.Value);
                        throw new ArgumentException(msg, "value");
                    }
                }
            }
            return value;
        }

    }

    public class Property<T> : PropertyBase<T>
    {
        T backingValue;

        public Property(IReactor owner, string name)
            : base(owner, name)
        {
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>The property value.</value>
        public override T Value
        {
            get { return backingValue; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(backingValue, value))
                {
                    value = Validate(value);
                    T oldValue = backingValue;
                    RaisePropertyChangeNotifications(oldValue, backingValue = value);
                }
            }
        }

    }

    public class LazyProperty<T> : PropertyBase<T>
    {
        T backingValue;

        public LazyProperty(IReactor owner, string name, Func<T> valueGetter)
            : base(owner, name)
        {
            if (valueGetter == null)
                throw new ArgumentNullException("valueGetter");
            this.ValueGetter = valueGetter;
        }

        public bool IsValueUpToDate
        {
            get;
            private set;
        }

        public Func<T> ValueGetter
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>The property value.</value>
        public override T Value
        {
            get
            {
                if (!IsValueUpToDate)
                    backingValue = ValueGetter();
                return backingValue;
            }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(backingValue, value))
                {
                    value = Validate(value);
                    T oldValue = backingValue;
                    IsValueUpToDate = true;
                    RaisePropertyChangeNotifications(oldValue, backingValue = value);
                }
            }
        }

        public void Invalidate()
        {
            IsValueUpToDate = false;
        }
    }
}
