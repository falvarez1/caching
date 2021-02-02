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

            public string[] Keys { get; set; }
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
            if (message is InvalidateMessage invalidate)
            {
                if (invalidate.Keys != null && invalidate.Source != instanceId)
                {
                    foreach (var key in invalidate.Keys)
                    {
                        if (key != null)
                        {
                            memoryCache.Remove(key);
                        }
                    }
                }
            }
        }

        public Task AddAsync(string key, object? value, TimeSpan expiration)
        {
            if (!options.Enable)
            {
                return Task.CompletedTask;
            }

            memoryCache.Set(key, value, expiration);

            return Task.CompletedTask;
        }

        public async Task RemoveAsync(params string[] keys)
        {
            if (!options.Enable)
            {
                return;
            }

            foreach (var key in keys)
            {
                if (key != null)
                {
                    memoryCache.Remove(key);
                }
            }

            await InvalidateAsync(keys);
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

        private Task InvalidateAsync(params string[] keys)
        {
            if (options.WaitForAcknowledgment)
            {
                return pubSub.PublishAsync(CreateMessage(keys));
            }
            else
            {
                pubSub.PublishAsync(CreateMessage(keys));

                return Task.CompletedTask;
            }
        }

        private InvalidateMessage CreateMessage(string[] keys)
        {
            return new InvalidateMessage { Keys = keys, Source = instanceId };
        }
    }
}
