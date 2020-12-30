// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Hosting;
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
                siloBuilder.AddOrleansPubSub();
            }
        }

        protected override async Task<(DomainObject[], TestCluster)> CreateObjectsAsync()
        {
            var cluster =
                new TestClusterBuilder(3)
                    .AddSiloBuilderConfigurator<Configurator>()
                    .Build();

            await cluster.DeployAsync();

            var domainObjects =
                cluster.Silos.OfType<InProcessSiloHandle>()
                    .Select(x =>
                    {
                        var pubSub = x.SiloHost.Services.GetRequiredService<IPubSub>();

                        return DomainObject.Create(pubSub);
                    }).ToArray();

            return (domainObjects, cluster);
        }

        protected override async Task ReleaseAsync(TestCluster value)
        {
            await Task.WhenAny(Task.Delay(1000), value.DisposeAsync().AsTask());
        }
    }
}
