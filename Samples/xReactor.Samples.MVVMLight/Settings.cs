using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace xReactor.Samples.MVVMLight
{
    public class Settings : ObservableObject
    {
        static Settings current;

        static Settings()
        {
            current = new Settings();
            var local = current;
            local.DecimalPlaces = 1;

            React.To(() => current.DecimalPlaces)
                .Set(places => local.NumberFormat = string.Format("{{0:f{0}}}", places));
        }

        public static Settings Current { get { return current; } }

        private int decimalPlaces;

        public int DecimalPlaces
        {
            get { return decimalPlaces; }
            set
            {
                value = Math.Max(0, value);

                if (decimalPlaces != value)
                {
                    decimalPlaces = value;
                    RaisePropertyChanged(() => DecimalPlaces);
                }
            }
        }

        private string numberFormat;

        public string NumberFormat
        {
            get { return numberFormat; }
            private set
            {
                if (numberFormat != value)
                {
                    numberFormat = value;
                    RaisePropertyChanged(() => NumberFormat);
                }
            }
        }
    }
}
