#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    internal struct PropertyTrackingInfo
    {
        private static readonly string[] EmptyArray = new string[0];

        public bool AreAllTracked;
        public string[] Tracked;

        public static PropertyTrackingInfo TrackNone
        {
            get
            {
                return new PropertyTrackingInfo()
                {
                    AreAllTracked = false,
                    Tracked = EmptyArray
                };
            }
        }

        public static PropertyTrackingInfo TrackAll
        {
            get
            {
                return new PropertyTrackingInfo()
                {
                    AreAllTracked = true,
                    Tracked = EmptyArray
                };
            }
        }

        public static PropertyTrackingInfo FromStringArray(string[] propertiesTracked)
        {
            return new PropertyTrackingInfo()
            {
                AreAllTracked = false,
                Tracked = propertiesTracked
            };
        }
    }

    internal struct TraversalOptions
    {
        public CollectionTrackOptions CollectionTrackOptions;
        public bool TrackFields;
        public bool TrackLastChild;
        public PropertyTrackingInfo Properties;

        public bool TracksCollectionItems
        {
            get
            {
                return CollectionTrackOptions == xReactor.CollectionTrackOptions.TrackItems;
            }
        }

        public static TraversalOptions Default()
        {
            return new TraversalOptions
            {
                CollectionTrackOptions = CollectionTrackOptions.TrackCollectionLevelChanges,
                TrackFields = true,
                TrackLastChild = false,
                Properties = PropertyTrackingInfo.TrackNone,
            };
        }
    }
}
