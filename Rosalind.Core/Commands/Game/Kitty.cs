using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using Rosalind.Core.Models;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Game;

public class Kitty : ModuleBase<SocketCommandContext>
{
    private readonly ComponentService _component;

    public Kitty(ComponentService component)
    {
        _component = component;
    }

    [Command("야옹이")]
    public async Task KittyAsync()
    {
        //TODO: 그냥 전부 다시 작성하셈
        
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync("https://aws.random.cat/meow");
        string responseBody = await response.Content.ReadAsStringAsync();

        var imageUrl = JObject.Parse(responseBody).SelectToken("file")?.ToString();

        var embed = new EmbedBuilder();
        embed.WithTitle("🐱 고양이");
        embed.WithColor(Color.LightOrange);
        embed.WithImageUrl(imageUrl);
        embed.WithFooter(new EmbedFooterBuilder
        {
            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
            Text = $"{Context.User.Username}"
        });
        embed.WithTimestamp(DateTimeOffset.Now);

        #region Component Message Delegate

        async void NextAction(SocketInteraction interaction, ComponentMessage message)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync("https://aws.random.cat/meow");
            string responseBody = await response.Content.ReadAsStringAsync();

            var imageUrl = JObject.Parse(responseBody).SelectToken("file")?.ToString();

            var builder = new EmbedBuilder();
            builder.WithTitle("🐱 고양이");
            builder.WithColor(Color.LightOrange);
            builder.WithImageUrl(responseBody);
            builder.WithFooter(new EmbedFooterBuilder {IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128), Text = $"{Context.User.Username}"});
            builder.WithTimestamp(DateTimeOffset.Now);

            await interaction.DeferAsync();
            await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = builder.Build());
        }

        void CloseAction(SocketInteraction interaction, ComponentMessage message)
        {
            ComponentService.RemoveComponentMessage(message.MessageId);
        }

        #endregion

        var dictionary = new Dictionary<Button, Action<SocketInteraction, ComponentMessage>>()
        {
            { new Button("다음 이미지", "next", new Emoji("▶️"), style: ButtonStyle.Primary), NextAction },
            { new Button("제거", "delete", new Emoji("🛑"), style: ButtonStyle.Danger), CloseAction }
        };

        await _component.SendComponentMessage(Context, dictionary, embed: embed.Build(), removeMessageAfterTimeOut: true);
    }
}