#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Common
{
    /// <summary>
    /// A lightweight object, of which creation should not
    /// take a large amount of time.
    /// </summary>
    public struct CollectorStamp
    {
        /// <summary>
        /// Small dummy field
        /// </summary>
        bool Accessible;
    }

    public class CollectorBox:CommonBase
    {
        private Property<ObservableCollection<CollectorStamp>> stampsProperty;
        public ObservableCollection<CollectorStamp> Stamps
        {
            get { return stampsProperty.Value; }
            set { stampsProperty.Value = value; }
        }

        private Property<int> numStampsProperty;
        public int NumStamps
        {
            get { return numStampsProperty.Value; }
            private set { numStampsProperty.Value = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CollectorBox"/> class.
        /// </summary>
        public CollectorBox()
        {
            stampsProperty = this.Create(() => Stamps, new ObservableCollection<CollectorStamp>());
            //Again, as lightweight as possible, no tracking of individual items,
            //but the expression depends on a collection.
            numStampsProperty = this.Create(() => NumStamps, () => Stamps.Count);
        }
    }


    public class TrackingCollectorBox : CommonBase
    {
        private Property<ObservableCollection<CollectorStamp>> stampsProperty;
        public ObservableCollection<CollectorStamp> Stamps
        {
            get { return stampsProperty.Value; }
            set { stampsProperty.Value = value; }
        }

        private Property<int> numStampsProperty;
        public int NumStamps
        {
            get { return numStampsProperty.Value; }
            private set { numStampsProperty.Value = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:CollectorBox"/> class.
        /// </summary>
        public TrackingCollectorBox()
        {
            stampsProperty = this.Create(() => Stamps, new ObservableCollection<CollectorStamp>());

            //Although, logically it has no sense to set TrackItems() here, it's used
            //for comparison purposes.
            numStampsProperty = this.Create(() => NumStamps, () => Stamps.TrackItems().Count);
        }
    }
}
