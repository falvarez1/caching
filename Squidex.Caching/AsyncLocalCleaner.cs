// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;

namespace Squidex.Caching
{
    internal sealed class AsyncLocalCleaner<T> : IDisposable
    {
        private readonly AsyncLocal<T> asyncLocal;

        public AsyncLocalCleaner(AsyncLocal<T> asyncLocal)
        {
            this.asyncLocal = asyncLocal ?? throw new ArgumentNullException(nameof(asyncLocal));
        }

        public void Dispose()
        {
            asyncLocal.Value = default!;
        }
    }
}
