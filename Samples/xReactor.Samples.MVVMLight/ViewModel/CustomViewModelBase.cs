using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace xReactor.Samples.MVVMLight.ViewModel
{
    public class CustomViewModelBase : ViewModelBase, IRaisePropertyChanged
    {
        void IRaisePropertyChanged.RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            this.RaisePropertyChanged(args.PropertyName);
        }
    }
}
