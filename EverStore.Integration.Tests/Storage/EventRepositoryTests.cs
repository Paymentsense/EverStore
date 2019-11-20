﻿using System;
using System.Linq;
using EverStore.Domain;
using EverStore.Storage;
using MongoDB.Driver;
using Xunit;

namespace EverStore.Integration.Tests.Storage
{
    [Collection("MongoCollection")]

    public class EventRepositoryTests
    {
        private readonly MongoFixture _mongoFixture;

        public EventRepositoryTests(MongoFixture mongoFixture)
        {
            _mongoFixture = mongoFixture;
            _mongoFixture.SetupFixture();
        }

        [Fact]
        public void AppendEvent_InOrder_Saves()
        {
            var repo = new EventRepository(_mongoFixture.MongoContext);

            var firstEvent = new PersistedEvent
            {
                GlobalVersion = 1,
                Stream = "contact_1234",
                StreamVersion = 1,
            };
            repo.AppendEvent(firstEvent);

            var secondEvent = new PersistedEvent
            {
                GlobalVersion = 2,
                Stream = "contact_1234",
                StreamVersion = 2,
            };
            repo.AppendEvent(secondEvent);
            
            var filter = Builders<PersistedEvent>.Filter.Eq(e => e.Stream, "contact_1234");
            var results = _mongoFixture.MongoClient.GetDatabase(_mongoFixture.DatabaseName).GetCollection<PersistedEvent>(typeof(PersistedEvent).Name).FindSync(filter, new FindOptions<PersistedEvent>(){BatchSize = 100});
            results.MoveNext();
            Assert.NotNull(results.Current.SingleOrDefault(e => e.GlobalVersion == 1));
            Assert.NotNull(results.Current.SingleOrDefault(e => e.GlobalVersion == 2));
        }
        
        [Fact]
        public void AppendEvent_ConflictingStreamVersions_Fails()
        {
            var repo = new EventRepository(_mongoFixture.MongoContext);

            var firstEvent = new PersistedEvent
            {
                GlobalVersion = 1,
                Stream = "contact_1234",
                StreamVersion = 1,
                CreatedAt = DateTimeOffset.Now
            };
            repo.AppendEvent(firstEvent);

            var secondEvent = new PersistedEvent
            {
                GlobalVersion = 2,
                Stream = "contact_1234",
                StreamVersion = 1,
                CreatedAt = DateTimeOffset.Now
            };
            Assert.Throws<MongoWriteException>(() => repo.AppendEvent(secondEvent));
        }
    }
}