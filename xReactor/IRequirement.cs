#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public interface IValidationResult
    {
        bool IsValid { get; }
        string Message { get; }
    }

    public class ValidationResult : IValidationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ValidationResult"/> class.
        /// </summary>
        public ValidationResult(bool isValid, string message)
        {
            this.IsValid = isValid;
            this.Message = message;
        }

        public bool IsValid
        {
            get;
            private set;
        }

        public string Message
        {
            get;
            private set;
        }
    }

    public interface IRequirement<T>
    {
        IValidationResult Validate(T value);
        T Coerce(T value);

        /// <summary>
        /// Determines whether the system should NOT generate an
        /// exception whenever the validation fails.
        /// If set to false, an exception will be thrown.
        /// If set to true, no exception will be generated and the value
        /// should be coerced into a valid domain instead.
        /// </summary>
        bool IsSilent { get; }
    }

    class LambdaRequirement<T> : IRequirement<T>
    {
        private Predicate<T> condition;
        private string failureMessage;
        private Func<T, T> coercion;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:LambdaRequirement"/> class.
        /// </summary>
        public LambdaRequirement(Predicate<T> condition, string failureMessage, Func<T, T> coercion)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            this.condition = condition;
            this.failureMessage = failureMessage;
            this.coercion = coercion;
        }

        public IValidationResult Validate(T value)
        {
            bool isOK = condition(value);
            return new ValidationResult(isOK, isOK ? string.Empty : failureMessage);
        }

        public T Coerce(T value)
        {
            if (this.coercion == null)
                throw new InvalidOperationException("Coercion not set. " +
                    "This requirement does not support coercion. Exception must " +
                    "be thrown instead.");
            return this.coercion(value);
        }

        public bool IsSilent
        {
            get { return this.coercion != null; }
        }
    }

}
