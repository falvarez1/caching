// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Squidex.Caching
{
    public class SimplePubSub : IPubSub
    {
        private readonly List<Action<object>> subscribers = new List<Action<object>>();
        private readonly ILogger<SimplePubSub> logger;

        public SimplePubSub(ILogger<SimplePubSub> logger)
        {
            this.logger = logger;
        }

        public virtual Task PublishAsync(object payload)
        {
            foreach (var subscriber in subscribers)
            {
                try
                {
                    subscriber(payload);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Subscriber failed to handle message {payload}", payload);
                }
            }

            return Task.CompletedTask;
        }

        public virtual void Subscribe(Action<object> subscriber)
        {
            var theSubscriber = subscriber ?? throw new ArgumentNullException(nameof(subscriber));

            subscribers.Add(theSubscriber);
        }
    }
}
