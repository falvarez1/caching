// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
    public class RedisIntegrationTests : ReplicatedCacheIntegrationTestBase<object>
    {
        protected override async Task<(DomainObject[], object)> CreateObjectsAsync()
        {
            var result = new DomainObject[3];

            for (var i = 0; i < result.Length; i++)
            {
                var pubSub = new RedisPubSub(Options.Create(new RedisPubSubOptions
                {
                    Configuration = ConfigurationOptions.Parse("localhost")
                }), A.Fake<ILogger<RedisPubSub>>());

                await pubSub.EnsureConnectedAsync();

                result[i] = DomainObject.Create(pubSub);
            }

            await Task.Yield();

            return (result, null);
        }

        protected override Task WaitForDistributionAsync()
        {
            return Task.Delay(30);
        }

        protected override Task ReleaseAsync(object value)
        {
            return Task.CompletedTask;
        }
    }
}
