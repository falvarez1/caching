// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Caching
{
    public interface IReplicatedCache
    {
        Task AddAsync(string key, object? value, TimeSpan expiration, bool invalidate = false);

        Task RemoveAsync(string key);

        bool TryGetValue(string key, out object? value);
    }
}
