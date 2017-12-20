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
    enum AccessContext
    {
        ParentPropertyChanged,
        TargetPropertyChanged,
        CollectionChanged,
        CollectionItemChanged,
    }

    class AccessTrace
    {
        public AccessContext Context;
        public object Sender;
        //public UsedPropertyBase Property;
        public string PropertyName;
    }

    interface ICyclicAccessRecord<TraceT>
        where TraceT : class
    {
        //void RecordStepIn(AccessContext context, object sender, UsedPropertyBase property);
        void RecordStepIn(TraceT trace);
        void RecordInterruptedStepInAttempt(TraceT trace);
        void StepOut();
        string GetStackInformation();
    }

    class CyclicAccessRecord<TraceT> : ICyclicAccessRecord<TraceT>
        where TraceT : class
    {
        Stack<TraceT> accessStack = new Stack<TraceT>();
        TraceT interruptedAccessTrace;

        protected TraceT InterruptedAccessTrace { get { return interruptedAccessTrace; } }

        protected IEnumerable<TraceT> EnumeratedAccessTraces()
        {
            foreach (var trace in accessStack)
            {
                yield return trace;
            }
        }

        public void RecordStepIn(TraceT trace)
        {
            if (trace == null)
                throw new ArgumentNullException("trace");

            //var info = new AccessInfo() { Context = context, Sender = sender, Property = property };
            //accessStack.Push(info);
            accessStack.Push(trace);
        }

        public void RecordInterruptedStepInAttempt(TraceT trace)
        {
            if (trace == null)
                throw new ArgumentNullException("trace");
            if (interruptedAccessTrace != null)
            {
                throw new InvalidOperationException(
                     "Interrupted access trace has already been set. " +
                     "The current context must be stepped out from first."
                     );
            }

            interruptedAccessTrace = trace;
        }

        public void StepOut()
        {
            if (interruptedAccessTrace != null)
            {
                interruptedAccessTrace = null;
            }
            else
                accessStack.Pop();
        }

        virtual public string GetStackInformation()
        {
            var builder = new StringBuilder();
            builder.AppendLine("Access layers:");
            foreach (var trace in EnumeratedAccessTraces().Where(TraceFilter))
            {
                AppendTrace(builder, trace, false);
            }

            if (interruptedAccessTrace != null && TraceFilter(interruptedAccessTrace))
            {
                AppendTrace(builder, interruptedAccessTrace, true);
            }

            return builder.ToString();
        }

        virtual protected void AppendTrace(StringBuilder builder, TraceT trace, bool interrupted)
        {
            builder.AppendFormat("-> {0} {1} ", interrupted ? "(interrupted)" : string.Empty, trace);
            builder.AppendLine();
        }

        virtual protected bool TraceFilter(TraceT trace)
        {
            return true;
        }
    }

    class CyclicExpressionAccessRecord : CyclicAccessRecord<AccessTrace>
    {
        protected override void AppendTrace(StringBuilder builder, AccessTrace trace, bool interrupted)
        {
            builder.AppendFormat(
                "-> {0} {1} {2} on object {3}, hashcode: {4}",
                interrupted ? "(interrupted)" : string.Empty,
                trace.Context,
                trace.PropertyName,
                trace.Sender,
                trace.Sender.GetHashCode()
                );
            builder.AppendLine();
        }
    }

    static class CyclicAccessRecordExtensions
    {
        public static void RecordStepIn(
            this ICyclicAccessRecord<AccessTrace> record,
            AccessContext context,
            object sender,
            string propertyName
            )
        {
            var trace = new AccessTrace() { Context = context, Sender = sender, PropertyName = propertyName };
            record.RecordStepIn(trace);
        }
    }
}
