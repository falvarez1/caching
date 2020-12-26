// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;
using Orleans.Streams;
using PubSubTest.Placement;

namespace Squidex.Caching.Orleans
{
    [LocalPlacement]
    public sealed class StreamingPubSubHostGrain : Grain, IStreamingPubSubHostGrain
    {
        private static readonly TimeSpan DelayedDeactivation = TimeSpan.FromDays(10000);
        private readonly OrleansStreamingPubSub pubSub;
        private readonly ILocalSiloDetails silo;
        private readonly ILogger<OrleansStreamingPubSub> logger;
        private StreamSubscriptionHandle<object?>? subscription;
        private IAsyncStream<object?>? stream;

        public StreamingPubSubHostGrain(OrleansStreamingPubSub pubSub, ILocalSiloDetails silo, ILogger<OrleansStreamingPubSub> logger)
        {
            this.pubSub = pubSub;
            this.silo = silo;
            this.logger = logger;
        }

        public override async Task OnActivateAsync()
        {
            var id = this.GetPrimaryKeyString();

            if (!string.Equals(id, silo.SiloAddress.ToParsableString()))
            {
                DeactivateOnIdle();
            }
            else
            {
                DelayDeactivation(DelayedDeactivation);

                var streamProvider = GetStreamProvider(Constants.StreamProviderName);

                stream = streamProvider.GetStream<object?>(Constants.StreamId, Constants.StreamProviderName);

                subscription = await stream.SubscribeAsync((data, token) =>
                {
                    pubSub.Publish(data);

                    return Task.CompletedTask;
                });
            }
        }

        public override async Task OnDeactivateAsync()
        {
            if (subscription != null)
            {
                await subscription.UnsubscribeAsync();
            }
        }

        public Task ActivateAsync()
        {
            return Task.CompletedTask;
        }

        public async Task SendAsync(object? payload)
        {
            if (stream == null)
            {
                return;
            }

            try
            {
                var timeout = Task.Delay(Constants.SendTimeout);

                var completed = await Task.WhenAny(timeout, stream.OnNextAsync(payload));

                if (completed == timeout)
                {
                    logger.LogWarning("Failed to send message within {time}", Constants.SendTimeout);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish message");
            }
        }
    }
}
