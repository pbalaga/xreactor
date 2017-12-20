#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace xReactor
{
    /// <summary>
    /// Enables fluent-style preconfiguration
    /// of all follow-up reactive expressions.
    /// Contains methods that affect how and when
    /// change notification handlers are attached
    /// to objects.
    /// </summary>
    public sealed class ExpressionPreconfiguration
    {
        internal TraversalOptions Options { get; set; }

        #region Marker Methods

        /// <summary>
        /// Marks a collection object
        /// that all contained items implementing INPC should
        /// be tracked as well.
        /// </summary>
        /// <returns>The same instance of the <see cref="ExpressionPreconfiguration"/>
        /// type, which holds preconfiguration options</returns>
        public ExpressionPreconfiguration TrackItems()
        {
            this.Options = MarkerMethods.TrackItems(this.Options, PropertyTrackingInfo.TrackAll);
            return this;
        }

        /// <summary>
        /// Marks a collection object
        /// that all contained items implementing INPC should
        /// be tracked as well.
        /// </summary>
        /// <returns>The same instance of the <see cref="ExpressionPreconfiguration"/>
        /// type, which holds preconfiguration options</returns>
        public ExpressionPreconfiguration TrackItems<TItem>(params Expression<Func<TItem, object>>[] propertiesToTrack)
        {
            this.Options = MarkerMethods.TrackItems(this.Options, propertiesToTrack);
            return this;
        }

        /// <summary>
        /// Marks the follow-up expressions 
        /// so that changes applied to any subproperty 
        /// of the last child property should be tracked as well. 
        /// </summary>
        /// <returns>The same instance of the <see cref="ExpressionPreconfiguration"/>
        /// type, which holds preconfiguration options</returns>
        public ExpressionPreconfiguration TrackLastChild()
        {
            this.Options = MarkerMethods.TrackLastChild(this.Options);
            return this;
        }

        /// <summary>
        /// Marks all fields in follow-up expression as trackable,
        /// given that they implement the 
        /// <see cref="INotifyPropertyChanged"/> interface.
        /// </summary>
        /// <returns>The same instance of the <see cref="ExpressionPreconfiguration"/>
        /// type, which holds preconfiguration options</returns>
        public ExpressionPreconfiguration TrackFields()
        {
            this.Options = MarkerMethods.TrackFields(this.Options);
            return this;
        }

        /// <summary>
        /// Marks the follow-up expressions 
        /// so that changes applied to any subproperty 
        /// of the last child property should be NOT tracked as well. 
        /// </summary>
        /// <returns>The same instance of the <see cref="ExpressionPreconfiguration"/>
        /// type, which holds preconfiguration options</returns>
        public ExpressionPreconfiguration DoNotTrackLastChild()
        {
            this.Options = MarkerMethods.DoNotTrackLastChild(this.Options);
            return this;
        }

        /// <summary>
        /// Marks all fields in follow-up expression as NOT trackable,
        /// given that they implement the 
        /// <see cref="INotifyPropertyChanged"/> interface.
        /// </summary>
        /// <returns>The same instance of the <see cref="ExpressionPreconfiguration"/>
        /// type, which holds preconfiguration options</returns>
        public ExpressionPreconfiguration DoNotTrackFields()
        {
            this.Options = MarkerMethods.DoNotTrackFields(this.Options);
            return this;
        }

        #endregion
    }
}
