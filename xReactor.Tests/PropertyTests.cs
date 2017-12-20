#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Reactive.Linq;
using System.Collections.ObjectModel;
using System.Linq;
using xReactor.Common;

namespace xReactor.Tests
{
    [TestClass]
    public class PropertyTests : ReactiveTestBase
    {
        #region Properties

        CollectionOfCollections CollectionOfCollectionsObject { get; set; }
        ObservableCollection<Person> People { get; set; }

        [TestCleanup]
        public void CleanupAfterEveryTest()
        {
            ClearPropertyChangedHandlers();
            People = null;
            CollectionOfCollectionsObject = null;
        }

        #endregion

        #region Stream tests

        [TestMethod]
        public void SubscriberGetsNotified()
        {
            var room = CreateRoom();
            string receivedValue = null;
            room.NameProperty.GetStream().Subscribe((str) => receivedValue = str);

            room.Name = "Mario's room";
            receivedValue.Should().Be("Mario's room", "subscriber should have received exactly this data");
        }

        [TestMethod]
        public void DisposedSubscriberDoesNotGetNotified()
        {
            var room = CreateRoom();
            string receivedValue = null;
            var subscription = room.NameProperty.GetStream().Subscribe((str) => receivedValue = str);
            subscription.Dispose();
            room.Name = "Mario's room";
            receivedValue.Should().Be(null, "subscriber should not have received any data (it's disposed)");
        }

        #endregion

        #region Collection tests

        [TestMethod]
        public void TrackedCollection_RaisesNotificationsForIndividualItems()
        {
            People = new ObservableCollection<Person>();
            int age = -1;
            React.To(() => People.TrackItems())
                 .Where(c => c.Any())
                 .Set(c => age = c.First().Age);

            var person = new Person() { Age = 20 };
            People.Add(person);
            person.Age = 30;

            age.Should().Be(30, "age has been changed and items ought to be tracked");
        }

        #endregion

        #region Collection of collections test

        [TestMethod]
        public void NonTrackedCollectionOfCollections_DoesNotPopulateEvenOuterCollectionItemChanges()
        {
            CollectionOfCollectionsObject = new CollectionOfCollections();
            var inner = new ObservableCollection<Person>();
            int age = -1;
            CollectionOfCollectionsObject.DeepCollection.Add(inner);
            React.To(() => CollectionOfCollectionsObject.DeepCollection.First().FirstOrDefault()).
                  Where(p => p != null).Set(p => age = p.Age);

            var person = new Person() { Age = 10 };
            inner.Add(person);

            age.Should().Be(-1, "item tracking is off. Even items of the outer collection " +
                "should not be listened to.");

            person.Age = 20;
            age.Should().Be(-1, "item tracking is off");
        }

        [TestMethod]
        public void TrackedCollectionOfCollections_DoesNotPopulateInnerCollectionItemChanges()
        {
            CollectionOfCollectionsObject = new CollectionOfCollections();
            var inner = new ObservableCollection<Person>();
            int age = -1;
            CollectionOfCollectionsObject.DeepCollection.Add(inner);
            React.To(() => CollectionOfCollectionsObject.DeepCollection
                          .TrackItems().First().FirstOrDefault())
                 .Where(p => p != null).Set(p => age = p.Age);

            var person = new Person() { Age = 10 };
            inner.Add(person);
            
            age.Should().Be(10, "as person has been added to the inner collection, its Count has changed. " +
                "Expression should be reevaluated.");

            person.Age = 20;
            age.Should().Be(10, "person is located in the inner collection and property changes of " +
                "individual (inner) items should not be tracked.");
        }

        class CollectionOfCollections : ReactiveBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="T:CollectionOfCollections"/> class.
            /// </summary>
            public CollectionOfCollections()
            {
                DeepCollection = new ObservableCollection<ObservableCollection<Person>>();
            }

            public ObservableCollection<ObservableCollection<Person>> DeepCollection
            {
                get;
                private set;
            }
        }

        #endregion

        //[TestMethod]
        //public void NotDisposedButCollectedSubscriberDoesNotGetNotified()
        //{
        //    var room = CreateRoom();
        //    string receivedValue = null;
        //    var weakAction = new WeakReference<Action<string>>((str) => receivedValue = str);
        //    var subscription = room.NameProperty.GetStream().Subscribe(weakAction.);
        //    subscription = null;

        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //    room.Name = "Mario's room";
        //    receivedValue.Should().Be(null, "subscriber should not have received any data (it's GC-ed)");
        //}
    }
}
