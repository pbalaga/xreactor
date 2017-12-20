#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Generic;

namespace xReactor.Tests
{
    [TestClass]
    public class ExpressionHelperTests
    {
        const string bothPropertiesShouldBeListed =
                "both properties are defined on INotifyPropertyChanged objects - " +
                "both should be included in the listing. Changing any of these " +
                "may cause a change in the expression value";

        SimpleClass model;

        public int TestProperty1
        {
            get;
            set;
        }

        public int TestProperty2
        {
            get;
            set;
        }

        /// <summary>
        /// Use TestInitialize to run code before running each test 
        /// </summary>
        [TestInitialize()]
        public void InitializeEach()
        {
            model = new SimpleClass();
        }

        #region GetNameFromExpression tests

        [TestMethod]
        public void GetNameFromExpression_ReturnsValidString()
        {
            string name = ExpressionHelper.GetNameFromExpression<int>(() => TestProperty1);
            name.Should().Be("TestProperty1");
        }

        [TestMethod]
        public void GetNameFromExpression_WithNameChain_ReturnsLastElement()
        {
            SimpleClass target = null;
            string name = ExpressionHelper.GetNameFromExpression<double>(() => target.Deep.Number);
            name.Should().Be("Number", "this is the last name in the chain");
        }

        [TestMethod]
        public void GetNameFromExpression_ThrowWhenInputInvalid()
        {
            Action act = () => ExpressionHelper.GetNameFromExpression(
                () => TestProperty2 - TestProperty1);
            act.ShouldThrow<ArgumentException>("expression provided must be like ()=>Name");
        }

        [TestMethod]
        public void GetNameFromExpression_WithParameter_ReturnsValidString()
        {
            string name = ExpressionHelper.GetNameFromExpression<string, int>(s => s.Length);
            name.Should().Be("Length");
        }

        [TestMethod]
        public void GetNameFromExpression_WithParameter_WithNameChain_ReturnsLastElement()
        {
            string name = ExpressionHelper.GetNameFromExpression<SimpleClass, double>((s) => s.Deep.Number);
            name.Should().Be("Number", "this is the last name in the chain");
        }

        [TestMethod]
        public void GetNameFromExpression_WithParameter_ThrowWhenInputInvalid()
        {
            Action act = () => ExpressionHelper.GetNameFromExpression<string, int>(
                (s) => s.Length - TestProperty1);
            act.ShouldThrow<ArgumentException>("expression provided must be like (obj)=>obj.Name");
        }

        #endregion

        #region GetUsedProperties tests

        [TestMethod]
        public void GetUsedProperties_TestConstant()
        {
            GetUsedProperties(model.ConstantExpression()).
                Should().HaveCount(0, "there is no property used");
        }

        [TestMethod]
        public void GetUsedProperties_TestProperty()
        {
            GetUsedPropertyNames(model.PropertyExpression())
                .Should().BeEquivalentTo(new[] { "Score" }, "that should be the name of the property used in PropertyExpression");
        }

        [TestMethod]
        public void GetUsedProperties_TestPropertyTarget()
        {
            GetUsedPropertyTargets(model.PropertyExpression())
                .Should().BeEquivalentTo(new[] { model }, "target is the object on which property is defined");
        }

        [TestMethod]
        public void GetUsedProperties_TestMultiProperty()
        {
            GetUsedPropertyNames(model.MultiPropertyExpression())
                .Should().BeEquivalentTo(new[] { "Score", "Times" },
                "that should be the names of the properties used in MultiPropertyExpression");
        }

        [TestMethod]
        public void GetUsedProperties_TestDeepProperty()
        {
            var used = GetUsedProperties(model.DeepPropertyExpression());

            used.Should().HaveCount(1, "there is 1 top-level property in that expression");
            used.Single().Name.Should().Be("Deep", "this is name of the top-level property");

            used.Single().Child.Should().NotBeNull("it must have a child");
            used.Single().Child.Name.Should().Be("Number", "this is name of the sub-property");
        }

        [TestMethod]
        public void GetUsedProperties_TestMethodCall()
        {
            GetUsedPropertyNames(model.MethodCallExpression())
                .Should().BeEquivalentTo(new[] { "Score", "Text" }, bothPropertiesShouldBeListed);
        }

        [TestMethod]
        public void GetUsedProperties_TestMethodCallTarget()
        {
            GetUsedPropertyTargets(model.MethodCallExpression())
                .Should().BeEquivalentTo(new[] { model });
        }

        [TestMethod]
        public void GetUsedProperties_TestPropertyInMethodCall()
        {
            GetUsedPropertyNames(model.PropertyInMethodCallExpression())
                .Should().BeEquivalentTo(new[] { "Score", "Text" }, bothPropertiesShouldBeListed);
        }

        [TestMethod]
        public void GetUsedProperties_TestPropertyInMethodCallTarget()
        {
            GetUsedPropertyTargets(model.PropertyInMethodCallExpression())
                .Should().BeEquivalentTo(new[] { model });
        }

        [TestMethod]
        public void GetUsedProperties_TestComplexExpressionTargets()
        {
            RecursiveObject<int> recInt = RecursiveObject<int>.CreateOfDepth(1);
            RecursiveObject<string> recStr = RecursiveObject<string>.CreateOfDepth(2);
            RecursiveObject<string> deepestInnerStr = recStr.Inner.Inner;
            string suffix = "suffix";
            recInt.Inner.FieldA = 3;
            recStr.PropertyB = "B";
            deepestInnerStr.PropertyA = "A";

            GetUsedPropertyTargets(
                    () => deepestInnerStr.TrackFields().PropertyA.
                    PadLeft(recInt.TrackFields().PropertyA * 2, ' ') + suffix
                ).Should().BeEquivalentTo(new object[] { recInt, deepestInnerStr });
        }

        #endregion

        #region Utility methods

        IEnumerable<UsedPropertyChain> GetUsedProperties<T>(Expression<Func<T>> valueExpression)
        {
            return ExpressionHelper.GetUsedProperties<T>(valueExpression);
        }

        IEnumerable<string> GetUsedPropertyNames<T>(Expression<Func<T>> valueExpression)
        {
            return ExpressionHelper.GetUsedProperties<T>(valueExpression).SelectNames<T>().Distinct();
        }

        IEnumerable<INotifyPropertyChanged> GetUsedPropertyTargets<T>(Expression<Func<T>> valueExpression)
        {
            return ExpressionHelper.GetUsedProperties<T>(valueExpression).SelectTargets<T>().Distinct();
        }

        #endregion

        #region Utility classes

        class SimpleBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
        }

        class SimpleClass : SimpleBase
        {
            public int Score { get; set; }
            public int Times { get; set; }
            public string Text { get; set; }
            public Subclass Deep { get; set; }

            public Expression<Func<int>> ConstantExpression() { return () => 10; }
            public Expression<Func<int>> PropertyExpression() { return () => Score; }
            public Expression<Func<int>> MultiPropertyExpression() { return () => Score * Times - 10; }
            public Expression<Func<double>> DeepPropertyExpression() { return () => 2 + Deep.Number; }
            public Expression<Func<string>> MethodCallExpression() { return () => Score.ToString() + Text; }
            public Expression<Func<string>> PropertyInMethodCallExpression() { return () => convertToString(Score) + Text; }
            public Expression<Func<double>> CastExpression() { return () => 2 + (int)Deep.Number; }

            public SimpleClass()
            {
                Deep = new Subclass();
            }

            private string convertToString(int score)
            {
                return score.ToString();
            }
        }

        class Subclass : SimpleBase
        {
            public double Number { get; private set; }
        }

        #endregion
    }
}
