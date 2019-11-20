﻿using System;
using System.Threading.Tasks;
using EverStore.Contract;

namespace EverStore
{
    public interface IEventContext
    {
        Task<ResolvedEvent> AppendToStreamAsync(string stream, long expectedStreamVersion, Event @event);
        Task<ReadEvents> ReadStreamEventsForwardAsync(string stream, long start, int count);
        void SubscribeToStreamFrom(string stream, long? lastCheckpoint, Action<CatchUpSubscription, ResolvedEvent> eventAppeared, Action<CatchUpSubscription> liveProcessingStarted = null, Action<CatchUpSubscription, SubscriptionDropReason, Exception> subscriptionDropped = null);
    }
}
