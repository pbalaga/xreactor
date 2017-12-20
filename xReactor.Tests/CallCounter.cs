#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Tests
{
    /// <summary>
    /// Wraps a <see cref="System.Action"/> delegate
    /// and invokes it in a controlled environment
    /// to count successful and interrupted invocations.
    /// </summary>
    class CallCounter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CallCounter"/> class
        /// with a fake empty <see cref="System.Action"/> object that does nothing.
        /// Counting capabilities of this class are preserved.
        /// </summary>
        public CallCounter()
            : this(() => { })
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CallCounter"/> class.
        /// </summary>
        public CallCounter(Action action)
        {
            if (action == null)
                throw new ArgumentNullException("action");
            this.Action = action;
        }

        public Action Action
        {
            get;
            private set;
        }

        /// <summary>
        /// Total number of calls via the <see cref="TryCall"/>
        /// method on this instance that were not interrupted
        /// by an exception thrown.
        /// </summary>
        public int NumberOfCompletedCalls
        {
            get;
            private set;
        }

        /// <summary>
        /// Total number of calls via the <see cref="TryCall"/>
        /// method on this instance that failed because of an exception.
        /// </summary>
        public int NumberOfInterruptedCalls
        {
            get;
            private set;
        }

        /// <summary>
        /// Total number of calls via the <see cref="TryCall"/>
        /// method on this instance.
        /// </summary>
        public int NumberOfAttemptedCalls
        {
            get { return NumberOfCompletedCalls + NumberOfInterruptedCalls; }
        }

        /// <summary>
        /// Gets a delegate that can be called provided one 
        /// argument. When called, the <see cref="TryCall"/>
        /// method is invoked internally and practically 
        /// the argument is not used. This property can be
        /// used as reactive subscription action.
        /// </summary>
        public Func<object> AsOneParameterFunc
        {
            get;
            private set;
        }

        /// <summary>
        /// Invokes the underlying <see cref="System.Action"/>
        /// within a try-catch wrapper and returns a value
        /// indicating a successful completion of the invocation.
        /// </summary>
        /// <returns>True, if no exception has been caught. 
        /// Otherwise, false.</returns>
        public bool TryCall()
        {
            try
            {
                Action();
            }
            catch (Exception e)
            {
                NumberOfInterruptedCalls++;
                return false;
            }
            NumberOfCompletedCalls++;
            return true;
        }
    }
}
