#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xReactor.Tests
{

    #region Constants

    /// <summary>
    /// Contains common reason strings.
    /// </summary>
    class Because
    {
        public const string SubscriptionIsInvalid =
            "subscription was not correct for reasons " +
            "described in detail in the inner exception";

        public const string SubscriptionShouldBeTurnedOffSilently =
            "subscription should be turned off silently";

        public const string CyclicSubscriptionShouldBeTurnedOffSilently =
            "although the expression causes a cyclic reference, " +
            "subscription should be turned off without user-visible exception thrown";

        public const string CyclicSubscriptionShouldFailImmediately =
            "property has been set up to be assigned its " +
            "own value, modified. An attempt to do so should cause cyclic reference to occur " +
            "immediately as initial value is set";

        public const string SwallowingExceptionsIsNotAllowed =
            "it must not be allowed to swallow " +
            "notification requests for the property changed event. It could unexpectedly " +
            "break a recursive cycle without a message or user decision";

        public const string SwallowingExceptionsIsNotAllowed_AndMustBeThrownImmediately =
            SwallowingExceptionsIsNotAllowed +
            ". Appropriate exception should be thrown at the moment of initialising " +
            "expressions that are the root cause. ";

        public const string MethodCallDoesNotAttachListenersByDefault =
            "expression is nowhere marked with TrackLastChild, so method call does not " +
            "cause any property changed handlers to be attached";
    }

    #endregion

}
