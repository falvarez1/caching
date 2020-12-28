// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Squidex.Caching.Redis
{
    public sealed class RedisPubSub : IPubSub
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1);
        private readonly Subscriptions subscriptions;
        private readonly RedisPubSubOptions options;
        private readonly RedisChannel redisChannel = new RedisChannel("CachingPubSub", RedisChannel.PatternMode.Auto);
        private readonly ILogger<RedisPubSub> logger;
        private ISubscriber? subscriber;

        private class LoggerTextWriter : TextWriter
        {
            private readonly ILogger log;

            public LoggerTextWriter(ILogger log)
            {
                this.log = log;
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char value)
            {
            }

            public override void WriteLine(string? value)
            {
                if (log.IsEnabled(LogLevel.Debug))
                {
                    log.LogDebug(new EventId(100, "RedisConnectionLog"), value);
                }
            }
        }

        public RedisPubSub(IOptions<RedisPubSubOptions> options, ILogger<RedisPubSub> logger)
        {
            this.options = options.Value;

            this.logger = logger;

            subscriptions = new Subscriptions(logger);

            EnsureConnectedAsync().Forget();
        }

        public void Dispose()
        {
            subscriber?.Unsubscribe(redisChannel);
        }

        public async Task PublishAsync(object? payload)
        {
            var currentSubscriber = await EnsureConnectedAsync();

            if (payload != null)
            {
                var value = $"{payload.GetType().FullName}|{JsonSerializer.Serialize(payload, Options)}";

                await currentSubscriber.PublishAsync(redisChannel, value, CommandFlags.None);
            }
            else
            {
                await currentSubscriber.PublishAsync(redisChannel, RedisValue.Null, CommandFlags.None);
            }
        }

        public async Task SubscribeAsync(Action<object?> subscriber)
        {
            await EnsureConnectedAsync();

            subscriptions.Subscribe(subscriber);
        }

        private async Task<ISubscriber> EnsureConnectedAsync()
        {
            if (subscriber == null)
            {
                await connectionLock.WaitAsync();
                try
                {
                    if (subscriber == null)
                    {
                        var writer = new LoggerTextWriter(logger);

                        var connection = await options.ConnectAsync(writer);

                        subscriber = connection.GetSubscriber();

                        await subscriber.SubscribeAsync(redisChannel, (_, v) =>
                        {
                            HandleMessage(v);
                        });
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to connect to redis.");
                    throw;
                }
                finally
                {
                    connectionLock.Release();
                }
            }

            return subscriber;
        }

        private void HandleMessage(RedisValue value)
        {
            if (subscriptions.IsEmpty)
            {
                return;
            }

            if (value.IsNullOrEmpty)
            {
                subscriptions.Publish(null);
            }

            try
            {
                HandleMessageCore(value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to handle payload {payload}", value);
            }
        }

        private void HandleMessageCore(string value)
        {
            var indexOfType = value.IndexOf('|');

            if (indexOfType <= 0)
            {
                logger.LogError("Invalid payload {payload}.", value);
            }

            var typeName = value.Substring(0, indexOfType);

            var type = Type.GetType(typeName);

            if (type == null)
            {
                logger.LogError("Cannot find type {typeName}.", typeName);
                return;
            }

            var json = value[(indexOfType + 1)..];

            var payload = JsonSerializer.Deserialize(json, type, Options);

            subscriptions.Publish(payload);
        }
    }
}
