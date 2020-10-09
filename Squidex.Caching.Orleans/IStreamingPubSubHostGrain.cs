// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Caching.Orleans
{
    public interface IStreamingPubSubHostGrain : IGrainWithStringKey
    {
        Task SendAsync(object payload);

        Task ActivateAsync();
    }
}
