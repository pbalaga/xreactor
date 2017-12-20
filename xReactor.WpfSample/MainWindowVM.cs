#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using xReactor.Common;

namespace xReactor.WpfSample
{
    class MainWindowVM : ReactiveBase
    {
        private Property<Person> personProperty;
        public Person Person
        {
            get { return personProperty.Value; }
            set { personProperty.Value = value; }
        }

        private int yearsBelow18;
        /// <summary>
        /// This is a typical implementation of a notifying
        /// property. One can still use this approach, although
        /// there is more boilterplate code.
        /// </summary>
        public int YearsBelow18
        {
            get { return yearsBelow18; }
            set
            {
                if (yearsBelow18 != value)
                {
                    yearsBelow18 = value;
                    this.RaisePropertyChanged(() => YearsBelow18);
                }
            }
        }

        /// <summary>
        /// This is a clean property without any INPC notification code.
        /// Notifications are raised automatically, via SetAndNotify(..) 
        /// method. This method can be used successfully in case of 
        /// properties with a private getter, because there is no need
        /// to raise notification events, if the property is changed 
        /// from outside of this class. Public getter could be used 
        /// effectively too, but it would require accessing the property 
        /// from the outside only via the React.To(...) syntax - with loss 
        /// of generality.
        /// </summary>
        public int YearsLeftToRetirement
        {
            get;
            private set;
        }

        /// <summary>
        /// Same as above, but a change to this property
        /// will require a switch of DataTemplate.
        /// </summary>
        public OperationPanelVM OperationPanel
        {
            get;
            private set;
        }

        public InMemoryLogger Logger
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:MainWindowVM"/> class.
        /// </summary>
        public MainWindowVM()
        {
            this.Logger = new InMemoryLogger();

            OperationPanelVM adultPanel = new AdultPanelVM(this.Logger);
            OperationPanelVM youngsterPanel = new YoungsterPanelVM(this.Logger);

            personProperty = this.Create(() => Person, new Person());
            React.To(() => 18 - Person.Age).Where(age => age > 0).Set(age => YearsBelow18 = age);
            React.To(() => 70 - Person.Age).SetAndNotify(() => YearsLeftToRetirement);

            //One can use Select() to convert the value stream and then call SetAndNotify()
            //with the target property as an argument. Thus, still no need for manual INPC event raising.
            //React.To(() => Person.Age >= 18).Select(isAdult => isAdult ? adultPanel : youngsterPanel).
            //      SetAndNotify(() => OperationPanel);

            //The same result can be obtained by using the Switch extension method.
            React.To(() => Person.Age).Switch(age => age >= 18, adultPanel, youngsterPanel).
                  SetAndNotify(() => OperationPanel);
        }
    }
}
