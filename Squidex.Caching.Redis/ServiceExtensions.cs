// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Caching;
using Squidex.Caching.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static void AddRedisPubSub(this IServiceCollection services, Action<RedisPubSubOptions> configure)
        {
            services.Configure(configure);

            services.AddSingleton<IPubSub, RedisPubSub>();
        }
    }
}
