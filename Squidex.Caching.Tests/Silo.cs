// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Orleans.Runtime;

namespace Squidex.Caching
{
    public sealed class Silo : IStartupTask, IDisposable
    {
        private static readonly List<Silo> AllSilos = new List<Silo>();
        private readonly HashSet<object> received = new HashSet<object>();

        public IPubSub PubSub { get; }

        public static IReadOnlyCollection<Silo> All => AllSilos;

        public IReadOnlyCollection<object> Received => received;

        public Silo(IPubSub pubSub)
        {
            PubSub = pubSub;
        }

        public static void Clear()
        {
            AllSilos.Clear();
        }

        public Task Execute(CancellationToken cancellationToken)
        {
            lock (AllSilos)
            {
                AllSilos.Add(this);
            }

            PubSub.SubscribeAsync(message =>
            {
                received.Add(message);
            });

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            lock (AllSilos)
            {
                AllSilos.Remove(this);
            }
        }
    }
}
