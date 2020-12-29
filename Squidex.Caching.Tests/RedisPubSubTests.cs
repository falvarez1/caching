// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Caching.Redis;
using StackExchange.Redis;
using Xunit;

namespace Squidex.Caching
{
    [Trait("Category", "Dependencies")]
    public class RedisPubSubTests
    {
        [Fact]
        public async Task Should_receive_simple_pubsub_messages()
        {
            var pubSubs = await CreatePubSubsAsync();

            await PubSubTestHelper.PublishSimpleValuesAsync(pubSubs, pubSubs.Count, 2000);
        }

        [Fact]
        public async Task Should_receive_complex_pubsub_messages()
        {
            var pubSubs = await CreatePubSubsAsync();

            await PubSubTestHelper.PublishComplexValuesAsync(pubSubs, pubSubs.Count, 2000);
        }

        private static async Task<IList<IPubSub>> CreatePubSubsAsync()
        {
            var pubsub1 = new RedisPubSub(Options.Create(new RedisPubSubOptions
            {
                Configuration = ConfigurationOptions.Parse("localhost")
            }), A.Fake<ILogger<RedisPubSub>>());

            var pubsub2 = new RedisPubSub(Options.Create(new RedisPubSubOptions
            {
                Configuration = ConfigurationOptions.Parse("localhost")
            }), A.Fake<ILogger<RedisPubSub>>());

            await pubsub1.EnsureConnectedAsync();
            await pubsub2.EnsureConnectedAsync();

            return new List<IPubSub> { pubsub1, pubsub2 };
        }
    }
}
