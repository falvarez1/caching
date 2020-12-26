// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Caching.Redis;
using StackExchange.Redis;
using Xunit;

namespace Squidex.Caching.Tests
{
    [Trait("Category", "Dependencies")]
    public class RedisPubSubTests
    {
        [Fact]
        public async Task Should_receive_pubsub_message()
        {
            var pubsub1 = new RedisPubSub(Options.Create(new RedisPubSubOptions
            {
                Configuration = ConfigurationOptions.Parse("localhost")
            }), A.Fake<ILogger<RedisPubSub>>());

            var pubsub2 = new RedisPubSub(Options.Create(new RedisPubSubOptions
            {
                Configuration = ConfigurationOptions.Parse("localhost")
            }), A.Fake<ILogger<RedisPubSub>>());

            var received1 = new HashSet<Guid>();
            var received2 = new HashSet<Guid>();

            var sent = new HashSet<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            await pubsub1.SubscribeAsync(v =>
            {
                if (v is Guid guid)
                {
                    received1.Add(guid);
                }
            });

            await pubsub2.SubscribeAsync(v =>
            {
                if (v is Guid guid)
                {
                    received2.Add(guid);
                }
            });

            foreach (var message in sent)
            {
                await pubsub1.PublishAsync(message);
            }

            await Task.Delay(2000);

            Assert.Equal(sent, received1);
            Assert.Equal(sent, received2);
        }
    }
}
