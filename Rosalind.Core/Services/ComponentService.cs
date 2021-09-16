using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Rosalind.Core.Services
{
    public class ComponentService
    {
        private IServiceProvider _service;
        private DiscordSocketClient _client;

        public ComponentService(IServiceProvider service)
        {
            _service = service;
            _client = service.GetRequiredService<DiscordSocketClient>();
            _client.InteractionCreated += OnInteractionCreated;
        }

        public void SendComponentMessage(SocketCommandContext context, Dictionary<ComponentBuilder, Action> dictionaries, TimeSpan timeout, bool removeMessageAfterTimeOut = false)
        {
            var cache = MemoryCache.Default;
            var policy = new CacheItemPolicy { SlidingExpiration = timeout, RemovedCallback = CacheRemovedCallback };
        }

        private Task OnInteractionCreated(SocketInteraction arg)
        {
            throw new NotImplementedException();
        }

        private void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            throw new NotImplementedException();
        }
    }
}