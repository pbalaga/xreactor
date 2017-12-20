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
using Agdur;
using xReactor.Common;

namespace xReactor.Tests.Benchmarks
{
    public static class Benchmarks
    {
        static Expression<Func<string>> expression1 = GetTestExpression1();

        static Expression<Func<string>> GetTestExpression1()
        {
            Room room = new Room();

            return () => string.Format("{0} with {1} pieces of furniture",
                 room.Name, room.Furniture.Count);
        }

        [Benchmark("rx object creation")]
        static void BenchmarkReactiveObjectCreation()
        {
            new Room();
        }

        [Benchmark("lightweight rx object creation")]
        static void BenchmarkLightweightReactiveObjectCreation()
        {
            new CollectorBox();
        }

        [Benchmark("expression compilation")]
        static void BenchmarkExpressionCompilation()
        {
            expression1.Compile();
        }

        [Benchmark("expression tree parsing")]
        static void BenchmarkExpressionTreeParsing()
        {
            ExpressionHelper.GetUsedPropertiesAndAttachListeners(expression1);
        }

        [Benchmark("collection: adding items on a lightweight object (no tracking)")]
        static void BenchmarkLightweightCollectionItemsAdded()
        {
            var room = new CollectorBox();

            for (int index = 0; index < Program.DefaultNumInstances; index++)
            {
                room.Stamps.Add(new CollectorStamp());
            }
        }

        [Benchmark("collection: adding items on a lightweight object with tracking")]
        static void BenchmarkLightweightTrackingCollectionItemsAdded()
        {
            var room = new TrackingCollectorBox();

            for (int index = 0; index < Program.DefaultNumInstances; index++)
            {
                room.Stamps.Add(new CollectorStamp());
            }
        }

        [Benchmark("collection: adding items (no tracking)")]
        static void BenchmarkCollectionItemsAdded()
        {
            var room = new Room();

            for (int index = 0; index < Program.DefaultNumInstances; index++)
            {
                //room.Guests.Add(new Person());
                room.Guests.Add(null);
            }
        }

        [Benchmark("collection: adding items with tracking")]
        static void BenchmarkCollectionWithTrackingItemsAdded()
        {
            var room = new Room();

            for (int index = 0; index < Program.SmallNumInstances; index++)
            {
                room.Furniture.Add(new Sofa());
            }
        }

    }

    public static class BenchmarkCollectionItemPropertySetterClass
    {
        static Room room;
        static Chair chair;

        static BenchmarkCollectionItemPropertySetterClass()
        {
            room = new Room();
            chair = new Chair();

            //configure benchmark
            for (int index = 0; index < Program.DefaultNumInstances; index++)
            {
                room.Furniture.Add(new Sofa());
            }
            room.Furniture.Add(chair);
        }

        [Benchmark("collection: tracked item property setter")]
        static void BenchmarkCollectionItemPropertySetter()
        {
            chair.NumSeats = 10;
            chair.NumSeats = 20;
        }
    }
}
