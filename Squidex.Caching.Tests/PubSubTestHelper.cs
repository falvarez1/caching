// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Caching
{
    public static class PubSubTestHelper
    {
        public record TestValue(Guid Id);

        public static async Task PublishSimpleValuesAsync(IList<IPubSub> pubSubs, int expectedCount, int wait = 0)
        {
            var values = new HashSet<object>
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            await PublishValuesAsync(pubSubs, expectedCount, values, wait);
        }

        public static async Task PublishComplexValuesAsync(IList<IPubSub> pubSubs, int expectedCount, int wait = 0)
        {
            var values = new HashSet<object>
            {
                new TestValue(Guid.NewGuid()),
                new TestValue(Guid.NewGuid()),
                new TestValue(Guid.NewGuid()),
                new TestValue(Guid.NewGuid())
            };

            await PublishValuesAsync(pubSubs, expectedCount, values, wait);
        }

        public static async Task PublishValuesAsync(IList<IPubSub> pubSubs, int expectedCount, HashSet<object> sending, int wait)
        {
            var received = new HashSet<object>[pubSubs.Count];

            for (var i = 0; i < pubSubs.Count; i++)
            {
                var receivedValues = new HashSet<object>();

                await pubSubs[i].SubscribeAsync(v =>
                {
                    receivedValues.Add(v);
                });

                received[i] = receivedValues;
            }

            foreach (var message in sending)
            {
                await pubSubs[0].PublishAsync(message);

                await Task.Delay(wait);

                Assert.Equal(expectedCount, received.Count(x => x.Contains(message)));
            }
        }
    }
}
