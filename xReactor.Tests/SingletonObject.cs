using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Tests
{
    public class SingletonObject : NotifyPropertyChangedBase
    {
        public readonly static SingletonObject InstanceField = new SingletonObject();
        public static SingletonObject InstanceProperty { get { return InstanceField; } }

        public int IntField;

        public int IntProperty
        {
            get { return IntField; }
            set
            {
                if (IntField != value)
                {
                    IntField = value;
                    RaisePropertyChanged(() => IntProperty);
                }
            }
        }
    }
}
