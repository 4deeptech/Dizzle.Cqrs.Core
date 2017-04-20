 

#region (c) 2015 4Deep Technologies LLC

// Copyright (c) 4Deep Technologies LLC 2015 (http://www.4deeptech.com)
// Created by Darren Ford

//Generated using PDizzle DSL Model and Code Generator
//see http://www.4deeptech.com/blog/post/2012/02/16/Creating-a-DSL-for-DDD-and-Event-Sourcing-Code-Generation.aspx
#endregion
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using Dizzle.Cqrs.Core;
using TestDomain.Cqrs.Events;
using TestDomain.Cqrs.Commands;
using TestDomain.Cqrs.Model; 

namespace TestDomain.Cqrs.Model
{
	/// <summary>
	/// A player
	/// </summary> 
	#pragma warning disable 0108
	public partial class Player : Aggregate,
		IHandleCommand<CreatePlayer>
,
		IHandleCommand<UpdatePlayer>
,
		IApplyEvent<PlayerCreated>
,
		IApplyEvent<PlayerUpdated>

	{
		/// <summary>
		/// The id for the player
		/// </summary>
		[DataMember(Order = 1)]
        public PlayerId Id { get; internal set; }

		/// <summary>
		/// The first name
		/// </summary>
		[DataMember(Order = 2)]
        public String FirstName { get; internal set; }

		/// <summary>
		/// The last name
		/// </summary>
		[DataMember(Order = 3)]
        public String LastName { get; internal set; }

		/// <summary>
		/// Player's birth date
		/// </summary>
		[DataMember(Order = 4)]
        public DateTime? BirthDate { get; internal set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember(Order = 5)]
        public String Street { get; internal set; }


		public Player()
        {

        }

	}//end aggregateroot

	#region Aggregate Id

	[DataContract(Namespace = "TestDomain.Cqrs")]
    public sealed class PlayerId : AbstractIdentity<Guid>
    {
        public const string TagValue = "player";

        public PlayerId()
        {
        }

        public PlayerId(Guid id)
        {
            Id = id;
        }

        public override string GetTag()
        {
            return TagValue;
        }


        [DataMember(Order = 1)]
        public override Guid Id { get; protected set; }


    }

	#endregion

	#region entities
	#endregion
}	

namespace TestDomain.Cqrs.Commands 
{
	/// <summary>
	/// Create a new player
	/// </summary>
	[DataContract(Namespace = "TestDomain.Cqrs")]
    public partial class CreatePlayer
    {

		/// <summary>
		/// The id for the player
		/// </summary>
		[DataMember(Order = 1)] 
        public PlayerId Id { get; private set; }

		/// <summary>
		/// The first name
		/// </summary>
		[DataMember(Order = 2)] 
        public String FirstName { get; private set; }

		/// <summary>
		/// The last name
		/// </summary>
		[DataMember(Order = 3)] 
        public String LastName { get; private set; }

		/// <summary>
		/// Player's birth date
		/// </summary>
		[DataMember(Order = 4)] 
        public DateTime? BirthDate { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember(Order = 5)] 
        public String Street { get; private set; }

		public CreatePlayer() 
        {
            
        }

        public CreatePlayer(PlayerId id, String firstName, String lastName, DateTime? birthDate, String street)
        {
            Id = id;
			FirstName = firstName;
			LastName = lastName;
			BirthDate = birthDate;
			Street = street;
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Id {1}, FirstName {2}, LastName {3}, BirthDate {4}, Street {5}",GetType().Name, Id, FirstName, LastName, BirthDate, Street);
        }
    }//end CreatePlayer class


	/// <summary>
	/// Update existing player
	/// </summary>
	[DataContract(Namespace = "TestDomain.Cqrs")]
    public partial class UpdatePlayer
    {

		/// <summary>
		/// The id for the player
		/// </summary>
		[DataMember(Order = 1)] 
        public PlayerId Id { get; private set; }

		/// <summary>
		/// The first name
		/// </summary>
		[DataMember(Order = 2)] 
        public String FirstName { get; private set; }

		/// <summary>
		/// The last name
		/// </summary>
		[DataMember(Order = 3)] 
        public String LastName { get; private set; }

		/// <summary>
		/// Player's birth date
		/// </summary>
		[DataMember(Order = 4)] 
        public DateTime? BirthDate { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember(Order = 5)] 
        public String Street { get; private set; }

