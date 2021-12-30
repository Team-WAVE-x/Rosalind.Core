using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;

namespace Rosalind.Core.Services;

public class ComponentService
{
    //TODO: 이 코드는 끔찍합니다.
    
    private readonly DiscordSocketClient _client;

    public ComponentService(IServiceProvider service)
    {
        _client = service.GetRequiredService<DiscordSocketClient>();
        _client.InteractionCreated += OnInteractionCreated;
    }

    public async Task<RestUserMessage> SendComponentMessage(SocketCommandContext context, Dictionary<Button, Action<SocketInteraction, ComponentMessage>> dictionary, string content = null, Embed embed = null, ulong seconds = 500, bool removeMessageAfterTimeOut = false)
    {
        var cache = MemoryCache.Default;
        var policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromSeconds(seconds), RemovedCallback = CacheRemovedCallback };
        var builder = new ComponentBuilder();
            
        foreach (var (key, _) in dictionary)
        {
            builder.WithButton(key.Label, key.CustomId, key.Style, key.Emote, row: key.Row);
        }

        var message = await context.Channel.SendMessageAsync(content, embed: embed, components: builder.Build());
        var componentMessage = new ComponentMessage(message.Id, context.User.Id, context.Channel.Id, context.Guild.Id, dictionary, removeMessageAfterTimeOut);

        cache.Add(message.Id.ToString(), componentMessage, policy);

        Console.WriteLine("{0:HH:mm:ss} {1,-11} {2}", DateTime.Now, "Component", $"Cached {message.Id}");

        return message;
    }

    public static void RemoveComponentMessage(ulong messageId)
    {
        var cache = MemoryCache.Default;
        cache.Remove(messageId.ToString());
    }

    private static Task OnInteractionCreated(SocketInteraction arg)
    {
        var cache = MemoryCache.Default;
            
        if (arg is not SocketMessageComponent socketMessageComponent) return Task.CompletedTask;
            
        if (cache.Get(socketMessageComponent.Message.Id.ToString()) is not ComponentMessage componentMessage ||
            socketMessageComponent.User.Id != componentMessage.MessageUserId ||
            socketMessageComponent.Message.Id != componentMessage.MessageId) return Task.CompletedTask;
            
        foreach (var item in componentMessage.Dictionary.Where(item => socketMessageComponent.Data.CustomId == item.Key.CustomId))
        {
            item.Value(arg, componentMessage);
        }

        return Task.CompletedTask;
    }

    private async void CacheRemovedCallback(CacheEntryRemovedArguments arguments)
    {
        if (arguments.CacheItem.Value is not ComponentMessage { RemoveMessageAfterTimeOut: true } componentMessage) return;
            
        try
        {
            if (await (_client.GetGuild(componentMessage.MessageGuildId).GetChannel(componentMessage.MessageChannelId) as ISocketMessageChannel)?.GetMessageAsync(componentMessage.MessageId)! is RestUserMessage message) await message.DeleteAsync();
        }
        catch (Exception)
        {
            Console.WriteLine("{0:HH:mm:ss} {1,-11} {2}", DateTime.Now, "Component", $"Error deleting message! ({componentMessage.MessageId})");
        }
    }
}