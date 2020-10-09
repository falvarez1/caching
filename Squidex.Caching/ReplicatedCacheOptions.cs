﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Caching
{
    public sealed class ReplicatedCacheOptions
    {
        public bool Enable { get; set; }

        public bool WaitForAcknowledgment { get; set; } = true;
    }
}
