//----------------------------------------------------------------------- 
// <copyright file="PlayerViewProjection.cs" company="4Deep Technologies LLC"> 
// Copyright (c) 4Deep Technologies LLC. All rights reserved. 
// <author>Darren Ford</author> 
// <date>Thursday, April 30, 2015 3:00:44 PM</date> 
// </copyright> 
//-----------------------------------------------------------------------


using Dizzle.Cqrs.Core;
using Dizzle.Cqrs.Core.Storage;
using TestDomain.Cqrs.Model;
using TestDomain.Cqrs.Events;
using TestDomain.Cqrs.Views;

namespace TestDomain.Projections
{
    public class PlayerViewProjection : AbstractBaseProjection,
        IApplyEvent<PlayerCreated>,
        IApplyEvent<PlayerUpdated>,
        ISubscribeTo<PlayerCreated>,
        ISubscribeTo<PlayerUpdated>
    {
        protected IDocumentWriter<PlayerId, PlayerView> _writer;

        public PlayerViewProjection()
        {
        }

        public PlayerViewProjection(IDocumentWriter<PlayerId,PlayerView> writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// Used only for unit tests
        /// </summary>
        /// <param name="writer"></param>
        public void SetWriter(IDocumentWriter<PlayerId, PlayerView> writer)
        {
            _writer = writer;
        }

        public void Apply(PlayerCreated e)
        {
            _writer.Add(e.Id, new PlayerView
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Street = e.Street
            });
        }

        public void Apply(PlayerUpdated e)
        {
            _writer.UpdateOrThrow(e.Id, pv =>
            {
                pv.Id = e.Id;
                pv.FirstName = e.FirstName;
                pv.LastName = e.LastName;
            });
        }

        public void Handle(PlayerCreated e)
        {
            Apply(e);
        }

        public void Handle(PlayerUpdated e)
        {
            Apply(e);
        }
    }
}
