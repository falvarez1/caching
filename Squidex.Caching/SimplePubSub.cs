// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Squidex.Caching
{
    public class SimplePubSub : IPubSub
    {
        private readonly Subscriptions subscriptions;

        public SimplePubSub(ILogger<SimplePubSub> logger)
        {
            subscriptions = new Subscriptions(logger);
        }

        public virtual Task PublishAsync(object? payload)
        {
            subscriptions.Publish(payload);

            return Task.CompletedTask;
        }

        public virtual Task SubscribeAsync(Action<object?> subscriber)
        {
            subscriptions.Subscribe(subscriber);

            return Task.CompletedTask;
        }
    }
}
