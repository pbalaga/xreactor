#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using xReactor.Common;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace xReactor.Tests
{
    [TestClass]
    public class PropertySystemIntegrationTests : ReactiveTestBase
    {
        #region Class level init and destruction / Instance properties

        public static RecursiveObject<int> StaticRecursiveIntField;

        public int CleanIntProperty
        {
            get;
            private set;
        }

        private int raisingIntPropertyWithoutEqualityCheckBackingField;
        public int RaisingIntPropertyWithoutEqualityCheck
        {
            get { return raisingIntPropertyWithoutEqualityCheckBackingField; }
            set
            {
                if (raisingIntPropertyWithoutEqualityCheckBackingField != value)
                {
                    raisingIntPropertyWithoutEqualityCheckBackingField = value;
                    RaisePropertyChanged(() => RaisingIntPropertyWithoutEqualityCheck);
                }
            }
        }

        private int raisingIntPropertyBackingField;
        public int RaisingIntProperty
        {
            get { return raisingIntPropertyBackingField; }
            set
            {
                if (raisingIntPropertyBackingField != value)
                {
                    raisingIntPropertyBackingField = value;
                    RaisePropertyChanged(() => RaisingIntProperty);
                }
            }
        }

        public RecursiveObject<int> RecursiveInt
        {
            get;
            set;
        }

        public RecursiveObject<char> RecursiveChar
        {
            get;
            set;
        }

        public ObservableCollection<Person> People
        {
            get;
            set;
        }

        public ObservableCollection<Room> Rooms
        {
            get;
            set;
        }

        private ObservableCollection<RecursiveObject<int>> resultantCollection;
        public ObservableCollection<RecursiveObject<int>> ResultantCollection
        {
            get { return resultantCollection; }
            set
            {
                if (resultantCollection != value)
                {
                    resultantCollection = value;
                    RaisePropertyChanged(() => ResultantCollection);
                }
            }
        }

        private ObservableCollection<RecursiveObject<int>> collectionA;
        public ObservableCollection<RecursiveObject<int>> CollectionA
        {
            get { return collectionA; }
            set
            {
                if (collectionA != value)
                {
                    collectionA = value;
                    RaisePropertyChanged(() => CollectionA);
                }
            }
        }

        private ObservableCollection<RecursiveObject<int>> collectionB;
        public ObservableCollection<RecursiveObject<int>> CollectionB
        {
            get { return collectionB; }
            set
            {
                if (collectionB != value)
                {
                    collectionB = value;
                    RaisePropertyChanged(() => CollectionB);
                }
            }
        }

        private bool toggle;
        public bool Toggle
        {
            get { return toggle; }
            set
            {
                if (toggle != value)
                {
                    toggle = value;
                    RaisePropertyChanged(() => Toggle);
                }
            }
        }

        public static class LockClass
        {
            public static object LockObject = new object();
        }

        [TestInitialize]
        public void InitializeEveryTest()
        {
            Monitor.Enter(LockClass.LockObject);
            ClearPropertyChangedHandlers();
            DiagnosticSettings.Custom.Reset();
            UsedPropertyBase.ResetCyclicAccessRecord();
            StaticRecursiveIntField = null;
            this.CleanIntProperty = default(int);
            this.RaisingIntPropertyWithoutEqualityCheck = default(int);
            this.RaisingIntProperty = default(int);
            this.RecursiveInt = null;
            this.RecursiveChar = null;
            this.People = null;
            this.Rooms = null;
            this.ResultantCollection = null;
            this.CollectionA = null;
            this.CollectionB = null;
            this.Toggle = false;
        }

        [TestCleanup]
        public void CleanupAfterEveryTest()
        {
            Monitor.Exit(LockClass.LockObject);
        }

        #endregion

        #region Basic tests

        [TestMethod]
        public void ReevaluatesTarget_WhenSourcePropertyChanges()
        {
            var room = CreateRoom();

            room.PeopleLimit.Should().Be(0, "room area is 0 - not set yet.");
            room.SquareMeters = 100;
            room.PeopleLimit.Should().Be(10, "people limit should have been updated " +
                "after SquareMeters property changes");
        }

        [TestMethod]
        public void ReevaluatesTarget_WhenSubPropertyChanges()
        {
            var room = CreateRoom();
            var person = new Person();
            room.Owner = person;

            room.OwnerAge.Should().Be(0, "age is not changed yet");
            person.Age = 20;
            room.OwnerAge.Should().Be(20, "age has already been updated");
        }

        [TestMethod]
        public void ReevaluatesTarget_WhenSourceCollectionCountChanges()
        {
            var room = CreateRoom();
            var chair = new Chair();
            var sofa = new Sofa();

            room.LargestPlaceToSit.Should().BeNull("no furniture has been added yet");
            room.Furniture.Add(chair);
            room.LargestPlaceToSit.Should().BeSameAs(chair, "added a 1-man chair");
            room.Furniture.Add(sofa);
            room.LargestPlaceToSit.Should().BeSameAs(sofa, "added a 4-man sofa");
        }

        [TestMethod]
        public void ReevaluatesTarget_WhenSourceCollectionElementChanges()
        {
            var room = CreateRoom();
            var chair = new Chair();
            var sofa = new Sofa();

            room.Furniture.Add(chair);
            room.Furniture.Add(sofa);
            room.LargestPlaceToSit.Should().BeSameAs(sofa, "now sofa is bigger");

            chair.NumSeats = 5; //hmm, a five-seat chair? 
            room.LargestPlaceToSit.Should().BeSameAs(chair, "now chair was made larger." +
                "This requires the system to trace all changes on each item in the " +
                "collection");
        }

        [TestMethod]
        public void DoesNotReevaluateTarget_WhenSourceCollectionElementChangesButItemsAreNotTracked()
        {
            var room = CreateRoom();
            var chair = new Chair();
            var sofa = new Sofa();

            room.Furniture.Add(chair);
            room.Furniture.Add(sofa);
            room.SmallestPlaceToSit.Should().BeSameAs(chair, "chair is smaller than sofa by default");

            chair.NumSeats = 5; //hmm, a five-seat chair? 
            room.SmallestPlaceToSit.Should().BeSameAs(chair, "although chair was made larger, " +
                "changes to individual items of the furniture collection are not tracked " +
                "- see the expression for the SmallestPlaceToSit property. Consequently, " +
                "the value of SmallestPlaceToSit should not be updated at this point.");
        }

        [TestMethod]
        public void DoesNotReevaluateExpression_WhenItemChangesAfterRemoval()
        {
            var room = CreateRoom();
            var chair = new Chair();
            var sofa = new Sofa();

            room.Furniture.Add(chair);
            room.Furniture.Add(sofa);
            room.LargestPlaceToSit.Should().BeSameAs(sofa, "now sofa is bigger");
            room.Furniture.Remove(chair);
            chair.NumSeats = 5;

            room.LargestPlaceToSit.Should().BeSameAs(sofa, "although chair was made larger," +
                "the chair has been removed from Furniture beforehand.");
        }

        #endregion

        #region Advanced chains / expressions (on recursive object)

        [TestMethod]
        public void FieldAtTheBeginning_ShouldBeTracked()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(0);
            int propertyAValue = -1;
            React.To(() => recursive.PropertyA)
                .Subscribe(val => propertyAValue = val);

            recursive.PropertyA = 10;

            propertyAValue.Should().Be(10, "tracking fields is enabled.");
        }

        [TestMethod]
        public void FieldAtTheBeginning_WithoutFieldTracking_TracksNothing()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(0);
            var counter = new CallCounter();
            Action sub = () => React.To(() => recursive.DoNotTrackFields().PropertyA)
                .Subscribe(val => counter.TryCall());

            sub.ShouldThrowBecauseNothingIsTracked();
            //additionally:
            recursive.PropertyA = 10;

            counter.NumberOfAttemptedCalls.Should().Be(0, "tracking fields is disabled " +
                "or local variables and method parameters is disabled");
        }

        [TestMethod]
        public void FieldAtTheBeginning_WithFieldTracking_TracksChanges()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(0);
            int propertyAValue = -1;
            React.To(() => recursive.TrackFields().PropertyA)
                .Subscribe(val => propertyAValue = val);

            recursive.PropertyA = 10;

            propertyAValue.Should().Be(10, "tracking fields is enabled.");
        }

        [TestMethod]
        public void FieldAtTheEnd_TracksPrecedingExpression()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            int log = -10;
            React.To(() => RecursiveInt.Inner.FieldA).
                SkipInitial().
                Subscribe(val => log = val);

            this.RecursiveInt.Inner = new RecursiveObject<int>() { FieldB = 10 };

            log.Should().Be(0, "0 is the default value for FieldA. " +
                "Handlers are to be fired, because changes of the Inner property are tracked " +
                "Notifications weren't raised, causing the expression to be reevaluted with " +
                "FieldA = 0.");
        }

        [TestMethod]
        public void FieldInTheMiddle_TracksPrecedingExpression()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(2);
            int callCount = 0;
            Action<int> subscription = (val) => callCount++;

            React.To(() => RecursiveInt.Inner.InnerField.PropertyA).Subscribe(subscription);
            RecursiveInt.Inner = RecursiveObject<int>.CreateOfDepth(1);

            callCount.Should().BeGreaterOrEqualTo(1, "because Recursive.Inner has changed " +
                "and Recursive.Inner should be tracked as the part of the expression, which " +
                "precedes a field (i.e. InnerField).");
        }

        [TestMethod]
        public void FieldChain_WithoutFieldTracking_NothingShouldBeTracked()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(3);
            var counter = new CallCounter();
            Action sub = () => React.To(() => recursive.DoNotTrackFields().InnerField.InnerField.FieldA)
                .Subscribe(val => counter.TryCall());

            sub.ShouldThrowBecauseNothingIsTracked();
            //additionally:
            recursive.PropertyA = 10;
            counter.NumberOfAttemptedCalls.Should().Be(0, "tracking fields is disabled " +
                "or local variables and method parameters is disabled");
        }

        [TestMethod]
        public void FieldChain_NothingShouldBeTracked()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(3);
            var counter = new CallCounter();
            Action sub = () => React.To(() => recursive.InnerField.InnerField.FieldA)
                .Subscribe(val => counter.TryCall());

            sub.ShouldThrowBecauseNothingIsTracked();
            //additionally:
            recursive.PropertyA = 10;
            counter.NumberOfAttemptedCalls.Should().Be(0, "although tracking fields is enabled, " +
                "(by default) the chain consists of fields *only*, so there's no way to know when " +
                "they change. ");
        }

        [TestMethod]
        public void FieldChain_WithEnforcedFieldTracking_NothingShouldBeTracked()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(3);
            var counter = new CallCounter();
            Action sub = () => React.To(() => recursive.InnerField.InnerField.TrackFields().FieldA)
                .Subscribe(val => counter.TryCall());

            sub.ShouldThrowBecauseNothingIsTracked();
            //additionally:
            recursive.PropertyA = 10;
            recursive.Inner.PropertyB = 20;
            recursive.Inner.Inner.PropertyA = 30;
            counter.NumberOfAttemptedCalls.Should().Be(0, "although tracking fields is enabled, " +
                "the chain consists of fields *only*, so there's no way to know when they change. ");
        }

        [TestMethod]
        public void FieldChain_EndingWithProperty_EntireFieldChainShouldBeTrackedAsAConstant()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(2);
            int log = -1;
            React.To(() => recursive.InnerField.InnerField.PropertyA)
                .Subscribe(val => log = val);

            recursive.InnerField.InnerField.PropertyA = 10;
            log.Should().Be(10, "entire field chain at the beginning should " +
                "be treated as a constant expression");
        }

        [TestMethod]
        public void FieldChain_EndingWithMethodCall_EntireFieldChainShouldBeTrackedAsAConstant_ButMethodCallMustCauseHandlersToBeAttached()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(2);
            int log = -1;
            recursive.InnerField.InnerField.PropertyA = 10;
            React.To(() => recursive.InnerField.InnerField.MethodA())
                .Subscribe(val => log = val);

            recursive.InnerField.InnerField.PropertyB = 20;
            recursive.InnerField.InnerField.PropertyA = 30;
            log.Should().Be(10,
                "entire field chain at the beginning should be treated as constant, " +
                "but trailing method call should not cause that attach any " +
                "event handlers are attached"
                );
        }

        [TestMethod]
        public void FieldChain_EndingWithMethodCall_WithLastChildTracking_EntireFieldChainShouldBeTrackedAsAConstant()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(2);
            int log = -1;
            recursive.InnerField.InnerField.PropertyA = 10;
            React.To(() => recursive.InnerField.InnerField.TrackLastChild().MethodA())
                .Subscribe(val => log = val);

            recursive.InnerField.InnerField.PropertyB = 20;
            log.Should().Be(10, "entire field chain at the beginning should " +
                "be treated as a constant expression and additionally all " +
                "properties on the last InnerField should be tracked due to " +
                "the trailing method call");

            recursive.InnerField.InnerField.PropertyA = 30;
            log.Should().Be(30, "entire field chain at the beginning should " +
                "be treated as a constant expression ");
        }

        [TestMethod]
        public void MethodAtTheBeginning_TracksNothing()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(0);
            Func<RecursiveObject<int>> recursiveGetter = () => recursive;
            var counter = new CallCounter();
            Action sub = () => React.To(() => recursiveGetter().PropertyA).Subscribe(val => counter.TryCall());

            sub.ShouldThrowBecauseNothingIsTracked();

            recursive.PropertyA = 10;
            counter.NumberOfAttemptedCalls.Should().Be(0, "method calls should disable tracking.");
        }

        [TestMethod]
        public void MethodInTheMiddle_IfNotMarkerMethod_TrackedChainIsNotContinued()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            int propertyAValue = -1;
            React.To(() => RecursiveInt.GetInner().PropertyA)
                .SkipInitial()
                .Subscribe(val => propertyAValue = val);

            this.RecursiveInt.Inner.PropertyA = 10;
            propertyAValue.Should().Be(-1, "GetInner() is not a registered marker method, " +
                "so PropertyA on the inner object should not be tracked");
        }

        [TestMethod]
        public void MethodCallsOneAfterAnother_DefaultFieldTrackingWorksWell()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(1);
            int propertyAValue = -1;
            React.To(() => recursive.TrackLastChild().GetInner().PropertyA)
                .SkipInitial()
                .Subscribe(val => propertyAValue = val);

            recursive.Inner.PropertyA = 20;
            propertyAValue.Should().Be(-1, "Inner instance should have no listeners attached");

            recursive.PropertyA = 10;
            propertyAValue.Should().Be(20, "TrackLastChild() causes tracking of all properties "
                + "on the 'recursive' instance - TrackLastChild() itself should be ignored as a chain link, " +
                "so it doesn't break the chain. Then the expression ought to be reevaluated");
        }

        [TestMethod]
        public void MethodCallsOneAfterAnother_EnforcedFieldTrackingWorksWell()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(1);
            int propertyAValue = -1;
            React.To(() => recursive.TrackLastChild().TrackFields().GetInner().PropertyA)
                .SkipInitial()
                .Subscribe(val => propertyAValue = val);

            recursive.Inner.PropertyA = 20;
            propertyAValue.Should().Be(-1, "Inner instance should have no listeners attached");

            recursive.PropertyA = 10;
            propertyAValue.Should().Be(20, "TrackLastChild() causes tracking of all properties "
                + "on the 'recursive' instance - TrackLastChild() itself should be ignored as a chain link, " +
                "so it doesn't break the chain. Then the expression ought to be reevaluated");
        }

        [TestMethod]
        public void MethodInTheMiddle_WithoutLastChildTracking_DoesNotTrackPrecedingExpression()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(0);
            int log = -1;

            React.To(() => RecursiveInt.FluentMethod().PropertyA)
                .SkipInitial()
                .Subscribe(val => log = val);
            RecursiveInt.PropertyA = 10;

            log.Should().Be(-1, "PropertyA that changed is on instance kept in 'RecursiveInt' " +
                "property. RecursiveInt is the subject of method call FluentMethod, " +
                "but this should not cause any listeners to be attached automatically, "+
                "so the whole expression should no be reevaluated.");
        }

        [TestMethod]
        public void MethodInTheMiddle_WithLastChildTracking_FullyTracksPrecedingExpression()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(0);
            int log = -1;

            React.To(() => RecursiveInt.FluentMethod().TrackLastChild().PropertyA).Subscribe(val => log = val);
            RecursiveInt.PropertyA = 10;

            log.Should().Be(10, "PropertyA that changed is on instance kept in 'RecursiveInt' " +
                "property. TrackLastChild causes tha all properties on RecursiveInt " +
                "are tracked, so the whole expression should be reevaluated.");
        }

        [TestMethod]
        public void MethodAtTheEnd_WithoutTrackLastChild_DoesNotFullyTrackPrecedingExpression()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            this.RecursiveInt.FieldA = 100;
            int callCount = 0; int log = 0;
            Action<int> subscription = (val) => { callCount++; log = val; };

            React.To(() => RecursiveInt.Inner.MethodA()).SkipInitial().Subscribe(subscription);
            RecursiveInt.PropertyB = 20; //no call, because RecursiveInt.PropertyB is not tracked
            RecursiveInt.Inner.PropertyA = 10; //no call, because of there's no TrackLastChild()

            callCount.Should().Be(0, Because.MethodCallDoesNotAttachListenersByDefault);
        }

        [TestMethod]
        public void MethodAtTheEnd_WithTrackLastChild_FullyTracksPrecedingExpression()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            this.RecursiveInt.FieldA = 100;
            int callCount = 0; int log = 0;
            Action<int> subscription = (val) => { callCount++; log = val; };

            React.To(() => RecursiveInt.Inner.TrackLastChild().MethodA()).SkipInitial().Subscribe(subscription);
            RecursiveInt.PropertyB = 20; //no call, because RecursiveInt.PropertyB is not tracked
            RecursiveInt.Inner.PropertyA = 10; //call, because of TrackLastChild()

            callCount.Should().Be(1, "PropertyA is kept on instance that should be tracked, " +
                "so the expression should be reevaluated once");
            log.Should().Be(10, "this is what should be returned from MethodA() on Recursive.Inner object.");
        }

        [TestMethod]
        public void WithLastChildTracking_TracksAllPropertiesOfTheLastChild()
        {
            RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            int log = -1;

            React.To(() => RecursiveInt.Inner.TrackLastChild()).
                SkipInitial().
                Set(inner => log = inner.PropertyA);

            RecursiveInt.Inner.PropertyA = 10;
            log.Should().Be(10, "PropertyA on Inner instance changed and " +
                "expression is marked with a TrackAllProps() call");
        }

        [TestMethod]
        public void NoLastChildTracking_DoesNotTrackAllPropertiesOfTheLastChild()
        {
            RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            int log = -1;

            React.To(() => RecursiveInt.Inner).
                SkipInitial().
                Set(inner => log = inner.PropertyA);

            RecursiveInt.Inner.PropertyA = 10;
            RecursiveInt.Inner.PropertyB = 20;
            log.Should().Be(-1, "Properties on Inner instance changed, but " +
                "that instance should not be tracked");
        }

        [TestMethod]
        public void ComplexMixedExpression_TracksWhatItShould()
        {
            RecursiveObject<int> recInt = RecursiveObject<int>.CreateOfDepth(1);
            RecursiveObject<string> recStr = RecursiveObject<string>.CreateOfDepth(2);
            RecursiveObject<string> deepestInnerStr = recStr.Inner.Inner;
            this.RecursiveChar = RecursiveObject<char>.CreateOfDepth(0);
            recInt.Inner.FieldA = 3;
            deepestInnerStr.PropertyA = "A";
            this.RecursiveChar.PropertyA = 'x';
            string suffix = "suffix";
            string result = "untouched";

            React.To(() => deepestInnerStr.TrackFields().PropertyA.
                    PadLeft(recInt.TrackFields().Inner.PropertyA * 2,
                    this.RecursiveChar.PropertyA) + suffix
                ).SkipInitial().Set(val => result = val);

            recInt.Inner.PropertyA = 2;
            result.Should().Be("xxxAsuffix");

            this.RecursiveChar.PropertyB = 'q';
            result.Should().Be("xxxAsuffix", "resulting string should've not changed, since " +
                "last assertion");

            recStr.Inner.Inner.PropertyA = "12";
            result.Should().Be("xx12suffix");
        }

        #endregion

        #region Static properties / Singletons

        [TestMethod]
        public void StaticProperty_IsNotTracked_BecauseItCannotImplementINPC()
        {
            int log = -1;
            Action act = () =>
                React.To(() => StaticClass.PropertyA)
                .Set(val => log = val);

            act.ShouldThrowBecauseNothingIsTracked();
        }

        [TestMethod]
        public void StaticField_IsNotTracked()
        {
            int log = -1;
            Action act = () =>
                React.To(() => StaticClass.FieldA)
                .Set(val => log = val);

            act.ShouldThrowBecauseNothingIsTracked();
        }

        [TestMethod]
        public void InstancePropertyAccessedViaStaticField_IsTracked()
        {
            StaticClass.RecursiveField = RecursiveObject<int>.CreateOfDepth(0);
            int log = -1;
            React.To(() => StaticClass.RecursiveField.PropertyA)
                .Set(val => log = val);

            StaticClass.RecursiveField.PropertyA = 20;
            log.Should().Be(20, "PropertyA is on an INPC instance but RecursiveField is not. " +
                "There is no way to know when RecursiveProperty changes, because it's on a static class. " +
                "However, to maintain coherent way of handling fields that occur at the beginning of the " +
                "expression, PropertyA should be tracked."
                );
        }

        [TestMethod]
        public void InstancePropertyAccessedViaStaticProperty_IsNotTracked()
        {
            StaticClass.RecursiveField = RecursiveObject<int>.CreateOfDepth(0);
            int log = -1;
            Action act = () =>
                React.To(() => StaticClass.RecursiveProperty.PropertyA)
                .Set(val => log = val);

            act.ShouldThrowBecauseNothingIsTracked(
                "PropertyA is on an INPC instance but RecursiveProperty is not. " +
                "There is no way to know when RecursiveProperty changes, because it's on a static class." +
                "Additionally, RecursiveProperty is a property, not field. "
                );
        }

        [TestMethod]
        public void SingletonField_IsNotTracked()
        {
            int log = -1;
            Action act = () =>
                React.To(() => SingletonObject.InstanceProperty.IntField)
                .Set(val => log = val);

            act.ShouldThrowBecauseNothingIsTracked();
        }

        [TestMethod]
        public void SingletonProperty_IsNotTracked()
        {
            int log = -1;
            Action act = () =>
                React.To(() => SingletonObject.InstanceProperty.IntProperty)
                .Set(val => log = val);

            //TODO: Add TrackStatic() marker method to track static properties or leave as it is
            //Better add Capture() marker/extension
            act.ShouldThrowBecauseNothingIsTracked();
        }

        [TestMethod]
        public void SingletonProperty_IsTrackedIfInstanceIsStoredInAVariable()
        {
            int log = -1;
            SingletonObject local = SingletonObject.InstanceProperty;
            React.To(() => local.IntProperty)
                .Set(val => log = val);

            SingletonObject.InstanceProperty.IntProperty = 20;

            //TODO: really? InstanceProperty breaks the chain.
            //Maybe treat it as field chain?
            log.Should().Be(20, "singleton was first stored in a local variable, " +
                "so the system should treat it as a field.");
        }

        #endregion

        #region Conditionals

        [TestMethod]
        public void TernaryConditionalOperator_TracksAllBranches()
        {
            var recursive = RecursiveObject<bool>.CreateOfDepth(0);
            bool? log = new bool?();
            React.To(() => recursive.Inner == null ?
                    false : recursive.Inner.PropertyA)
                 .Subscribe(val => log = val);

            log.Should().BeFalse("this is the initial value");

            recursive.Inner = RecursiveObject<bool>.CreateOfDepth(0);
            log.Should().BeFalse("Inner property is not null, but default of " +
                "its PropertyA is false");

            recursive.Inner.PropertyA = true;
            log.Should().BeTrue("Inner property is not null and its PropertyA " +
                "has been set true");
        }

        [TestMethod]
        public void CoalesceOperator_TracksAllBranches()
        {
            var recursiveA = RecursiveObject<bool>.CreateOfDepth(0);
            var recursiveB = RecursiveObject<bool>.CreateOfDepth(1);
            RecursiveObject<bool> instanceSeen = null;

            //TODO: make it possible to say something like
            //React.TrackingFields().To( ... the usual stuff ... )
            //which removes the need to repeat TrackFields() in the expression
            React.To(() => recursiveA.Inner ?? recursiveB.Inner)
                 .Subscribe(val => instanceSeen = val);

            instanceSeen.Should().Be(recursiveB.Inner, "this is the initial value");

            recursiveA.Inner = RecursiveObject<bool>.CreateOfDepth(0);
            instanceSeen.Should().Be(recursiveA.Inner, "recursiveA.Inner is not null now");
        }

        #endregion

        #region Collection tests

        [TestMethod]
        public void CollectionWithTrackedItems_OnlySpecifiedPropertiesAreTracked()
        {
            CollectionA = new ObservableCollection<RecursiveObject<int>>()
            {
                new RecursiveObject<int>() { PropertyA = 5, PropertyB = 10 },
                new RecursiveObject<int>() { PropertyA = 15, PropertyB = 20 }
            };
            int log = -1;
            React.To(() => CollectionA.TrackItems(i => i.PropertyA))
                .Select(coll => coll.Sum(i => i.PropertyA + i.PropertyB))
                .Subscribe(val => log = val);

            log.Should().Be(5 + 10 + 15 + 20, "this is initial value");

            CollectionA.ElementAt(0).PropertyB = 11;
            log.Should().Be(5 + 10 + 15 + 20,
                "PropertyB is not listed in TrackItems() marker method and should not be tracked");

            CollectionA.ElementAt(1).PropertyA = 16;
            log.Should().Be(5 + 11 + 16 + 20,
                "PropertyA is listed in TrackItems() marker method and should be tracked. " +
                "Also, PropertyB was changed earlier on element 0, so this change should be reflected now");

        }

        [TestMethod]
        public void WhenObservedCollectionIsSwappedManually_AllHandlersAreRecreated()
        {
            CollectionA = new ObservableCollection<RecursiveObject<int>>()
            {
                new RecursiveObject<int>() { PropertyA = 5 },
                new RecursiveObject<int>() { PropertyA = 15 }
            };
            CollectionB = new ObservableCollection<RecursiveObject<int>>()
            {
                new RecursiveObject<int>() { PropertyA = 30 },
                new RecursiveObject<int>() { PropertyA = 50 }
            };

            int callCount = 0;
            double avg = -1.0;

            ResultantCollection = CollectionA;

            React.To(() => ResultantCollection.TrackItems())
                .Select(resultant => resultant.Average(r => r.PropertyA))
                .Set(val => { avg = val; callCount++; });

            avg.Should().Be(10, "this is average taken from CollectionA's items");

            CollectionA.ElementAt(1).PropertyA = 25;
            avg.Should().Be(15, "this is average taken from CollectionA's items, " +
                "after second item's PropertyA changed");

            ResultantCollection = CollectionB;

            callCount = 0;
            CollectionA.ElementAt(0).PropertyA = 15;
            callCount.Should().Be(0, "average must not be updated when " +
                "modifying CollectionA's elements");

            CollectionB.ElementAt(1).PropertyA = 10;
            avg.Should().Be(20, "this is average taken from CollectionB's items, " +
                "after second item's PropertyA changed");
        }

        [TestMethod]
        public void WhenObservedCollectionIsSwappedViaAnotherReactiveRelation_AllHandlersAreRecreated()
        {
            CollectionA = new ObservableCollection<RecursiveObject<int>>()
            {
                new RecursiveObject<int>() { PropertyA = 5 },
                new RecursiveObject<int>() { PropertyA = 15 }
            };
            CollectionB = new ObservableCollection<RecursiveObject<int>>()
            {
                new RecursiveObject<int>() { PropertyA = 30 },
                new RecursiveObject<int>() { PropertyA = 50 }
            };
            Toggle = true;
            int callCount = 0;
            double avg = -1.0;

            React.To(() => Toggle ? CollectionA : CollectionB)
                .Set(() => ResultantCollection);

            React.To(() => ResultantCollection.TrackItems())
                .Select(resultant => resultant.Average(r => r.PropertyA))
                .Set(val => { avg = val; callCount++; });

            ResultantCollection.As<object>().Should().BeSameAs(CollectionA);
            avg.Should().Be(10, "this is average taken from CollectionA's items");

            CollectionA.ElementAt(1).PropertyA = 25;
            avg.Should().Be(15, "this is average taken from CollectionA's items, " +
                "after second item's PropertyA changed");

            Toggle = false;

            ResultantCollection.As<object>().Should().BeSameAs(CollectionB);
            avg.Should().Be(40, "this is average taken from CollectionB's items");

            callCount = 0;
            CollectionA.ElementAt(0).PropertyA = 15;
            callCount.Should().Be(0, "average must not be updated when " +
                "modifying CollectionA's elements");

            CollectionB.ElementAt(1).PropertyA = 10;
            avg.Should().Be(20, "this is average taken from CollectionB's items, " +
                "after second item's PropertyA changed");
        }

        #endregion

        #region Type Expressions

        private static ObservableCollection<object> CreateTestObjectList()
        {
            var list = new ObservableCollection<object>();
            list.Add(10);
            list.Add(-2.0);
            list.Add("text");
            list.Add("text 2");
            return list;
        }

        [TestMethod]
        public void TypeEqualityTestExpressionInCollection_SubExpressionIsTracked()
        {
            var list = CreateTestObjectList();
            int log = -1;

            React.To(() => list.Count(tab => tab.GetType() == typeof(string)))
                .Set(count => log = count);

            log.Should().Be(2, "there are two string instances in the list.");

            list.Add("new text");

            log.Should().Be(3, "there are now three string instances in the list.");
        }

        [TestMethod]
        public void Is_TypeBinaryExpressionWithoutTrackableProperty_NothingIsTracked()
        {
            object item = "text";
            bool? log = new bool?();

            Action act = () => React.To(() => item is string)
                .Set(count => log = count);

            act.ShouldThrowBecauseNothingIsTracked();
        }

        [TestMethod]
        public void Is_TypeBinaryExpressionInCollection_SubExpressionIsTracked()
        {
            var list = CreateTestObjectList();
            int log = -1;

            React.To(() => list.Count(tab => tab is string))
                .Set(count => log = count);

            log.Should().Be(2, "there are two string instances in the list.");

            list.Add("new text");

            log.Should().Be(3, "there are now three string instances in the list.");
        }

        [TestMethod]
        public void As_TypeBinaryExpressionWithoutTrackableProperty_NothingIsTracked()
        {
            object item = "text";
            string castItem = null;

            Action act = () => React.To(() => item as string)
                .Set(castValue => castItem = castValue);

            act.ShouldThrowBecauseNothingIsTracked();
        }

        [TestMethod]
        public void As_TypeBinaryExpressionInCollection_SubExpressionIsTracked()
        {
            var list = CreateTestObjectList();
            int log = -1;

            React.To(() => list.Select(tab => tab as string))
                .Set(filtered => log = filtered.Count(tab => tab != null));

            log.Should().Be(2, "there are two string instances in the list.");

            list.Add("new text");

            log.Should().Be(3, "there are now three string instances in the list.");
        }

        #endregion

        #region Invocations

        [TestMethod]
        public void Invocation_BreaksChain()
        {
            RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            int log = -1;
            React.To(() => RecursiveInt.InnerAsFunc().PropertyA)
                .SkipInitial()
                .Set(val => log = val);

            RecursiveInt.Inner.PropertyA = 20;
            log.Should().Be(-1, "invocation breaks notification chain");

            RecursiveInt.Inner = new RecursiveObject<int>() { PropertyA = 30 };
            log.Should().Be(30, "InnerAsFunc property is updated whenever Inner is set");
        }

        #endregion

        #region Set

        [TestMethod]
        public void Set_SetsAMemberGivenByLongPath()
        {
            var instance = RecursiveObject<int>.CreateOfDepth(0);
            React.To(() => instance.PropertyA).Set(() => instance.PropertyB);

            instance.PropertyA = 10;
            instance.PropertyB.Should().Be(10,
                "PropertyB is bound to PropertyA's value, " +
                "so they should be equal");
        }

        #endregion

        #region SetAndNotify / Cyclic access with SetAndNotify

        [TestMethod]
        public void SetAndNotify_RaisesNotification_WhenTrackedPropertiesAreOnDifferentInstanceThanThePropertySet()
        {
            var propertyOwner = RecursiveObject<int>.CreateOfDepth(0);
            var counter = new CallCounter();
            this.PropertyChanged += (sender, args) => counter.TryCall();

            React.To(() => propertyOwner.PropertyA)
                .SkipInitial()
                .SetAndNotify(() => this.CleanIntProperty);

            propertyOwner.PropertyA = 10;

            const string reason = "this.CleanIntProperty should be set exactly once in " +
                "response to exactly one change of this.RecursiveInt.PropertyA";
            this.CleanIntProperty.Should().Be(10, reason);
            counter.NumberOfAttemptedCalls.Should().Be(1, reason);
        }

        [TestMethod]
        public void SetAndNotify_RaisesNotification_WhenTrackedPropertiesAreOnTheSameInstanceAsThePropertySet()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(0);
            var counter = new CallCounter();
            this.PropertyChanged += (sender, args) => counter.TryCall();

            React.To(() => this.RecursiveInt.PropertyA)
                .SkipInitial()
                .SetAndNotify(() => this.CleanIntProperty);

            this.RecursiveInt.PropertyA = 10;

            const string reason = "this.CleanIntProperty should be set exactly once in " +
                "response to exactly one change of this.RecursiveInt.PropertyA";
            this.CleanIntProperty.Should().Be(10, reason);
            counter.NumberOfAttemptedCalls.Should().Be(1, reason);
        }

        [TestMethod]
        public void SetAndNotify_RaisesNotificationWithoutCyclicAccessException_WhenPropertySetRaisesOwnNotificationsWithoutEqualityCheck()
        {
            this.RecursiveInt = RecursiveObject<int>.CreateOfDepth(0);

            React.To(() => this.RecursiveInt.PropertyA)
                //.SkipInitial()
                .SetAndNotify(() => this.CleanIntProperty);

            React.To(() => this.CleanIntProperty)
                .SkipInitial()
                .SetAndNotify(() => this.RaisingIntPropertyWithoutEqualityCheck);

            Action act = () => this.RecursiveInt.PropertyA = 10;

            const string reason = "at best this.RaisingIntPropertyWithoutEqualityCheck should be set exactly once in " +
                "response to exactly one change of this.CleanIntProperty. " +
                "Specifically, this code must not throw CyclicAccessException.";
            act.ShouldNotThrow<CyclicAccessException>(reason);
            this.RaisingIntPropertyWithoutEqualityCheck.Should().Be(10, reason);
        }

        [TestMethod]
        public void SetAndNotify_ExpressionCausingCyclicAccessExceptionAsInThePeopleAndPersonSample_ShouldNotThrow_WithTurnOffTheSubscription()
        {
            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.TurnOffTheSubscription;
            var outer = new CyclicAccess_OuterModel();
            Action act = () => outer.IsViewingActors = !outer.IsViewingActors;

            act.ShouldNotThrow<SubscriptionFailedException>(Because.CyclicSubscriptionShouldBeTurnedOffSilently);
        }

        [TestMethod]
        public void SetAndNotify_ShouldThrowWhenNotificationsAreSwallowedSilently_WithFailFast()
        {
            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.FailFast;
            Action act = () => new NotificationsSwallowed_OuterModel();

            act.ShouldThrow<SubscriptionFailedException>(Because.SubscriptionIsInvalid)
                .And.InnerException.Should().BeOfType<CannotNotifyException>(
                    Because.SwallowingExceptionsIsNotAllowed_AndMustBeThrownImmediately
                );
        }

        [TestMethod]
        public void SetAndNotify_ShouldNotThrowWhenNotificationsAreSwallowedSilently_WithTurnOffTheSubscription()
        {
            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.TurnOffTheSubscription;
            var outer = new NotificationsSwallowed_OuterModel();
            Action act = () => outer.IsViewingActors = !outer.IsViewingActors;

            act.ShouldNotThrow<SubscriptionFailedException>(Because.CyclicSubscriptionShouldBeTurnedOffSilently);
        }

        #endregion

        #region Cyclic access

        [TestMethod]
        public void SelfAccess_DoesNotCauseCyclicAccess_WhenThePropertyCheckValueEqualityInSetter()
        {
            Action act = () => React.To(() => RaisingIntProperty).Set(() => RaisingIntProperty);

            act.ShouldNotThrow<CyclicAccessException>("property has been set up to be assigned its " +
                "own value, whenever it changes. However, its setter doesn't raise change notifications, " +
                "because the setter first checks whether or not the value has changed");
        }

        [TestMethod]
        public void CyclicAccess_IsDetectedWhileConstructingExpression_WithTurnOffTheSubscription()
        {
            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.TurnOffTheSubscription;
            Action act = () => React.To(() => RaisingIntProperty + 1).Set(() => RaisingIntProperty);

            act.ShouldThrow<SubscriptionFailedException>(Because.SubscriptionIsInvalid)
                .And.InnerException.Should().BeOfType<CyclicAccessException>(Because.CyclicSubscriptionShouldFailImmediately);
        }

        [TestMethod]
        public void CyclicAccess_IsDetectedWhileConstructingExpression_WithTurnOffTheSubscription_AndNoFurtherNotificationsArePublished()
        {
            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.TurnOffTheSubscription;
            Action subscription = () => React.To(() => RaisingIntProperty + 1).Set(() => RaisingIntProperty);

            subscription.ShouldThrow<SubscriptionFailedException>(Because.SubscriptionIsInvalid)
                .And.InnerException.Should().BeOfType<CyclicAccessException>(Because.CyclicSubscriptionShouldFailImmediately);

            Action assignment = () => RaisingIntProperty = 10;
            assignment.ShouldNotThrow<SubscriptionFailedException>(Because.SubscriptionShouldBeTurnedOffSilently);
        }

        [TestMethod]
        public void CyclicAccess_IsDetectedWhileConstructingExpression_WithFailFast()
        {
            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.FailFast;
            Action act = () => React.To(() => RaisingIntProperty + 1).Set(() => RaisingIntProperty);

            act.ShouldThrow<SubscriptionFailedException>(Because.SubscriptionIsInvalid)
                .And.InnerException.Should().BeOfType<CyclicAccessException>(Because.CyclicSubscriptionShouldFailImmediately);
        }

        [TestMethod]
        public void CyclicAccess_IsDetectedWhileConstructingExpression_WithMethodCall_AndTrackLastChild()
        {
            RecursiveInt = RecursiveObject<int>.CreateOfDepth(0);
            Action act = () => React.To(() => RecursiveInt.TrackLastChild().MethodA()).Set(() => RecursiveInt.PropertyA);

            act.ShouldThrow<SubscriptionFailedException>(Because.SubscriptionIsInvalid)
                .And.InnerException.Should().BeOfType<CyclicAccessException>(
                "whenever PropertyA is set it causes the expression " +
                "to be re-evaluated, because of the TrackLastChild marker method. It is a cyclic reference.");
        }

        [TestMethod]
        public void MethodCall_DoesNotCauseCyclicAccess()
        {
            RecursiveInt = RecursiveObject<int>.CreateOfDepth(0);
            Action act = () => React.To(() => RecursiveInt.MethodA()).Set(() => RecursiveInt.PropertyA);

            act.ShouldNotThrow("method call should not track all properties of RecursiveInt " +
                "because it would cause recursive access");
        }

        #endregion

        #region Load / Unload

        //[TestMethod]
        //public void LoadAwareExpression_IsTrackedOnlyIfLoaded()
        //{
        //    var recursive = RecursiveObject<int>.CreateOfDepth(0);
        //    ILoadable loadableInterface = (ILoadable)recursive;
        //    int propertyAValue = -1;

        //    React.To(() => recursive.PropertyA)
        //        .Only
        //        .Subscribe(val => propertyAValue = val);

        //    recursive.PropertyA = 10;

        //    propertyAValue.Should().Be(10, "tracking fields is enabled.");
        //}

        #endregion

        #region Preconfiguration

        [TestMethod]
        public void PreconfiguredWithFieldTracking_TracksAllFields()
        {
            var recursive1 = RecursiveObject<int>.CreateOfDepth(0);
            var recursive2 = RecursiveObject<int>.CreateOfDepth(0);
            recursive1.PropertyA = 10;
            recursive2.PropertyB = 20;
            int log = -1;

            React.Preconfig().TrackFields().To(() => recursive1.PropertyA + recursive2.PropertyB).
                Set(val => log = val);

            log.Should().Be(30, "this is the initial value");

            recursive2.PropertyB = 50;
            log.Should().Be(60, "PropertyB has changed on the object used in expression");

            recursive1.PropertyA = 30;
            log.Should().Be(80, "PropertyA has changed on the object used in expression");
        }

        [TestMethod]
        public void PreconfiguredWithNoFieldTracking_TracksNothing()
        {
            var recursive1 = RecursiveObject<int>.CreateOfDepth(0);
            var recursive2 = RecursiveObject<int>.CreateOfDepth(0);
            recursive1.PropertyA = 10;
            recursive2.PropertyB = 20;
            int log = -1;

            Action act = () => React.Preconfig().DoNotTrackFields().
                To(() => recursive1.PropertyA + recursive2.PropertyB).
                Set(val => log = val);

            act.ShouldThrowBecauseNothingIsTracked();
        }

        [TestMethod]
        public void PreconfiguredWithItemTracking_RaisesNotificationsForIndividualItems()
        {
            People = new ObservableCollection<Person>();
            Rooms = new ObservableCollection<Room>();
            People.Add(new Person());
            Rooms.Add(new Room());
            double log = -1;
            React.Preconfig().TrackItems()
                 .To(() => People.First().Age + Rooms.First().SquareMeters)
                 .Set(val => log = val);

            log.Should().Be(0.0, "these are default values");

            People.First().Age = 10;
            log.Should().Be(10.0, "Age property has been changed on a tracked item");

            Rooms.First().SquareMeters = 20.0;
            log.Should().Be(30.0, "SquareMeters property has been changed on a tracked item");
        }

        [TestMethod]
        public void PreconfiguredWithLastChildTracking_TracksAllPropertiesOfTheLastChild()
        {
            RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            int log = -1;

            React.Preconfig().TrackLastChild().To(() => RecursiveInt.Inner).
                Set(inner => log = inner.PropertyA);

            RecursiveInt.Inner.PropertyA = 10;
            log.Should().Be(10, "all properties on Inner instance should " +
                "be tracked, because of TrackAllProps() call");
        }

        [TestMethod]
        public void PreconfiguredWithNoLastChildTracking_TracksNoPropertiesOfTheLastChild()
        {
            RecursiveInt = RecursiveObject<int>.CreateOfDepth(1);
            int log = -1;

            React.Preconfig().DoNotTrackLastChild().To(() => RecursiveInt.Inner).
                SkipInitial().
                Set(inner => log = inner.PropertyA);

            RecursiveInt.Inner.PropertyA = 10;
            RecursiveInt.Inner.PropertyB = 20;
            log.Should().Be(-1, "changes have been made to Inner property, " +
                "but object stored as value of the Inner property is not tracked, " +
                "because of explicit call to DoNotTrackLastChild().");
        }

        #endregion

        #region React.To() Stream Disposal

        [TestMethod]
        public void SimpleReactToSyntax_AfterDisposal_NoExpressionReevaluation()
        {
            var recursive = RecursiveObject<int>.CreateOfDepth(0);
            int propertyAValue = -1;
            IDisposable stream = React.To(() => recursive.PropertyA)
                .SkipInitial()
                .Subscribe(val => propertyAValue = val);
            stream.Dispose();

            recursive.PropertyA = 10;
            propertyAValue.Should().Be(-1, "stream was disposed before changing PropertyA.");
        }

        [TestMethod]
        public void PreconfiguredWithItemTracking_AfterDisposal_NoExpressionReevaluation()
        {
            People = new ObservableCollection<Person>();
            Rooms = new ObservableCollection<Room>();
            People.Add(new Person());
            Rooms.Add(new Room());
            double log = -1;
            React.Preconfig().TrackItems()
                 .To(() => People.First().Age + Rooms.First().SquareMeters)
                 .SkipInitial()
                 .Set(val => log = val).Dispose();

            People.First().Age = 10;
            log.Should().Be(-1, "stream was disposed - no property change tracking shoul be in place");

            Rooms.First().SquareMeters = 20.0;
            log.Should().Be(-1, "stream was disposed - no property change tracking shoul be in place");
        }

        #endregion

        #region Raw INPC tests

        [TestMethod]
        public void ReevaluatesTarget_WhenReactivePropertyIsBuiltOnINPCObject()
        {
            var productInfo = new ProductInfo() { IsAvailable = true };
            var product = new Product(productInfo);

            product.IsAvailable.Should().Be(true, "this is initial value in product info");
            productInfo.IsAvailable = false;
            product.IsAvailable.Should().Be(false, "value has been set anew via product info instance");
        }

        #endregion

        #region ViaProperty

        #region Normal Property

        class PropertyClass
        {
            public class WithReactToSyntax : ReactiveBase
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="T:WithReactToSyntax"/> class.
                /// </summary>
                public WithReactToSyntax()
                {
                    inputProperty = this.Create(() => Input);
                    outputProperty = React.To(() => Input)
                        .Where(i => i > 0).Select(i => i * 2)
                        .ViaProperty(() => Output).On(this);
                }

                private IProperty<int> inputProperty;
                public int Input
                {
                    get { return inputProperty.Value; }
                    set { inputProperty.Value = value; }
                }

                private IProperty<int> outputProperty;
                public int Output
                {
                    get { return outputProperty.Value; }
                }
            }
        }

        [TestMethod]
        public void ViaProperty_WorksWhenCreatedFromAReactiveStream()
        {
            var instance = new PropertyClass.WithReactToSyntax();

            instance.Output.Should().Be(0, "this default value of Input property should propagate through " +
                "the query based on which the Output property is created");

            instance.Input = -20;
            instance.Output.Should().Be(0, "there is a Where clause that filters out non-positive numbers");

            instance.Input = 15;
            instance.Output.Should().Be(30, "last number set to Input goes through the Where clause filter " +
                "and after that is doubled before the property is assigned to it");
        }

        #endregion

        #region Lazy Property

        class LazyPropertyClass
        {
            public class WithPropertySyntax : ReactiveBase
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="T:WithPropertySyntax"/> class.
                /// </summary>
                public WithPropertySyntax()
                {
                    inputProperty = this.Create(() => Input);
                    outputProperty = this.CreateLazy(() => Output, () => Input());
                }

                private IProperty<int> outputProperty;
                public int Output
                {
                    get { return outputProperty.Value; }
                }

                private PropertyBase<Func<int>> inputProperty;
                public Func<int> Input
                {
                    get { return inputProperty.Value; }
                    set { inputProperty.Value = value; }
                }
            }

            public class WithReactToSyntax : ReactiveBase
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="T:WithReactToSyntax"/> class.
                /// </summary>
                public WithReactToSyntax()
                {
                    inputProperty = this.Create(() => Input);
                    outputProperty = React.To(() => Input()).ViaLazyProperty(() => Output).On(this);
                }

                private IProperty<int> outputProperty;
                public int Output
                {
                    get { return outputProperty.Value; }
                }

                private PropertyBase<Func<int>> inputProperty;
                public Func<int> Input
                {
                    get { return inputProperty.Value; }
                    set { inputProperty.Value = value; }
                }
            }
        }

        [TestMethod]
        public void ViaLazyProperty_WithPropertySyntax_PropertyIsReallyLazy()
        {
            int counter = 0; int value = 10;
            var lazyClass = new LazyPropertyClass.WithPropertySyntax();
            Func<int> func = () => { ++counter; return value; };

            //create a new Func<int> every time to get over equality check 
            lazyClass.Input = () => func();
            lazyClass.Input = () => func();

            counter.Should().Be(0, "evaluator should not yet be called, because the property is lazy");
            lazyClass.Output.Should().Be(value, "although lazy the should be evaluated correctly");
            counter.Should().Be(1, "evaluator should be called only once, because the property is lazy");
        }

        [TestMethod]
        public void ViaLazyProperty_WithReactToSyntax_PropertyIsReallyLazy()
        {
            int counter = 0; int value = 10;
            var lazyClass = new LazyPropertyClass.WithReactToSyntax();
            Func<int> func = () => { ++counter; return value; };

            //create a new Func<int> every time to get over equality check 
            lazyClass.Input = () => func();
            lazyClass.Input = () => func();

            counter.Should().Be(0, "evaluator should not yet be called, because the property is lazy");
            lazyClass.Output.Should().Be(value, "although lazy the should be evaluated correctly");
            counter.Should().Be(1, "evaluator should be called only once, because the property is lazy");
        }

        #endregion

        #endregion
    }
}
