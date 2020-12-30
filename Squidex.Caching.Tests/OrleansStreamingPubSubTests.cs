// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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
                siloBuilder.AddOrleansPubSub();
            }
        }

        [Fact]
        public async Task Should_receive_simple_pubsub_messages()
        {
            var cluster = await CreateSilosAsync();

            try
            {
                await WaitForClusterSizeAsync(cluster, 3);

                var pubSubs = GetPubSubs(cluster);

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
            var cluster = await CreateSilosAsync();

            try
            {
                await WaitForClusterSizeAsync(cluster, 3);

                var pubSubs = GetPubSubs(cluster);

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
            var cluster = await CreateSilosAsync();

            try
            {
                var pubSubs = GetPubSubs(cluster);

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
            var cluster = await CreateSilosAsync();

            try
            {
                var pubSubs = GetPubSubs(cluster);

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
            var cluster = await CreateSilosAsync();

            try
            {
                cluster.StartAdditionalSilo();

                await WaitForClusterSizeAsync(cluster, 4);

                var pubSubs = GetPubSubs(cluster);

                await PubSubTestHelper.PublishComplexValuesAsync(pubSubs, 4);
            }
            finally
            {
                await ReleaseAsync(cluster);
            }
        }

        private static async Task<TestCluster> CreateSilosAsync()
        {
            var cluster =
                new TestClusterBuilder(3)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            return cluster;
        }

        private static List<IPubSub> GetPubSubs(TestCluster cluster)
        {
            return cluster.Silos.OfType<InProcessSiloHandle>()
                .Select(x => x.SiloHost.Services.GetRequiredService<IPubSub>())
                .ToList();
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
