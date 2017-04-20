//----------------------------------------------------------------------- 
// <copyright file="PlayerTestsWithView.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using Dizzle.Cqrs.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using TestDomain.Cqrs.Commands;
using TestDomain.Cqrs.Events;
using TestDomain.Cqrs.Model;
using TestDomain.Cqrs.Views;

namespace TestDomain.Tests
{
    [TestFixture]
    public class PlayerTestsWithView : BDDTestWithView<Player>
    {
        private PlayerId testId;
        [SetUp]
        public void Setup()
        {
            testId = new PlayerId(Guid.NewGuid());
        }

        [Test]
        public void PlayerTestsWithView_Can_Create_New_Player()
        {
            Test<PlayerId, PlayerView>(
                Given(new List<IEvent>()),
                When(new CreatePlayer(testId, "FirstName", "LastName", null, "123 4th Street")),
                Then(new PlayerCreated(testId, "FirstName", "LastName", null, "123 4th Street")),
                testId,
                new PlayerView { Id = testId, FirstName = "FirstName", LastName = "LastName", Street = "123 4th Street" }
                );
        }

        [Test]
        public void PlayerTestsWithView_Can_Update_New_Player()
        {
            Test<PlayerId, PlayerView>(
                Given(new List<IEvent>() { new PlayerCreated(testId, "FirstName", "LastName", null, "123 4th Street") }),
                When(new UpdatePlayer(testId, "FirstNameUpdated", "LastName", null, "123 4th Street")),
                Then(new PlayerUpdated(testId, "FirstNameUpdated", "LastName", null, "123 4th Street")),
                testId,
                new PlayerView { Id = testId, FirstName = "FirstNameUpdated", LastName = "LastName", Street = "123 4th Street" }
                );
        }
    }
}
