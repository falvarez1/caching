// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans.Runtime;
using Orleans.Runtime.Placement;

namespace PubSubTest.Placement
{
    public sealed class LocalPlacementDirector : IPlacementDirector
    {
        private Task<SiloAddress>? cachedLocalSilo;

        public Task<SiloAddress> OnAddActivation(PlacementStrategy strategy, PlacementTarget target, IPlacementContext context)
        {
            cachedLocalSilo ??= Task.FromResult(context.LocalSilo);

            return cachedLocalSilo;
        }
    }
}
