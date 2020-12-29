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
using Xunit;

#pragma warning disable RECS0108 // Warns about static fields in generic types

namespace Squidex.Caching
{
    public abstract class ReplicatedCacheIntegrationTestBase<T>
    {
        public sealed class DomainObject
        {
            private static Guid sharedValue;
            private readonly IReplicatedCache cache;

            public DomainObject(IReplicatedCache cache)
            {
                this.cache = cache;
            }

            public static DomainObject Create(IPubSub pubSub)
            {
                var domainObject = new DomainObject(
                    new ReplicatedCache(
                        new MemoryCache(Options.Create(new MemoryCacheOptions())),
                        pubSub,
                        Options.Create(new ReplicatedCacheOptions { Enable = true })));

                return domainObject;
            }

            public async Task SetValue(Guid value)
            {
                sharedValue = value;

                await cache.AddAsync("KEY", sharedValue, TimeSpan.FromSeconds(10), true);
            }

            public async Task<Guid> GetValueAsync()
            {
                if (cache.TryGetValue("KEY", out var cached) && cached is Guid guid)
                {
                    return guid;
                }

                await Task.Delay(1);

                guid = sharedValue;

                await cache.AddAsync("KEY", guid, TimeSpan.FromSeconds(10));

                return guid;
            }
        }

        protected abstract Task<(DomainObject[], T)> CreateObjectsAsync();

        protected abstract Task ReleaseAsync(T value);

        [Fact]
        public async Task Should_update_values()
        {
            var random = new Random();

            var (objects, context) = await CreateObjectsAsync();

            try
            {
                for (var i = 0; i < 100; i++)
                {
                    var value = Guid.NewGuid();

                    await objects[random.Next(objects.Length)].SetValue(value);

                    await WaitForDistributionAsync();

                    foreach (var obj in objects)
                    {
                        Assert.Equal(value, await obj.GetValueAsync());
                    }
                }
            }
            finally
            {
                await ReleaseAsync(context);
            }
        }

        protected virtual Task WaitForDistributionAsync()
        {
            return Task.CompletedTask;
        }
    }
}
