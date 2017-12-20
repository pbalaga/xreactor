#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace xReactor
{
    public static class CompatibilitySettings
    {
        /// <summary>
        /// Defines a private thread synchronization object for the <see cref="T:CompatibilitySettings"/> class.
        /// </summary>
        private static object synchronizationObject = new object();
        private static EventHandler<PropertyChangedEventArgs> notifyPropertyChanged;
        private static Action<UsedPropertyChain> notifyPropertyChainChanged;

        static CompatibilitySettings()
        {
            NotifyPropertyChanged = (sender, args) =>
                {
                    var customImplementation = sender as IRaisePropertyChanged;
                    if (customImplementation != null)
                        customImplementation.RaisePropertyChanged(args);
                    else
                    {
                        string message = string.Format(
                            "PropertyChanged event could not be raised, because the view model " +
                            "{0} does not implement {1} interface. Change notifications cannot " +
                            "be swallowed silently. Make sure that all your view models implement " +
                            "the {1} interface or do not use the SetAndNotify syntax. The SetAndNotify " +
                            "syntax requires a way to auto-propagate property changed notifications.",
                            sender.GetType(),
                            typeof(IRaisePropertyChanged)
                            );
                        System.Diagnostics.Debug.WriteLine(message);
                        throw new CannotNotifyException(message);
                    }
                };
            NotifyPropertyChainChanged = chain => chain.RaiseAfterChainChangedOnTarget();
        }

        public static EventHandler<PropertyChangedEventArgs> NotifyPropertyChanged
        {
            get
            {
                lock (synchronizationObject)
                {
                    return notifyPropertyChanged;
                }
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                lock (synchronizationObject)
                {
                    notifyPropertyChanged = value;
                }
            }
        }

        internal static Action<UsedPropertyChain> NotifyPropertyChainChanged
        {
            get
            {
                lock (synchronizationObject)
                {
                    return notifyPropertyChainChanged;
                }
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                lock (synchronizationObject)
                {
                    notifyPropertyChainChanged = value;
                }
            }
        }

    }
}
