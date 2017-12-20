#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public static class ReflectionServices
    {
        internal static MethodInfo GetMethodInfo<T>(Func<T, T> fluentFunc)
        {
            return fluentFunc.Method;
        }

        public static void RaiseEvent<T>(T target, string eventName)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (eventName == null)
                throw new ArgumentNullException("eventName");

            EventInfo eventInfo = typeof(T).GetEvent(eventName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (eventInfo == null)
            {
                string msg = string.Format("Type {0} does not have event " +
                    "named '{1}'.", typeof(T), eventName);
                throw new ArgumentException(msg);
            }

            FieldInfo delegateField = target.GetType().GetField(
                eventInfo.Name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            if (delegateField == null)
            {
                string msg = string.Format("Type {0} does not have backing field for the event " +
                    "named '{1}'. Events with add/remove handlers specified cannot be used this way.",
                    target.GetType(), eventName);
                throw new ArgumentException(msg);
            }

            MulticastDelegate eventDelegate = (MulticastDelegate)delegateField.GetValue(target);

            if (eventDelegate != null)
                eventDelegate.DynamicInvoke(null);
        }
    }
}
