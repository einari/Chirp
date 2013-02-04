﻿using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web.Routing;
using Bifrost.Configuration;
using Bifrost.Entities;
using Bifrost.Execution;
using Bifrost.Ninject;
using Bifrost.RavenDB;
using Bifrost.Services.Execution;
using Bifrost.Web;
using Chirp.Application.Modules;
using Chirp.Concepts;
using Chirp.Read.Domain.Chirping;
using Chirp.Read.Domain.Follow;
using Chirp.Read.Streams;
using Chirp.Web.Services;
using Ninject;
using System.Net;
using Chirp.Application.Security;
using ChirperId = Chirp.Concepts.ChirperId;

namespace Chirp.Web
{
    public class Global : BifrostHttpApplication
    {
        protected override IContainer CreateContainer()
        {
            var kernel = new StandardKernel(new UsersModule(), new ChirpingModule());
            var container = new Container(kernel);
            return container;
        }

        public override void OnStarted()
        {
            RouteTable.Routes.AddService<StreamerService>();
            RouteTable.Routes.AddService<UserService>();
            RouteTable.Routes.AddService<DebugInfoService>();

            base.OnStarted();
            EnsureScottAndHannahAreSetup(base.Container);
        }

        public override void OnConfigure(IConfigure configure)
        {
            var connectionString = ConfigurationManager.AppSettings["Database"];
            var userName = ConfigurationManager.AppSettings["RavenUsername"];
            var password = ConfigurationManager.AppSettings["RavenPassword"];

            var entityIds = new EntityIdPropertyRegister();
            entityIds.RegisterIdProperty<ReadingStream,ReaderId>(rs => rs.Reader);
            entityIds.RegisterIdProperty<MyChirps,ChirperId>(c => c.Chirper);
            entityIds.RegisterIdProperty<Read.Streams.Chirp, ChirpId>(c => c.Id);
            entityIds.RegisterIdProperty<Chirper, ChirperId>(c => c.ChirperId);
            entityIds.RegisterIdProperty<Read.Domain.Chirping.ChirperId,ChirperId>(c => c.Id);
            entityIds.RegisterIdProperty<MyFollowers,ChirperId>(c => c.Chirper);

            configure
                .Events.Asynchronous()
                .Events.UsingRavenDB(c =>
                {
                    c.Url = "http://localhost:8080";
                    c.DefaultDatabase = "Chirp";
                    if (!string.IsNullOrEmpty(userName))
                        c.Credentials = new NetworkCredential(userName, password);
                })
                .Serialization.UsingJson()
                .Sagas.WithoutLibrarian()
                .DefaultStorage.UsingRavenDB(connectionString, c =>
                {
                    c.DefaultDatabase = "Chirp";
                    if (!string.IsNullOrEmpty(userName))
                        c.Credentials = new NetworkCredential(userName, password);
                    c.IdPropertyRegister = entityIds;

                })
                .AsSinglePageApplication();
                //.WithMimir();

            base.OnConfigure(configure);

        }

        #region Just For Pav
        static Guid hannah = Guid.Parse("{03F1D667-063B-4D15-B892-06F89818E9A8}");
        static Guid scott = Guid.Parse("{A345577C-28AD-4371-87FE-8A57E84BDE2E}");

       void EnsureScottAndHannahAreSetup(IContainer container)
       {
           EnsureArtifactsArePresentFor(container,hannah, "@hannah", scott);
           EnsureArtifactsArePresentFor(container,scott, "@scott", hannah);
       }

        void EnsureArtifactsArePresentFor(IContainer container, Guid user, string name, Guid follower)
        {
            var chirpersEntityContext = container.Get<IEntityContext<Chirper>>();
            var chirperIdsEntityContext = container.Get<IEntityContext<Read.Domain.Chirping.ChirperId>>();
            var myChirpsEntityContext = container.Get<IEntityContext<MyChirps>>();
            var myReadingStreamEntityContext = container.Get<IEntityContext<ReadingStream>>();
            var myFollowersEntityContext = container.Get<IEntityContext<MyFollowers>>();

            var chirperByGetById = chirpersEntityContext.GetById<ChirperId>(user);
            var chirperByEntities = chirpersEntityContext.Entities.FirstOrDefault(c => c.ChirperId == user);
            if(chirperByGetById == null && chirperByEntities == null)
            {
                chirpersEntityContext.Insert(new Chirper() { ChirperId = user, DisplayName = name });
                chirpersEntityContext.Commit();
            }

            var chirperIdsByGetById = chirperIdsEntityContext.GetById<ChirperId>(user);
            var chirperIdsByEntities = chirperIdsEntityContext.Entities.FirstOrDefault(c => c.Id == user);
            if(chirperIdsByGetById == null &&  chirperIdsByEntities == null )
            {
                chirperIdsEntityContext.Insert(new Read.Domain.Chirping.ChirperId() { Id = user});
                chirperIdsEntityContext.Commit();
            }

            var myChirpsByGetById = myChirpsEntityContext.GetById<ChirperId>(user);
            var myChirpsByEntities = myChirpsEntityContext.Entities.FirstOrDefault(c => c.Chirper == user);
            if(myChirpsByGetById == null && myChirpsByEntities == null)
            {
                myChirpsEntityContext.Insert(new MyChirps(user));
                myChirpsEntityContext.Commit();
            }

            var myReadingStreamByGetById = myReadingStreamEntityContext.GetById<ReaderId>(user);
            var myReadingStreamByEntities = myReadingStreamEntityContext.Entities.FirstOrDefault(c => c.Reader == user);
            if(myReadingStreamByGetById == null && myReadingStreamByEntities == null)
            {
                var readingStream = new ReadingStream(user);
                myReadingStreamEntityContext.Insert(readingStream);
                myReadingStreamEntityContext.Commit();
            }

            var myFollowersByGetById = myFollowersEntityContext.GetById<ChirperId>(user);
            var myFollowersByEntities = myFollowersEntityContext.Entities.FirstOrDefault(c => c.Chirper == user);
            if(myFollowersByGetById == null && myFollowersByEntities == null)
            {
                var followers = new MyFollowers(user);
                followers.AddFollower(follower);
                myFollowersEntityContext.Insert(followers);
                myFollowersEntityContext.Commit();
            }
        }
        #endregion
    }
} 
