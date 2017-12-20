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
using System.Reactive.Linq;

namespace xReactor.Tests
{
    class CyclicAccess_InnerModel : ReactiveBase
    {
        public CyclicAccess_InnerModel(CyclicAccess_OuterModel outerModel)
            {
                this.OuterModel = outerModel;

                React.To(() => this.Age - OuterModel.AverageAge)
                    .Select(diff =>
                    {
                        if (diff > 0) return new AnyReferenceType(-90.0);
                        else if (diff == 0) return new AnyReferenceType(0.0);
                        else return new AnyReferenceType(90.0);
                    })
                    .Set(() => ArrowIndicatorRotation);
            }

            private int age;
            public int Age
            {
                get { return age; }
                set
                {
                    value = Math.Max(0, value);

                    if (age != value)
                    {
                        age = value;
                        RaisePropertyChanged(() => Age);
                    }
                }
            }

            private AnyReferenceType arrowIndicatorRotation;
            public AnyReferenceType ArrowIndicatorRotation
            {
                get { return arrowIndicatorRotation; }
                set
                {
                    //if (arrowIndicatorRotation != value)
                    {
                        arrowIndicatorRotation = value;
                        RaisePropertyChanged(() => ArrowIndicatorRotation);
                    }
                }
            }

            public CyclicAccess_OuterModel OuterModel
            {
                get;
                private set;
            }
        }
    
    class CyclicAccess_OuterModel : ReactiveBase
    {
        public CyclicAccess_OuterModel()
        {
            actors = new ObservableCollection<CyclicAccess_InnerModel>();
            actors.Add(new CyclicAccess_InnerModel(this) { Age = 54});
            actors.Add(new CyclicAccess_InnerModel(this) { Age = 32,});
            actors.Add(new CyclicAccess_InnerModel(this) { Age = 61 });

            filmDirectors = new ObservableCollection<CyclicAccess_InnerModel>();
            filmDirectors.Add(new CyclicAccess_InnerModel(this) { Age = 54 });
            filmDirectors.Add(new CyclicAccess_InnerModel(this) { Age = 52 });
            filmDirectors.Add(new CyclicAccess_InnerModel(this) { Age = 50});

            React.To(() => IsViewingActors ? actors : filmDirectors)
                .Set(() => People);

            React.To(() => People.TrackItems())
                .Where(people => people.Any())
                .Select(people => people.Average(p => p.Age))
                .SetAndNotify(() => AverageAge);
        }

        private ObservableCollection<CyclicAccess_InnerModel> actors;
        private ObservableCollection<CyclicAccess_InnerModel> filmDirectors;
        private ObservableCollection<CyclicAccess_InnerModel> people;

        public ObservableCollection<CyclicAccess_InnerModel> People
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

        public double AverageAge
        {
            get;
            private set;
        }
    }
}
