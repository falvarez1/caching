// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Squidex.Caching
{
    public sealed class Subscriptions
    {
        private readonly ConcurrentBag<Action<object?>> subscribers = new ConcurrentBag<Action<object?>>();
        private readonly ILogger logger;

        public bool IsEmpty => subscribers.Count == 0;

        public Subscriptions(ILogger logger)
        {
            this.logger = logger;
        }

        public void Subscribe(Action<object?> subscriber)
        {
            var theSubscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

            subscribers.Add(theSubscriber);
        }

        public void Publish(object? payload)
        {
            foreach (var subscriber in subscribers)
            {
                try
                {
                    subscriber(payload);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Subscriber failed to handle message {payload}.", payload);
                }
            }
        }
    }
}
