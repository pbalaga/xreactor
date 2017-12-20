#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using GalaSoft.MvvmLight;

namespace xReactor.Samples.MVVMLight.ViewModel
{
    abstract public class TabViewModel : CustomViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:TabViewModel"/> class.
        /// </summary>
        public TabViewModel(MainViewModel mainViewModel)
        {
            if (mainViewModel == null)
                throw new ArgumentNullException("mainViewModel");
            this.MainViewModel = mainViewModel;

            React.To(() => mainViewModel.ActiveTab == this)
                .Set(() => IsActive);

            React.To(() => IsActive)
                .Switch(true, Brushes.Orange, Brushes.Gray)
                .Set(() => HeaderBackgroundColor);

            React.To(() => IsActive ? Header + " [active]" : Header)
                .Set(() => ExtendedHeader);

            //TODO: allow tracked expressions in switch method
            //(maybe make another method - ReactiveSwitch not to confuse).
            //Most of the time reactive behaviour would be expected, though.

            //React.To(() => IsActive)
            //    .Switch(true, () => Header + " [active]", () => Header)
            //    .Set(() => ExtendedHeader);
        }

        public MainViewModel MainViewModel
        {
            get;
            private set;
        }

        private string header;

        public string Header
        {
            get { return header; }
            protected set
            {
                if (header != value)
                {
                    header = value;
                    RaisePropertyChanged(() => Header);
                }
            }
        }

        private string extendedHeader;
        /// <summary>
        /// One might wonder why properties like that are put
        /// into a view model. They don't have to. Typically,
        /// it'd be done with converters in XAML. View model
        /// logic can successfully replace converters without 
        /// loss of testability, though. Converters consist of
        /// a lot of boiler-plate code and are quite verbose
        /// in nature.
        /// </summary>
        public string ExtendedHeader
        {
            get { return extendedHeader; }
            set
            {
                if (extendedHeader != value)
                {
                    extendedHeader = value;
                    RaisePropertyChanged(() => ExtendedHeader);
                }
            }
        }

        private bool isActive;

        public bool IsActive
        {
            get { return isActive; }
            private set
            {
                if (isActive != value)
                {
                    isActive = value;
                    RaisePropertyChanged(() => IsActive);
                }
            }
        }

        private Brush headerBackgroundColor;

        public Brush HeaderBackgroundColor
        {
            get { return headerBackgroundColor; }
            private set
            {
                if (headerBackgroundColor != value)
                {
                    headerBackgroundColor = value;
                    RaisePropertyChanged(() => HeaderBackgroundColor);
                }
            }
        }
    }
}
