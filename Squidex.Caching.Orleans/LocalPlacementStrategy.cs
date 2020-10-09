// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Orleans.Placement;
using Orleans.Runtime;

namespace PubSubTest.Placement
{
    [Serializable]
    public class LocalPlacementStrategy : PlacementStrategy
    {
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class LocalPlacementAttribute : PlacementAttribute
    {
        public LocalPlacementAttribute()
            : base(new LocalPlacementStrategy())
        {
        }
    }
}
