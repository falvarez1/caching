// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Orleans.Hosting;
using PubSubTest.Placement;
using Squidex.Caching;
using Squidex.Caching.Orleans;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static void AddOrleansPubSub(this ISiloBuilder siloBuilder)
        {
            siloBuilder.AddPlacementDirector<LocalPlacementStrategy, LocalPlacementDirector>();

            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<OrleansStreamingPubSub>();
                services.AddSingleton<IPubSub>(c => c.GetRequiredService<OrleansStreamingPubSub>());
            });

            siloBuilder.ConfigureApplicationParts(builder =>
            {
                builder.AddApplicationPart(typeof(OrleansStreamingPubSub).Assembly);
            });

            siloBuilder.AddSimpleMessageStreamProvider(Constants.StreamProviderName);
            siloBuilder.AddMemoryGrainStorage("PubSubStore");

            siloBuilder.AddStartupTask<OrleansStreamingPubSub>();
        }
    }
}
