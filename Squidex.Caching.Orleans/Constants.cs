// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Caching.Orleans
{
    public static class Constants
    {
        public const string BrokerId = "DEFAULT";
        public const string StreamProviderName = "PubSub";

        public static readonly TimeSpan SendTimeout = TimeSpan.FromMilliseconds(100);

        public static readonly Guid StreamId = default;
    }
}
