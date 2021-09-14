using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Rosalind.Core.Services
{
    public class ReactService
    {
        private IServiceProvider _service;
        private DiscordSocketClient _client;

        public ReactService(IServiceProvider service)
        {
            _service = service;
            _client = service.GetRequiredService<DiscordSocketClient>();
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            _client.ReactionsCleared += OnReactionCleared;
        }

        public void AddReactionMessage(RestUserMessage message, ulong userId, ulong guildId, Dictionary<IEmote, Action> dictionaries, TimeSpan timeout, bool removeMessageAfterTimeOut = false)
        {
            var cache = MemoryCache.Default;
            var policy = new CacheItemPolicy { SlidingExpiration = timeout, RemovedCallback = CacheRemovedCallback };
            var reactMessage = new ReactMessage(message, userId, guildId, dictionaries, removeMessageAfterTimeOut);

            cache.Add(message.Id.ToString(), reactMessage, policy);

            foreach (var item in dictionaries)
            {
                message.AddReactionAsync(item.Key);
            }

            Console.WriteLine("{0} {1,-11} {2}", DateTime.Now.ToString("HH:mm:ss"), "React", $"Cached {message.Id}");
        }

        public void RemoveReactionMessage(ulong messageId)
        {
            var cache = MemoryCache.Default;
            cache.Remove(messageId.ToString());
        }

        private async void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
        {
            var reactMessage = arguments.CacheItem.Value as ReactMessage;

            if (reactMessage.RemoveMessageAfterTimeOut)
            {
                try
                {
                    var message = await (_client.GetGuild(reactMessage.MessageGuildId).GetChannel(reactMessage.Message.Channel.Id) as ISocketMessageChannel).GetMessageAsync(reactMessage.Message.Id) as RestUserMessage;
                    await message.DeleteAsync();
                }
                catch (Exception)
                {
                    Console.WriteLine("{0} {1,-11} {2}", DateTime.Now.ToString("HH:mm:ss"), "React", $"Error deleting message! ({reactMessage.Message.Id})");
                }
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            var cache = MemoryCache.Default;
            var reactMessage = cache.Get(message.Id.ToString()) as ReactMessage; //캐싱된 ReactMessage 객체

            if (message != null && reactMessage != null && reaction.User.IsSpecified && reaction.UserId != _client.CurrentUser.Id && reactMessage.Dictionaries.ContainsKey(reaction.Emote) && reactMessage.MessageUserId == reaction.UserId)
            {
                Console.WriteLine("{0} {1,-11} {2}", DateTime.Now.ToString("HH:mm:ss"), "React", $"{reaction.User.Value} Added React At {message.Id}");

                foreach (var item in reactMessage.Dictionaries)
                {
                    if (item.Key.Equals(reaction.Emote))
                    {
                        item.Value(); //대리자 실행
                    }
                }

                await message.RemoveReactionAsync(reaction.Emote, reaction.UserId); //유저 반응 제거
            }
        }

        private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();

            if (reaction.UserId == _client.CurrentUser.Id)
            {
                await message.AddReactionAsync(reaction.Emote);
            }
        }

        private async Task OnReactionCleared(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> channel)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            var cache = MemoryCache.Default;
            var reactMessage = cache.Get(message.Id.ToString()) as ReactMessage;

            if (reactMessage != null)
            {
                foreach (var item in reactMessage.Dictionaries)
                {
                    await message.AddReactionAsync(item.Key);
                }
            }
        }
    }
}