#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Common
{
    public class Person : CommonBase
    {
        private Property<string> nameProperty;
        public string Name
        {
            get { return nameProperty.Value; }
            set { nameProperty.Value = value; }
        }

        private PropertyBase<int> ageProperty;
        public int Age
        {
            get { return ageProperty.Value; }
            set { ageProperty.Value = value; }
        }

        private Property<bool> isKidProperty;
        public bool IsKid
        {
            get { return isKidProperty.Value; }
            private set { isKidProperty.Value = value; }
        }

        private Property<bool> isTeenagerProperty;
        public bool IsTeenager
        {
            get { return isTeenagerProperty.Value; }
            private set { isTeenagerProperty.Value = value; }
        }

        private Property<string> cardStringProperty;
        public string CardString
        {
            get { return cardStringProperty.Value; }
            private set { cardStringProperty.Value = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Person"/> class.
        /// </summary>
        public Person()
        {
            nameProperty = this.Create(() => Name, string.Empty);
            ageProperty = this.Create<int>(() => Age).Coerce(age => Math.Max(age, 0));
            cardStringProperty = this.Create(() => CardString,
                () => string.Format("Name: {0}, Age: {1}", Name, Age));

            //Stream of property changes example:
            isKidProperty = this.Create<bool>(() => IsKid);
            ageProperty.GetStream().Subscribe((age) => IsKid = age < 10);

            //Streams like that one above can be merged and processed like
            //every Rx stream.

            //Here is a similar property, but initialized differently.
            //This method is shorter for simple logic.
            //Notice lack of explicit <bool> type parameter.
            isTeenagerProperty = this.Create(() => IsTeenager, () => Age >= 10 && Age < 20);
        }
    }

}
