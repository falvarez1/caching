// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Squidex.Caching;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static void AddSimplePubSub(this IServiceCollection services)
        {
            services.TryAddSingleton<IPubSub, SimplePubSub>();
        }

        public static void AddReplicatedCache(this IServiceCollection services)
        {
            AddReplicatedCache(services, null);
        }

        public static void AddReplicatedCache(this IServiceCollection services, Action<ReplicatedCacheOptions>? configureOptions = null)
        {
            services.Configure(configureOptions);
            services.TryAddSingleton<IReplicatedCache, ReplicatedCache>();
        }
    }
}
