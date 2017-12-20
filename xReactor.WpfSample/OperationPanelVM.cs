#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace xReactor.WpfSample
{
    abstract public class OperationPanelVM : NotifyPropertyChangedBase
    {
        public string Name
        {
            get;
            set;
        }
    }

    public class AdultPanelVM : OperationPanelVM
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:AdultPanelVM"/> class.
        /// </summary>
        public AdultPanelVM(ILogger logger)
        {
            this.Name = "Adult control panel";
            this.DrinkBeer = new RelayCommand(p => logger.WriteLine("I'm drunk already!"));
            this.Drive = new RelayCommand(p => logger.WriteLine("Grabbed the wheel."));
        }

        public ICommand DrinkBeer { get; private set; }
        public ICommand Drive { get; private set; }

    }

    public class YoungsterPanelVM : OperationPanelVM
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:YoungsterPanelVM"/> class.
        /// </summary>
        public YoungsterPanelVM(ILogger logger)
        {
            this.Name = "Youngster control panel";
            this.WatchCartoons = new RelayCommand(p => logger.WriteLine("Silcence for an hour..."));
            this.Cry = new RelayCommand(p => logger.WriteLine("No silence at all..."));
        }

        public ICommand WatchCartoons { get; private set; }
        public ICommand Cry { get; private set; }
    }
}
