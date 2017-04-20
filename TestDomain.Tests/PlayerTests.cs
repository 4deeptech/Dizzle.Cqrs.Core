//----------------------------------------------------------------------- 
// <copyright file="PlayerTests.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using Dizzle.Cqrs.Core;
using Dizzle.Cqrs.Core.TestSupport;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestDomain.Cqrs.Commands;
using TestDomain.Cqrs.Events;
using TestDomain.Cqrs.Model;

namespace TestDomain.Tests
{
    [TestFixture]
    public class PlayerTests : BDDTest<Player>
    {
        private PlayerId testId;
        [SetUp]
        public void Setup()
        {
            testId = new PlayerId(Guid.NewGuid());
        }

        [Test]
        public void PlayerTests_Can_Create_New_Player()
        {
            Test(
                Given(new List<IEvent>()),
                When(new CreatePlayer(testId,"FirstName","LastName",null,"test")),
                Then(new PlayerCreated(testId, "FirstName", "LastName", null, "test"))
                );
        }

        [Test]
        public void PlayerTests_Can_Update_New_Player()
        {
            Test(
                Given(new List<IEvent> { 

                    new PlayerCreated(testId, "FirstName", "LastName", null,"test"),
                    new PlayerUpdated(testId, "FirstName2", "LastName2", null,"test"), 
                    new PlayerUpdated(testId, "FirstName3", "LastName3", null,"test") }),

                When(new UpdatePlayer(testId, "FirstName4", "LastName4", null, "test")),

                Then(new PlayerUpdated(testId, "FirstName4", "LastName4", null, "test"))

                );
        }
    }
}
