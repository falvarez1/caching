// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
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
        private readonly Subscriptions subscriptions;

        public OrleansStreamingPubSub(IGrainFactory grainFactory, ILocalSiloDetails silo, ILogger<OrleansStreamingPubSub> logger)
        {
            hostGrain =
                grainFactory.GetGrain<IStreamingPubSubHostGrain>(
                    silo.SiloAddress.ToParsableString());

            subscriptions = new Subscriptions(logger);
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            return hostGrain.ActivateAsync();
        }

        public Task PublishAsync(object? payload)
        {
            return hostGrain.SendAsync(payload);
        }

        public Task<int> GetClusterSizeAsync()
        {
            return Task.FromResult(-1);
        }

        public void Publish(object? payload)
        {
            subscriptions.Publish(payload);
        }

        public Task SubscribeAsync(Action<object?> subscriber)
        {
            subscriptions.Subscribe(subscriber);

            return Task.CompletedTask;
        }
    }
}
