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
using System.ComponentModel;
using System.Linq.Expressions;

namespace xReactor.Tests
{
    /// <summary>
    /// Observable object not implementing <see cref="IRaisePropertyChanged"/> interface.
    /// </summary>
    class AnotherINPCImplementation : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> nameExpression)
        {
            string propertyName = ExpressionHelper.GetNameFromExpression(nameExpression);
            RaisePropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    class NotificationsSwallowed_InnerModel : AnotherINPCImplementation
    {
        public NotificationsSwallowed_InnerModel(NotificationsSwallowed_OuterModel outerModel)
        {
            this.OuterModel = outerModel;

            React.To(() => this.Age - OuterModel.AverageAge)
                .Select(diff =>
                {
                    if (diff > 0) return new AnyReferenceType(-90.0);
                    else if (diff == 0) return new AnyReferenceType(0.0);
                    else return new AnyReferenceType(90.0);
                })
                .SetAndNotify(() => ArrowIndicatorRotation);
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

        public AnyReferenceType ArrowIndicatorRotation
        {
            get;
            private set;
        }

        public NotificationsSwallowed_OuterModel OuterModel
        {
            get;
            private set;
        }
    }

    class NotificationsSwallowed_OuterModel : ReactiveBase
    {
        public NotificationsSwallowed_OuterModel()
        {
            actors = new ObservableCollection<NotificationsSwallowed_InnerModel>();
            actors.Add(new NotificationsSwallowed_InnerModel(this) { Age = 54 });
            actors.Add(new NotificationsSwallowed_InnerModel(this) { Age = 32, });
            actors.Add(new NotificationsSwallowed_InnerModel(this) { Age = 61 });

            filmDirectors = new ObservableCollection<NotificationsSwallowed_InnerModel>();
            filmDirectors.Add(new NotificationsSwallowed_InnerModel(this) { Age = 54 });
            filmDirectors.Add(new NotificationsSwallowed_InnerModel(this) { Age = 52 });
            filmDirectors.Add(new NotificationsSwallowed_InnerModel(this) { Age = 50 });

            React.To(() => IsViewingActors ? actors : filmDirectors)
                .Set(() => People);

            React.To(() => People.TrackItems())
                .Where(people => people.Any())
                .Select(people => people.Average(p => p.Age))
                .SetAndNotify(() => AverageAge);
        }

        private ObservableCollection<NotificationsSwallowed_InnerModel> actors;
        private ObservableCollection<NotificationsSwallowed_InnerModel> filmDirectors;
        private ObservableCollection<NotificationsSwallowed_InnerModel> people;

        public ObservableCollection<NotificationsSwallowed_InnerModel> People
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
