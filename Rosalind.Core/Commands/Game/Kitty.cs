using Discord;
using Discord.Commands;
using Discord.Rest;
using Newtonsoft.Json.Linq;
using Rosalind.Core.Models;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Game
{
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
            var client = new WebClient();
            var imageUrl = JObject.Parse(client.DownloadString("http://aws.random.cat/meow")).SelectToken("file").ToString();

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
            RestUserMessage message = null;

            Action nextAction = async delegate
            {
                var client = new WebClient();
                var imageUrl = JObject.Parse(client.DownloadString("http://aws.random.cat/meow")).SelectToken("file").ToString();

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

                await message.ModifyAsync(msg => msg.Embed = embed.Build());
            };

            Action closeAction = delegate
            {
                _component.RemoveComponentMessage(message.Id);
            };
            #endregion

            var dictionary = new Dictionary<Button, Action>()
            {
                { new Button("다음 이미지", "next", new Emoji("▶️"), style: ButtonStyle.Primary), nextAction },
                { new Button("제거", "delete", new Emoji("🛑"), style: ButtonStyle.Danger), closeAction }
            };

            message = await _component.SendComponentMessage(Context, dictionary, embed: embed.Build(), removeMessageAfterTimeOut: true);
        }
    }
}
