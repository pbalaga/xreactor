#region License

// Copyright (c) Pawel Balaga https://xreactor.codeplex.com/
// Licensed under MS-PL, See License file or http://opensource.org/licenses/MS-PL

#endregion
using System;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace xReactor.Tests
{
    [TestClass]
    public class CachingTests : ReactiveTestBase
    {
        [TestMethod]
        public void WhenCreatingTwoInstancesOfReactiveClassOneIsCached()
        {
//            var room = CreateRoom();
//            Reactor reactor = new Reactor(room, null,null);
////            Reactor reactor = ((Reactor)(((dynamic)room).reactor));

//            //reactor.Create(()=>room.Name,
//            string targetPropertyName = ExpressionHelper.GetNameFromExpression(() => room.Name);
//            ExpressionProvider<string> provider = Reactor.ExpressionCache.
//                GetProvider(room.GetType(), targetPropertyName);
            
        }
    }
}
