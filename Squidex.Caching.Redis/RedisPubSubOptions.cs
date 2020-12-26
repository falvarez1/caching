// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Squidex.Caching.Redis
{
    public sealed class RedisPubSubOptions
    {
        public Func<TextWriter, Task<IConnectionMultiplexer>>? ConnectionFactory { get; set; }

        public ConfigurationOptions? Configuration { get; set; }

        internal async Task<IConnectionMultiplexer> ConnectAsync(TextWriter log)
        {
            var factory = ConnectionFactory;
            if (factory == null)
            {
                return await ConnectionMultiplexer.ConnectAsync(Configuration, log);
            }

            return await factory(log);
        }
    }
}
