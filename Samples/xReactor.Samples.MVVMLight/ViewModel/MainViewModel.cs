#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace xReactor.Samples.MVVMLight.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : CustomViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.AddPlaceholderTabCommand = new RelayCommand(() => this.Tabs.Add(new PlaceholderTab(this)));

            PeopleViewModel peopleViewModel = new PeopleViewModel(this);

            this.Tabs = new ObservableCollection<TabViewModel>();
            this.Tabs.Add(peopleViewModel);
            this.Tabs.Add(new PlaceholderTab(this));
            this.Tabs.Add(new PlaceholderTab(this));

            //if (IsInDesignMode)
            //{
            //    // Code runs in Blend --> create design time data.
            //}
            //else
            //{
            //    // Code runs "for real"
            //}
        }

        public ObservableCollection<TabViewModel> Tabs
        {
            get;
            private set;
        }

        private TabViewModel activeTab;

        public TabViewModel ActiveTab
        {
            get { return activeTab; }
            set
            {
                if (activeTab != value)
                {
                    activeTab = value;
                    RaisePropertyChanged(() => ActiveTab);
                }
            }
        }

        public ICommand AddPlaceholderTabCommand
        {
            get;
            private set;
        }   
    }
}