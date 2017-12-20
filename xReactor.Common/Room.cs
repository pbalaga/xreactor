#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;

namespace xReactor.Common
{
    public class Room : CommonBase
    {
        public Property<string> NameProperty;
        public string Name
        {
            get { return NameProperty.Value; }
            set { NameProperty.Value = value; }
        }

        private Property<double> sqmProperty;
        public double SquareMeters
        {
            get { return sqmProperty.Value; }
            set { sqmProperty.Value = value; }
        }

        private Property<int> peopleLimitProperty;
        public int PeopleLimit
        {
            get { return peopleLimitProperty.Value; } //read-only
        }

        private Property<ObservableCollection<Furniture>> furnitureProperty;
        public ObservableCollection<Furniture> Furniture
        {
            get { return furnitureProperty.Value; }
        }

        private Property<int> numFurnitureProperty;
        public int NumFurniture
        {
            get { return numFurnitureProperty.Value; }
        }

        private Property<Sitable> largestPlaceToSitProperty;
        public Sitable LargestPlaceToSit
        {
            get { return largestPlaceToSitProperty.Value; }
        }

        private Property<Sitable> smallestPlaceToSitProperty;
        public Sitable SmallestPlaceToSit
        {
            get { return smallestPlaceToSitProperty.Value; }
        }

        private Property<Person> ownerProperty;
        public Person Owner
        {
            get { return ownerProperty.Value; }
            set { ownerProperty.Value = value; }
        }

        private Property<ObservableCollection<Person>> guestsProperty;
        public ObservableCollection<Person> Guests
        {
            get { return guestsProperty.Value; }
        }

        private Property<int> numGuestsProperty;
        public int NumGuests
        {
            get { return numGuestsProperty.Value; }
        }

        private Property<int> ownerAgeProperty;
        public int OwnerAge
        {
            get { return ownerAgeProperty.Value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Room"/> class.
        /// </summary>
        public Room()
        {
            NameProperty = this.Create(() => Name, "VIP room");
            sqmProperty = this.Create(() => SquareMeters, 0.0);
            peopleLimitProperty = this.Create(() => PeopleLimit, () => (int)SquareMeters / 10);
            furnitureProperty = this.Create(() => Furniture, new ObservableCollection<Furniture>());
            numFurnitureProperty = this.Create(() => NumFurniture, () => Furniture.Count);

            largestPlaceToSitProperty = this.Create(() => LargestPlaceToSit,
                () => Furniture.TrackItems().OfType<Sitable>().MaxByOrDefault(f => f.NumSeats));

            //very similar, but uses no TrackItems()
            smallestPlaceToSitProperty = this.Create(() => SmallestPlaceToSit,
                () => Furniture.OfType<Sitable>().MinByOrDefault(f => f.NumSeats));

            guestsProperty = this.Create(() => Guests, new ObservableCollection<Person>());
            numGuestsProperty = this.Create(() => NumGuests, () => Guests.Count);

            ownerProperty = this.Create<Person>(() => Owner);
            ownerAgeProperty =
                ownerAgeProperty = this.Create(() => OwnerAge, () => Owner == null ? 0 : Owner.Age);
        }
    }

}
