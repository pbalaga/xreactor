#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using System.Reactive.Linq;
using System.Windows.Media;

namespace xReactor.Samples.MVVMLight.ViewModel
{
    public class PersonViewModel : CustomViewModelBase
    {
        public PersonViewModel(PeopleViewModel peopleViewModel)
        {
            if (peopleViewModel == null)
                throw new ArgumentNullException("peopleViewModel");

            this.PeopleViewModel = peopleViewModel;

            React.To(() => this.Age - PeopleViewModel.AverageAge)
                .Select(diff =>
                {
                    if (diff > 0) return "above average";
                    else if (diff == 0) return "exact average";
                    else return "below average";
                })
                .SetAndNotify(() => ComparedToAverageAge);

            //TODO: SetAndNotify usually has no equality check on the new value.
            //Put such logic in SetAndNotify itself.
            React.To(() => this.Age - PeopleViewModel.AverageAge)
                .Select(diff =>
                {
                    if (diff > 0) return new RotateTransform(-90.0);
                    else if (diff == 0) return new RotateTransform(0.0);
                    else return new RotateTransform(90.0);
                })
                .SetAndNotify(() => ArrowIndicatorRotation);

            React.To(() => this.Age - PeopleViewModel.AverageAge)
                .Select(diff =>
                {
                    if (diff > 0) return Brushes.Green;
                    else if (diff == 0) return Brushes.Blue;
                    else return Brushes.Red;
                })
                .SetAndNotify(() => ArrowIndicatorColor);
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

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                if (name != value)
                {
                    name = value;
                    RaisePropertyChanged(() => Name);
                }
            }
        }

        public string ComparedToAverageAge
        {
            get;
            private set;
        }

        public RotateTransform ArrowIndicatorRotation
        {
            get;
            private set;
        }

        public Brush ArrowIndicatorColor
        {
            get;
            private set;
        }

        public PeopleViewModel PeopleViewModel
        {
            get;
            private set;
        }
    }
}
