// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace Squidex.Caching.Orleans
{
    public sealed class OrleansStreamingPubSub : IPubSub, IStartupTask
    {
        private readonly IStreamingPubSubHostGrain hostGrain;
        private readonly ConcurrentBag<Action<object>> subscribers = new ConcurrentBag<Action<object>>();
        private readonly ILogger<OrleansStreamingPubSub> logger;

        public OrleansStreamingPubSub(IGrainFactory grainFactory, ILocalSiloDetails silo, ILogger<OrleansStreamingPubSub> logger)
        {
            hostGrain =
                grainFactory.GetGrain<IStreamingPubSubHostGrain>(
                    silo.SiloAddress.ToParsableString());

            this.logger = logger;
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            return hostGrain.ActivateAsync();
        }

        public Task PublishAsync(object payload)
        {
            return hostGrain.SendAsync(payload);
        }

        public Task<int> GetClusterSizeAsync()
        {
            return Task.FromResult(-1);
        }

        public void Publish(object payload)
        {
            foreach (var subscriber in subscribers)
            {
                try
                {
                    subscriber(payload);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Subscriber failed to handle message {payload}", payload);
                }
            }
        }

        public void Subscribe(Action<object> subscriber)
        {
            var theSubscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

            subscribers.Add(theSubscriber);
        }
    }
}
