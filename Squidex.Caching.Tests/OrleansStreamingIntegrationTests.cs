// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.TestingHost;
using Xunit;

namespace Squidex.Caching
{
    [Trait("Category", "Dependencies")]
    public sealed class OrleansStreamingIntegrationTests : ReplicatedCacheIntegrationTestBase<TestCluster>
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

        private sealed class SiloHandle : IStartupTask, IDisposable
        {
            private static readonly ConcurrentDictionary<SiloHandle, SiloHandle> AllSilos = new ConcurrentDictionary<SiloHandle, SiloHandle>();

            public static ICollection<SiloHandle> All => AllSilos.Keys;

            public DomainObject DomainObject { get; }

            public SiloHandle(IPubSub pubSub)
            {
                DomainObject = DomainObject.Create(pubSub);
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

        public OrleansStreamingIntegrationTests()
        {
            SiloHandle.Clear();
        }

        protected override async Task<(DomainObject[], TestCluster)> CreateObjectsAsync()
        {
            var cluster =
                new TestClusterBuilder(3)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            var domainObjects = SiloHandle.All.Select(x => x.DomainObject).ToArray();

            return (domainObjects, cluster);
        }

        protected override async Task ReleaseAsync(TestCluster value)
        {
            await Task.WhenAny(Task.Delay(1000), value.DisposeAsync().AsTask());
        }
    }
}
