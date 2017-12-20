#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace xReactor.Tests
{
    [TestClass]
    public class PropertyValidationTest
    {
        [TestMethod]
        public void WhenRequirementIsMet_NothingHappens()
        {
            Property<double> property = CreateMockProperty<double>();
            property.Value = 10;
            property.Require(val => val > 0, "must be greater than 0.");
            Action act = () => property.Value = 25;
            act.ShouldNotThrow("the value set is feasible");
        }

        [TestMethod]
        public void WhenRequirementIsNotMet_ExceptionIsThrown()
        {
            Property<double> property = CreateMockProperty<double>();
            property.Value = 10;
            property.Require(val => val > 0, "must be greater than 0.");
            Action act = () => property.Value = -1;
            act.ShouldThrow<ArgumentException>("the value is not in feasible domain");
        }

        [TestMethod]
        public void WhenRequirementIsNotMetButCoercionIsSet_ExceptionIsNotThrown()
        {
            Property<double> property = CreateMockProperty<double>();
            property.Coerce(val => Math.Max(val, 0));
            Action act = () => property.Value = -1;
            act.ShouldNotThrow("the value should be coerced instead of throwing exceptions");
        }

        [TestMethod]
        public void WhenRequirementIsNotMetButCoercionIsSet_ValueIsCoerced()
        {
            Property<double> property = CreateMockProperty<double>();
            property.Coerce(val => Math.Max(val, 0));
            property.Value = -1;
            property.Value.Should().Be(0, "the value should be coerced");
        }

        private static Property<T> CreateMockProperty<T>()
        {
            return new Property<T>(
                new Reactor(new ReactiveBase(), null, null),
                "TestPropertyName");
        }
    }
}
