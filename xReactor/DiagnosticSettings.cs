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
    /// Defines possible actions tha can be undertaken in
    /// case an exception occurs in the OnNext method
    /// of the observer.
    /// </summary>
    public enum ExceptionHandlingPolicy
    {
        /// <summary>
        /// The subscription that appears to cause an error
        /// will be turned off with as little influence on
        /// other subscriptions as possible.
        /// </summary>
        TurnOffTheSubscription,
        /// <summary>
        /// When an excpetion is caught within a subscription,
        /// it will be rethrown to enable app-level handling
        /// by the user. If there is no specific exception
        /// handling logic, this will most probably cause the 
        /// application to crash.
        /// </summary>
        FailFast,
    }

    public class DiagnosticSettings
    {
        /// <summary>
        /// Defines a private thread synchronization object for the <see cref="T:DiagnosticSettings"/> class.
        /// </summary>
        private static readonly object synchronizationObject = new object();

        private static DiagnosticSettings custom;

        public static DiagnosticSettings Custom
        {
            get
            {
                lock (synchronizationObject)
                {
                    if (custom == null)
                        custom = new DiagnosticSettings();
                    return custom;
                }
            }
        }

        private ExceptionHandlingPolicy exceptionHandlingPolicy;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:DebuggingSettings"/> class.
        /// </summary>
        public DiagnosticSettings()
        {
            Reset();
        }

        /// <summary>
        /// Determines what behavior should be undertaken in
        /// case an exception occurs in the OnNext method
        /// of the observer.
        /// </summary>
        public ExceptionHandlingPolicy ExceptionHandlingPolicy
        {
            get { return exceptionHandlingPolicy; }
            set
            {
                if (!Enum.IsDefined(typeof(ExceptionHandlingPolicy), value))
                    throw new ArgumentException("Value is not defined in the enumeration", "value");

                exceptionHandlingPolicy = value;
            }
        }

        internal void Reset()
        {
            this.exceptionHandlingPolicy = xReactor.ExceptionHandlingPolicy.FailFast;
        }

    }
}
