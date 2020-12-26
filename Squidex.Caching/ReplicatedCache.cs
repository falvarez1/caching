// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace Squidex.Caching
{
    public sealed class ReplicatedCache : IReplicatedCache
    {
        private readonly Guid instanceId = Guid.NewGuid();
        private readonly IMemoryCache memoryCache;
        private readonly IPubSub pubSub;
        private readonly ReplicatedCacheOptions options;

        public class InvalidateMessage
        {
            public Guid Source { get; set; }

            public string Key { get; set; }
        }

        public ReplicatedCache(IMemoryCache memoryCache, IPubSub pubSub, IOptions<ReplicatedCacheOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            this.pubSub = pubSub ?? throw new ArgumentNullException(nameof(pubSub));

            if (options.Value.Enable)
            {
                this.pubSub.SubscribeAsync(OnMessage).Forget();
            }

            this.options = options.Value;
        }

        private void OnMessage(object? message)
        {
            if (message is InvalidateMessage invalidate && invalidate.Source != instanceId)
            {
                memoryCache.Remove(invalidate.Key);
            }
        }

        public async Task AddAsync(string key, object? value, TimeSpan expiration, bool invalidate)
        {
            if (!options.Enable)
            {
                return;
            }

            memoryCache.Set(key, value, expiration);

            if (invalidate)
            {
                await InvalidateAsync(key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (!options.Enable)
            {
                return;
            }

            memoryCache.Remove(key);

            await InvalidateAsync(key);
        }

        public bool TryGetValue(string key, out object? value)
        {
            if (!options.Enable)
            {
                value = null;

                return false;
            }

            return memoryCache.TryGetValue(key, out value);
        }

        private Task InvalidateAsync(string key)
        {
            if (options.WaitForAcknowledgment)
            {
                return pubSub.PublishAsync(new InvalidateMessage { Key = key, Source = instanceId });
            }
            else
            {
                pubSub.PublishAsync(new InvalidateMessage { Key = key, Source = instanceId });

                return Task.CompletedTask;
            }
        }
    }
}
