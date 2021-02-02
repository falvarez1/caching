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
        Task AddAsync(string key, object? value, TimeSpan expiration);

        Task RemoveAsync(params string[] keys);

        bool TryGetValue(string key, out object? value);
    }
}
