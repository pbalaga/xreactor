#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    interface ICyclicAccessGuard<TraceT>
        where TraceT : class
    {
        uint CycleCount { get; }
        uint AllowedNumberOfCycles { get; }
        bool TryStepIn(TraceT trace);
        void StepInOrThrow(TraceT trace);
        bool TryStepOut();
        void StepOutOrThrow();
    }

    class CyclicAccessGuard<TraceT> : ICyclicAccessGuard<TraceT>
        where TraceT : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:CyclicAccessGuard"/> class.
        /// </summary>
        public CyclicAccessGuard()
            : this(1)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CyclicAccessGuard"/> class.
        /// </summary>
        public CyclicAccessGuard(uint allowedNumberOfCycles)
            : this(allowedNumberOfCycles, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CyclicAccessGuard"/> class.
        /// </summary>
        public CyclicAccessGuard(uint allowedNumberOfCycles, ICyclicAccessRecord<TraceT> accessRecord)
        {
            this.AllowedNumberOfCycles = allowedNumberOfCycles;
            this.AccessRecord = accessRecord;
        }

        public uint CycleCount
        {
            get;
            private set;
        }

        public uint AllowedNumberOfCycles
        {
            get;
            private set;
        }

        public ICyclicAccessRecord<TraceT> AccessRecord
        {
            get;
            private set;
        }

        public bool TryStepIn(TraceT trace)
        {
            if (CycleCount + 1 > AllowedNumberOfCycles)
            {
                WriteInterruptedTraceIfNotNull(trace);
                return false;
            }
            else
            {
                WriteTraceIfNotNull(trace);
                CycleCount++;
                return true;
            }
        }

        public void StepInOrThrow(TraceT trace)
        {
            if (CycleCount + 1 > AllowedNumberOfCycles)
            {
                WriteInterruptedTraceIfNotNull(trace);
                //When exception is thrown CycleCount is not further incremented.
                throw new CyclicAccessException(
                    string.Format(
                    "Operation cannot proceed, beceause access is guarded against " +
                    "cyclic reference. Possible cyclic reference has been detected: " +
                    "all {0} of {0} recursive entrances allowed have been made. " +
                    Environment.NewLine + GetAccessStackInformationOrEmptyString(),
                    AllowedNumberOfCycles
                    ));
            }
            else
            {
                WriteTraceIfNotNull(trace);
                CycleCount++;
            }
        }

        string GetAccessStackInformationOrEmptyString()
        {
            return AccessRecord == null ? string.Empty : AccessRecord.GetStackInformation();
        }

        public bool TryStepOut()
        {
            StepOutFromRecordContext();
            if (CycleCount - 1 < 0)
            {
                return false;
            }
            else
            {
                CycleCount--;
                return true;
            }
        }

        public void StepOutOrThrow()
        {
            StepOutFromRecordContext();
            if (CycleCount - 1 < 0)
            {
                throw new InvalidOperationException(
                    "Operation cannot proceed, beceause access is guarded against " +
                    "cyclic reference. Now it has been attempted to step out from " +
                    "the is-in-cycle context, but there was no corresponding 'step-in' call."
                    );
            }
            else
            {
                CycleCount--;
            }
        }

        void WriteInterruptedTraceIfNotNull(TraceT trace)
        {
            if (trace != null)
            {
                CheckRecordIsAvailable();
                this.AccessRecord.RecordInterruptedStepInAttempt(trace);
            }
        }

        void WriteTraceIfNotNull(TraceT trace)
        {
            if (trace != null)
            {
                CheckRecordIsAvailable();
                this.AccessRecord.RecordStepIn(trace);
            }
        }

        void StepOutFromRecordContext()
        {
            if (this.AccessRecord != null)
                this.AccessRecord.StepOut();
        }

        void CheckRecordIsAvailable()
        {
            if (this.AccessRecord == null)
            {
                throw new InvalidOperationException(
                    string.Format(
                    "Access record must be set in the constructor of the '{0}' type, if you want to " +
                    "write access history. Access record does not need to set if you do not pass " +
                    "'trace' object. ",
                    this.GetType()
                    ));
            }
        }
    }

    static class CyclicAccessGuardExtensions
    {
        public static bool TryStepIn(this ICyclicAccessGuard<AccessTrace> guard)
        {
            return guard.TryStepIn(null);
        }

        public static void StepInOrThrow(this ICyclicAccessGuard<AccessTrace> guard)
        {
            guard.StepInOrThrow(null);
        }

        public static bool TryStepIn(
            this ICyclicAccessGuard<AccessTrace> guard,
            AccessContext context,
            object sender,
            string propertyName
            )
        {
            var trace = new AccessTrace() { Context = context, Sender = sender, PropertyName = propertyName };
            return guard.TryStepIn(trace);
        }

        public static void StepInOrThrow(
            this ICyclicAccessGuard<AccessTrace> guard,
            AccessContext context,
            object sender,
            string propertyName
            )
        {
            var trace = new AccessTrace() { Context = context, Sender = sender, PropertyName = propertyName };
            guard.StepInOrThrow(trace);
        }
    }
}
