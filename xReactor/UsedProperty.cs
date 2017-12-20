#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    abstract public class UsedPropertyBase
    {
        static ICyclicAccessRecord<AccessTrace> cyclicAccessRecord;

        UsedPropertyChain topLevelParent;
        INotifyPropertyChanged lastParent;
        object lastTarget;

        List<INotifyPropertyChanged> tracedItems;
        ICyclicAccessGuard<AccessTrace> cyclicAccessGuard;

        public string Name
        {
            get;
            private set;
        }

        public UsedSubproperty Child
        {
            get;
            private set;
        }

        public bool IsLastChild
        {
            get { return Child == null; }
        }

        public UsedPropertyChain TopLevelParent
        {
            get { return topLevelParent; }
            set
            {
                topLevelParent = value;
                if (Child != null)
                    Child.TopLevelParent = value;
            }
        }

        public Func<object> TargetRetriever
        {
            get;
            set;
        }

        internal TraversalOptions TraversalOptions
        {
            get;
            private set;
        }

        static UsedPropertyBase()
        {
            ResetCyclicAccessRecord();
        }

        internal static void ResetCyclicAccessRecord()
        {
            cyclicAccessRecord = new CyclicExpressionAccessRecord();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UsePropertyBase"/> class.
        /// </summary>
        internal UsedPropertyBase(string name, UsedSubproperty child,
            TraversalOptions traversalOptions)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            this.Child = child;
            this.Name = name;
            this.tracedItems = new List<INotifyPropertyChanged>();
            this.TraversalOptions = traversalOptions;
            this.cyclicAccessGuard = new CyclicAccessGuard<AccessTrace>(1, cyclicAccessRecord);
        }

        abstract internal INotifyPropertyChanged GetCurrentParent();

        virtual protected object GetCurrentTarget()
        {
            var retriever = this.TargetRetriever;
            if (retriever != null)
            {
                return retriever();
            }
            else return null;
        }

        internal void EnsureHandlersAreAttached()
        {
            RehookParentIfRequired();
            RehookTargetIfRequired();

            if (Child != null)
                Child.EnsureHandlersAreAttached();
        }

        private void RehookParentIfRequired()
        {
            INotifyPropertyChanged newParent = GetCurrentParent();

            if (newParent != lastParent)
            {
                UnhookParent(lastParent);
                HookParent(newParent);

                lastParent = newParent;
            }
        }

        private void RehookTargetIfRequired()
        {
            object newTarget = GetCurrentTarget();

            if (newTarget != lastTarget)
            {
                UnhookTarget(lastTarget);
                HookTarget(newTarget);

                lastTarget = newTarget;
            }
        }

        internal void RaiseOnParentChanged()
        {
            EnsureHandlersAreAttached();
        }

        /// <summary>
        /// Raises cascading property change notifications,
        /// but does that directly on target instances.
        /// </summary>
        protected internal void RaiseAfterChainChangedOnTarget()
        {
            RaiseAfterChainChangedOnTarget(new PropertyChangedEventArgs(this.Name));
        }

        private void RaiseAfterChainChangedOnTarget(PropertyChangedEventArgs args)
        {
            var notifiable = this.GetCurrentParent() as INotifyPropertyChanged;
            if (notifiable != null)
                RaiseAfterPropertyChangedNotification(notifiable, args);

            if (this.Child != null)
                this.Child.RaiseAfterChainChangedOnTarget();
        }

        private void RaiseAfterPropertyChangedNotification(INotifyPropertyChanged notifiable,
            PropertyChangedEventArgs args)
        {
            var handler = CompatibilitySettings.NotifyPropertyChanged;
            if (handler != null)
            {
                handler(notifiable, args);
            }
            //var customImplementation = notifiable as NotifyPropertyChangedBase;
            //if (customImplementation != null)
            //    customImplementation.RaisePropertyChanged(args);
            //else
            //{
            //    throw new NotImplementedException();
            //    ReflectionServices.RaiseEvent(notifiable, "PropertyChanged");
            //}
        }

        private void HookTarget(object target)
        {
            var sourceCollection = target as INotifyCollectionChanged;
            if (sourceCollection != null)
            {
                sourceCollection.CollectionChanged += sourceCollection_CollectionChanged;

                if (TraversalOptions.TracksCollectionItems)
                {
                    //In case there are any object in the collection already, attach handlers
                    //now:
                    AttachAllItemListeners(sourceCollection as IEnumerable<object>);
                }
            }

            //Listen to INPC events only if this is the last child 
            //(last element of the expression that has no descendants).
            //Typically INPC listeners are set on the parent of a property
            //(that is on the object, on which the property is defined),
            //but it may useful to raise notifications, if anything 
            //on last child changed, although it's not a parent for any
            //subelement. It's useful, because some functions may operate on 
            //more than one subproperty of the last-child-object. I.e.:
            //React.To(() => calculateTotalScore(TeamProperty));
            //Same with extension methods.
            if (this.TraversalOptions.TrackLastChild)
            {
                var source = target as INotifyPropertyChanged;
                if (source != null && IsLastChild)
                    source.PropertyChanged += target_PropertyChanged;
            }
        }

        private void UnhookTarget(object target)
        {
            var sourceCollection = target as INotifyCollectionChanged;
            if (sourceCollection != null)
            {
                sourceCollection.CollectionChanged -= sourceCollection_CollectionChanged;

                if (TraversalOptions.TracksCollectionItems)
                {
                    //Clear if anything's there
                    UnhookAllItems();
                }
            }

            if (this.TraversalOptions.TrackLastChild)
            {
                var source = target as INotifyPropertyChanged;
                if (source != null && IsLastChild)
                    source.PropertyChanged -= target_PropertyChanged;
            }
        }

        private void HookParent(INotifyPropertyChanged parent)
        {
            if (parent != null)
                parent.PropertyChanged += source_PropertyChanged;
        }

        void UnhookParent(INotifyPropertyChanged parent)
        {
            if (parent != null)
                parent.PropertyChanged -= source_PropertyChanged;
        }

        void UnhookAllItems()
        {
            if (tracedItems != null)
            {
                foreach (var item in tracedItems)
                {
                    item.PropertyChanged -= item_PropertyChanged;
                }
                tracedItems.Clear();
            }
        }

        private void target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsLastChild)
                throw new InvalidOperationException("Target changes are only supported on " +
                    "instances that do not have any children. If they have, their children " +
                    "are supposed to track changes on these instances.");

            //cyclicAccessRecord.RecordStepIn(AccessContext.TargetPropertyChanged, sender, e.PropertyName);
            cyclicAccessGuard.StepInOrThrow(AccessContext.TargetPropertyChanged, sender, e.PropertyName);
            RaiseSourceChanged();
            cyclicAccessGuard.StepOutOrThrow();
            //cyclicAccessRecord.StepOut();
        }

        private void source_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ShouldRaiseChangeNotification(e.PropertyName))
            {
                //if (!cyclicAccessGuard.TryStepIn(AccessContext.ParentChanged, sender, this))
                //    return;

                cyclicAccessGuard.StepInOrThrow(AccessContext.ParentPropertyChanged, sender, e.PropertyName);
                RehookTargetIfRequired();

                //update child before reevaluating expression
                //-may save some cycles
                if (Child != null)
                    Child.RaiseOnParentChanged();

                RaiseSourceChanged();
                cyclicAccessGuard.StepOutOrThrow();
            }
        }

        void sourceCollection_CollectionChanged(object sender,
            NotifyCollectionChangedEventArgs e)
        {
            cyclicAccessRecord.RecordStepIn(AccessContext.CollectionChanged, sender, this.Name);

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UnhookAllItems();
            }
            else if (e.OldItems != null && TraversalOptions.TracksCollectionItems) //OldItems is null for Reset
            {
                RemoveAllItemListeners(e.OldItems);
            }

            if (e.NewItems != null && TraversalOptions.TracksCollectionItems)
            {
                AttachAllItemListeners(e.NewItems);
            }

            RaiseSourceChanged();
            cyclicAccessRecord.StepOut();
        }

        void item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (ShouldRaiseItemChangeNotification(e.PropertyName))
            {
                cyclicAccessRecord.RecordStepIn(AccessContext.CollectionItemChanged, sender, e.PropertyName);
                RaiseSourceChanged();
                cyclicAccessRecord.StepOut();
            }
        }

        private void RemoveAllItemListeners(System.Collections.IEnumerable items)
        {
            if (items != null)
            {
                foreach (var item in items.OfType<INotifyPropertyChanged>())
                {
                    RemoveItemListener(item);
                }
            }
        }

        private void AttachAllItemListeners(System.Collections.IEnumerable items)
        {
            if (items != null)
            {
                foreach (var item in items.OfType<INotifyPropertyChanged>())
                {
                    AttachItemListener(item);
                }
            }
        }

        private void RemoveItemListener(INotifyPropertyChanged item)
        {
            item.PropertyChanged -= item_PropertyChanged;
            tracedItems.Remove(item);
        }

        private void AttachItemListener(INotifyPropertyChanged item)
        {
            item.PropertyChanged += item_PropertyChanged;
            tracedItems.Add(item);
        }

        private bool ShouldRaiseChangeNotification(string changedPropertyName)
        {
            return PropertyNamesMatch(this.Name, changedPropertyName);
        }

        private bool ShouldRaiseItemChangeNotification(string changedPropertyName)
        {
            return SignalsAllPropertiesChanged(changedPropertyName)
             || TraversalOptions.Properties.AreAllTracked
             || TraversalOptions.Properties.Tracked.Contains(changedPropertyName);
        }

        private bool PropertyNamesMatch(string tracedName, string changedName)
        {
            return SignalsAllPropertiesChanged(changedName) || tracedName == changedName;
        }

        private static bool SignalsAllPropertiesChanged(string changedName)
        {
            return string.IsNullOrEmpty(changedName);
        }

        /// <summary>
        /// Call when there is a need to reevaluate property 
        /// from lambda expression.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        protected void RaiseSourceChanged()
        {
            TopLevelParent.RaiseChainChanged();
        }

    }

    public class UsedSubproperty : UsedPropertyBase
    {
        Func<INotifyPropertyChanged> parentRetriever;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UsedSubproperty"/> class.
        /// </summary>
        internal UsedSubproperty(string name, Func<INotifyPropertyChanged> parentRetriever, UsedSubproperty child,
            TraversalOptions traversalOptions)
            : base(name, child, traversalOptions)
        {
            if (parentRetriever == null)
                throw new ArgumentNullException("parentRetriever");

            this.parentRetriever = parentRetriever;
        }

        internal override INotifyPropertyChanged GetCurrentParent()
        {
            return parentRetriever();
        }
    }

    public class UsedPropertyChain : UsedPropertyBase
    {
        public event Action Changed;

        public INotifyPropertyChanged Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:UsedProperty"/> class.
        /// </summary>
        internal UsedPropertyChain(INotifyPropertyChanged parent, string name, UsedSubproperty child,
            TraversalOptions traversalOptions)
            : base(name, child, traversalOptions)
        {
            if (parent == null)
                throw new ArgumentNullException("target");

            this.Parent = parent;
            this.TopLevelParent = this;
        }

        protected internal void RaiseChainChanged()
        {
            var handler = this.Changed;
            if (handler != null)
            {
                handler();
            }
        }

        internal override INotifyPropertyChanged GetCurrentParent()
        {
            return Parent;
        }
    }

    internal static class UsedPropertyChainExtensions
    {
        public static void AttachListeners(this IEnumerable<UsedPropertyChain> chains)
        {
            foreach (var chain in chains)
            {
                chain.EnsureHandlersAreAttached();
            }
        }
    }
}
