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
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Reactive.Linq;

namespace xReactor.Samples.MVVMLight.ViewModel
{
    public class PeopleViewModel : TabViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:PeopleViewModel"/> class.
        /// </summary>
        public PeopleViewModel(MainViewModel mainViewModel)
            : base(mainViewModel)
        {
            this.Header = "People";
            this.AddPersonCommand = new RelayCommand(AddPerson);

            actors = new ObservableCollection<PersonViewModel>();   
            actors.Add(new PersonViewModel(this) { Age = 54, Name = "Kevin Spacey" });
            actors.Add(new PersonViewModel(this) { Age = 32, Name = "Joseph Gordon-Levitt" });
            actors.Add(new PersonViewModel(this) { Age = 61, Name = "Mickey Rourke" });

            filmDirectors = new ObservableCollection<PersonViewModel>();
            filmDirectors.Add(new PersonViewModel(this) { Age = 54, Name = "Frank Darabont" });
            filmDirectors.Add(new PersonViewModel(this) { Age = 52, Name = "Peter Jackson" });
            filmDirectors.Add(new PersonViewModel(this) { Age = 50, Name = "Quentin Tarantino" });
                
            React.To(() => IsViewingActors ? actors : filmDirectors)
                .Set(() => People);

            React.To(() => People.TrackItems(p => p.Age))
                .Where(people => people.Any())
                .Select(people => people.Average(p => p.Age))
                .SetAndNotify(() => AverageAge);

            var settings = Settings.Current;
            React.To(() => string.Format("Average age:" + settings.NumberFormat, AverageAge))
                .SetAndNotify(() => AverageAgeString);
        }

        private ObservableCollection<PersonViewModel> actors;
        private ObservableCollection<PersonViewModel> filmDirectors;
        private ObservableCollection<PersonViewModel> people;

        public ObservableCollection<PersonViewModel> People
        {
            get { return people; }
            private set
            {
                if (people != value)
                {
                    people = value;
                    RaisePropertyChanged(() => People);
                }
            }
        }

        private bool isViewingActors;

        public bool IsViewingActors
        {
            get { return isViewingActors; }
            set
            {
                if (isViewingActors != value)
                {
                    isViewingActors = value;
                    RaisePropertyChanged(() => IsViewingActors);
                }
            }
        }

        //private double averageAge;

        //public double AverageAge
        //{
        //    get { return averageAge; }
        //    set
        //    {
        //        if (averageAge != value)
        //        {
        //            averageAge = value;
        //            RaisePropertyChanged(() => AverageAge);
        //        }
        //    }
        //}

        //private string averageAgeString;

        //public string AverageAgeString
        //{
        //    get { return averageAgeString; }
        //    set
        //    {
        //        if (averageAgeString != value)
        //        {
        //            averageAgeString = value;
        //            RaisePropertyChanged(() => AverageAgeString);
        //        }
        //    }
        //}

        public double AverageAge
        {
            get;
            private set;
        }

        public string AverageAgeString
        {
            get;
            private set;
        }

        private PersonViewModel selectedPerson;

        public PersonViewModel SelectedPerson
        {
            get { return selectedPerson; }
            set
            {
                if (selectedPerson != value)
                {
                    selectedPerson = value;
                    RaisePropertyChanged(() => SelectedPerson);
                }
            }
        }

        public ICommand AddPersonCommand
        {
            get;
            private set;
        }

        private void AddPerson()
        {
            this.People.Add(new PersonViewModel(this));
        }
    }
}
