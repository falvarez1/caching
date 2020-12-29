// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.TestingHost;
using Xunit;

namespace Squidex.Caching
{
    [Trait("Category", "Dependencies")]
    public class OrleansStreamingPubSubTests
    {
        private sealed class Configurator : ISiloConfigurator
        {
            public void Configure(ISiloBuilder siloBuilder)
            {
                siloBuilder.ConfigureLogging(builder =>
                {
                    builder.AddConsole();
                    builder.AddDebug();
                });

                siloBuilder.AddOrleansPubSub();
                siloBuilder.AddStartupTask<SiloHandle>();
            }
        }

        protected sealed class SiloHandle : IStartupTask, IDisposable
        {
            private static readonly ConcurrentDictionary<SiloHandle, SiloHandle> AllSilos = new ConcurrentDictionary<SiloHandle, SiloHandle>();

            public IPubSub PubSub { get; }

            public static ICollection<SiloHandle> All => AllSilos.Keys;

            public SiloHandle(IPubSub pubSub)
            {
                PubSub = pubSub;
            }

            public static void Clear()
            {
                AllSilos.Clear();
            }

            public Task Execute(CancellationToken cancellationToken)
            {
                AllSilos.TryAdd(this, this);

                return Task.CompletedTask;
            }

            public void Dispose()
            {
                AllSilos.TryRemove(this, out _);
            }
        }

        public OrleansStreamingPubSubTests()
        {
            SiloHandle.Clear();
        }

        [Fact]
        public async Task Should_receive_simple_pubsub_messages()
        {
            var (pubSubs, cluster) = await CreateSilosAsync();

            try
            {
                await WaitForClusterSizeAsync(cluster, 3);

                await PubSubTestHelper.PublishSimpleValuesAsync(pubSubs, 3);
            }
            finally
            {
                await ReleaseAsync(cluster);
            }
        }

        [Fact]
        public async Task Should_receive_complex_pubsub_messages()
        {
            var (pubSubs, cluster) = await CreateSilosAsync();

            try
            {
                await WaitForClusterSizeAsync(cluster, 3);

                await PubSubTestHelper.PublishComplexValuesAsync(pubSubs, 3);
            }
            finally
            {
                await ReleaseAsync(cluster);
            }
        }

        [Fact]
        public async Task Should_not_send_message_to_dead_member()
        {
            var (pubSubs, cluster) = await CreateSilosAsync();

            try
            {
                await cluster.StopSiloAsync(cluster.Silos[1]);

                await WaitForClusterSizeAsync(cluster, 2);

                await PubSubTestHelper.PublishComplexValuesAsync(pubSubs, 2);
            }
            finally
            {
                await ReleaseAsync(cluster);
            }
        }

        [Fact]
        public async Task Should_not_send_message_to_dead_but_not_unregistered_member()
        {
            var (pubSubs, cluster) = await CreateSilosAsync();

            try
            {
                await cluster.StopSiloAsync(cluster.Silos[1]);

                await PubSubTestHelper.PublishComplexValuesAsync(pubSubs, 2);
            }
            finally
            {
                await ReleaseAsync(cluster);
            }
        }

        [Fact]
        public async Task Should_send_message_to_new_member()
        {
            var (pubSubs, cluster) = await CreateSilosAsync();

            try
            {
                cluster.StartAdditionalSilo();

                await WaitForClusterSizeAsync(cluster, 4);

                await PubSubTestHelper.PublishComplexValuesAsync(pubSubs, 4);
            }
            finally
            {
                await ReleaseAsync(cluster);
            }
        }

        private static async Task<(IList<IPubSub>, TestCluster)> CreateSilosAsync()
        {
            var cluster =
                new TestClusterBuilder(3)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            return (SiloHandle.All.Select(x => x.PubSub).ToList(), cluster);
        }

        private static async Task<bool> WaitForClusterSizeAsync(TestCluster cluster, int expectedSize)
        {
            var managementGrain = cluster.Client.GetGrain<IManagementGrain>(0);

            var timeout = TestCluster.GetLivenessStabilizationTime(new ClusterMembershipOptions());

            var stopWatch = Stopwatch.StartNew();
            do
            {
                var hosts = await managementGrain.GetHosts();

                if (hosts.Count == expectedSize)
                {
                    stopWatch.Stop();
                    return true;
                }

                await Task.Delay(100);
            }
            while (stopWatch.Elapsed < timeout);

            return false;
        }

        protected static async Task ReleaseAsync(TestCluster value)
        {
            await Task.WhenAny(Task.Delay(1000), value.DisposeAsync().AsTask());
        }
    }
}
