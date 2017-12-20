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
    /// <summary>
    /// Thrown when the SetAndNotify syntax is used with a property on an object, 
    /// which is not capable of notifying of property changes
    /// </summary>
    [Serializable]
    public class CannotNotifyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CannotNotifyException"/> class
        /// </summary>
        public CannotNotifyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CannotNotifyException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the error. The content of message is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
        public CannotNotifyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CannotNotifyException"/> class
        /// </summary>
        /// <param name="message">A <see cref="T:System.String"/> that describes the error. The content of message is intended to be understood by humans. The caller of this constructor is required to ensure that this string has been localized for the current system culture.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the innerException parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public CannotNotifyException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:IncompatibleViewModelException"/> class
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected CannotNotifyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
