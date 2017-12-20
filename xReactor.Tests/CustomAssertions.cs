#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xReactor;
using xReactor.Tests;

namespace FluentAssertions
{
    public static class CustomAssertions
    {
        public static Specialized.ExceptionAssertions<Exception>
            ShouldThrowBecauseNothingIsTracked(this Action action, string additionalMessage = "")
        {
            return action.ShouldThrow<Exception>(
                "expression tracks nothing, so the user should be notified. " +
                Environment.NewLine + additionalMessage
                );
        }

        public static void RegardlessOfExceptionHandlingPolicy(this Action assertion, Action cleanupCase)
        {
            cleanupCase = cleanupCase ?? new Action(() => { });

            ExceptionHandlingPolicy defaultSettings = DiagnosticSettings.Custom.ExceptionHandlingPolicy;

            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.TurnOffTheSubscription;
            assertion(); cleanupCase();

            DiagnosticSettings.Custom.ExceptionHandlingPolicy = ExceptionHandlingPolicy.FailFast;
            assertion(); cleanupCase();

            DiagnosticSettings.Custom.ExceptionHandlingPolicy = defaultSettings;
        }

        public static void SubscriptionShouldFailWithInner<ExceptionT>(
            this Action action,
            Action cleanupCase = null,
            string reason = "",
            params object[] reasonArgs
            )
            where ExceptionT : Exception
        {
            RegardlessOfExceptionHandlingPolicy(() =>
                action.ShouldThrow<SubscriptionFailedException>(Because.SubscriptionIsInvalid)
                .And.InnerException.Should().BeOfType<CyclicAccessException>(reason, reasonArgs),
                cleanupCase
                );
        }
    }
}
