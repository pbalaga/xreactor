using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    public interface IRaisePropertyChanged
    {
        void RaisePropertyChanged(PropertyChangedEventArgs args);
    }
}
