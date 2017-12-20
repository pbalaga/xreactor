using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Tests
{
    public static class StaticClass
    {
        public static int FieldA;

        public static int PropertyA
        {
            get { return FieldA; }
            set
            {
                FieldA = value;
                //Raising notifications is impossible on a static class
            }
        }

        public static int MethodA()
        {
            return FieldA;
        }

        public static RecursiveObject<int> RecursiveField;

        public static RecursiveObject<int> RecursiveProperty
        {
            get { return RecursiveField; }
            set { RecursiveField = value; }
        }
    }
}