		public UpdatePlayer() 
        {
            
        }

        public UpdatePlayer(PlayerId id, String firstName, String lastName, DateTime? birthDate, String street)
        {
            Id = id;
			FirstName = firstName;
			LastName = lastName;
			BirthDate = birthDate;
			Street = street;
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Id {1}, FirstName {2}, LastName {3}, BirthDate {4}, Street {5}",GetType().Name, Id, FirstName, LastName, BirthDate, Street);
        }
    }//end UpdatePlayer class


}//end namespace

namespace TestDomain.Cqrs.Events
{
	/// <summary>
	/// A new player was created
	/// </summary>
	[DataContract(Namespace = "TestDomain.Cqrs")]
    public partial class PlayerCreated : IEvent
    {

		/// <summary>
		/// The id for the player
		/// </summary>
		[DataMember(Order = 1)] 
        public PlayerId Id { get; private set; }

		/// <summary>
		/// The first name
		/// </summary>
		[DataMember(Order = 2)] 
        public String FirstName { get; private set; }

		/// <summary>
		/// The last name
		/// </summary>
		[DataMember(Order = 3)] 
        public String LastName { get; private set; }

		/// <summary>
		/// Player's birth date
		/// </summary>
		[DataMember(Order = 4)] 
        public DateTime? BirthDate { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember(Order = 5)] 
        public String Street { get; private set; }

		public PlayerCreated() 
        {
            
        }

        public PlayerCreated(PlayerId id, String firstName, String lastName, DateTime? birthDate, String street)
        {
            Id = id;
			FirstName = firstName;
			LastName = lastName;
			BirthDate = birthDate;
			Street = street;
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Id {1}, FirstName {2}, LastName {3}, BirthDate {4}, Street {5}",GetType().Name, Id, FirstName, LastName, BirthDate, Street);
        }
    }//end PlayerCreated class


	/// <summary>
	/// An existing player was updated
	/// </summary>
	[DataContract(Namespace = "TestDomain.Cqrs")]
    public partial class PlayerUpdated : IEvent
    {

		/// <summary>
		/// The id for the player
		/// </summary>
		[DataMember(Order = 1)] 
        public PlayerId Id { get; private set; }

		/// <summary>
		/// The first name
		/// </summary>
		[DataMember(Order = 2)] 
        public String FirstName { get; private set; }

		/// <summary>
		/// The last name
		/// </summary>
		[DataMember(Order = 3)] 
        public String LastName { get; private set; }

		/// <summary>
		/// Player's birth date
		/// </summary>
		[DataMember(Order = 4)] 
        public DateTime? BirthDate { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember(Order = 5)] 
        public String Street { get; private set; }

		public PlayerUpdated() 
        {
            
        }

        public PlayerUpdated(PlayerId id, String firstName, String lastName, DateTime? birthDate, String street)
        {
            Id = id;
			FirstName = firstName;
			LastName = lastName;
			BirthDate = birthDate;
			Street = street;
        }

        public override string ToString()
        {
            return string.Format("Type: {0}, Id {1}, FirstName {2}, LastName {3}, BirthDate {4}, Street {5}",GetType().Name, Id, FirstName, LastName, BirthDate, Street);
        }
    }//end PlayerUpdated class


}//end namespace

namespace TestDomain.Cqrs.Views 
{
	/// <summary>
	/// Basic player view
	/// </summary>
    public partial class PlayerView
    {

		/// <summary>
		/// The id for the player
		/// </summary>
		[DataMember(Order = 1)] 
        public PlayerId Id { get; set; }

		/// <summary>
		/// The first name
		/// </summary>
		[DataMember(Order = 2)] 
        public String FirstName { get; set; }

		/// <summary>
		/// The last name
		/// </summary>
		[DataMember(Order = 3)] 
        public String LastName { get; set; }

		/// <summary>
		/// Player's birth date
		/// </summary>
		[DataMember(Order = 4)] 
        public DateTime? BirthDate { get; set; }

		/// <summary>
		/// 
		/// </summary>
		[DataMember(Order = 5)] 
        public String Street { get; set; }

		public PlayerView() 
        {
            
        }

        public PlayerView(PlayerId id, String firstName, String lastName, DateTime? birthDate, String street)
        {
            Id = id;
			FirstName = firstName;
			LastName = lastName;
			BirthDate = birthDate;
			Street = street;
        }
        
    }//end PlayerView class

}//end namespace
