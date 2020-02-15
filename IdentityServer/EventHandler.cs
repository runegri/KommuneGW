// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using IdentityServer4.Services;
using IdentityServer4.Events;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class EventHandler : IEventSink
    {
        public Task PersistAsync(Event evt)
        {
            return Task.CompletedTask;
        }
    }
}