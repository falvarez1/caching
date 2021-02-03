// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Xunit;

namespace Squidex.Caching
{
    public class ReplicatedCacheTests
    {
        private readonly IPubSub pubSub = A.Fake<SimplePubSub>(options => options.CallsBaseMethods());
        private readonly ReplicatedCacheOptions options = new ReplicatedCacheOptions { Enable = true };
        private readonly ReplicatedCache sut;

        public ReplicatedCacheTests()
        {
            sut = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));
        }

        [Fact]
        public async Task Should_serve_from_cache()
        {
            await sut.AddAsync("Key", 1, TimeSpan.FromMinutes(10));

            AssertCache(sut, "Key", 1, true);

            await sut.RemoveAsync("Key");

            AssertCache(sut, "Key", null, false);
        }

        [Fact]
        public async Task Should_not_serve_from_cache_when_disabled()
        {
            options.Enable = false;

            await sut.AddAsync("Key", 1, TimeSpan.FromMilliseconds(100));

            AssertCache(sut, "Key", null, false);
        }

        [Fact]
        public async Task Should_not_serve_from_cache_when_expired()
        {
            await sut.AddAsync("Key", 1, TimeSpan.FromMilliseconds(1));

            await Task.Delay(100);

            AssertCache(sut, "Key", null, false);
        }

        [Fact]
        public async Task Should_not_invalidate_other_instances_when_added()
        {
            var cache1 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));
            var cache2 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));

            await cache1.AddAsync("Key", 1, TimeSpan.FromMinutes(1));
            await cache2.AddAsync("Key", 2, TimeSpan.FromMinutes(1));

            AssertCache(cache1, "Key", 1, true);
            AssertCache(cache2, "Key", 2, true);
        }

        [Fact]
        public async Task Should_invalidate_other_instances_when_item_removed()
        {
            var cache1 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));
            var cache2 = new ReplicatedCache(CreateMemoryCache(), pubSub, Options.Create(options));

            await cache1.AddAsync("Key", 1, TimeSpan.FromMinutes(1));
            await cache2.RemoveAsync("Key");

            AssertCache(cache1, "Key", null, false);
            AssertCache(cache2, "Key", null, false);
        }

        [Fact]
        public async Task Should_not_send_invalidation_message_when_not_enabled()
        {
            options.Enable = false;

            await sut.RemoveAsync("Key");

            A.CallTo(() => pubSub.PublishAsync(A<object>._))
                .MustNotHaveHappened();
        }

        private static void AssertCache(IReplicatedCache cache, string key, object expectedValue, bool expectedFound)
        {
            var found = cache.TryGetValue(key, out var value);

            Assert.Equal(expectedFound, found);
            Assert.Equal(expectedValue, value);
        }

        private static MemoryCache CreateMemoryCache()
        {
            return new MemoryCache(Options.Create(new MemoryCacheOptions()));
        }
    }
}
