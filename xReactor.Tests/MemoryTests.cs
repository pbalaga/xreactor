#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xReactor.Common;
using FluentAssertions;

namespace xReactor.Tests
{
    [TestClass]
    public class MemoryTests : ReactiveTestBase
    {
        [TestMethod]
        public void UnreferencedReactiveObjectGetsGarbageCollected()
        {
            Room room = CreateRoom();
            WeakReference<Room> weakReference = new WeakReference<Room>(room);

            room = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            bool isAlive = weakReference.TryGetTarget(out room);
            isAlive.Should().BeFalse("because it should've been garbage collected");
        }
    }
}
