using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Game
{
    public class Kitty : ModuleBase<SocketCommandContext>
    {
        private readonly ReactService _react;

        public Kitty(ReactService react)
        {
            _react = react;
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

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build());

            #region ReactMessage Delegate
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
                _react.RemoveReactionMessage(message.Id);
            };
            #endregion

            var dictionary = new Dictionary<IEmote, Action>
            {
                { new Emoji("▶️"), nextAction },
                { new Emoji("🛑"), closeAction }
            };

            _react.AddReactionMessage(message, Context.User.Id, Context.Guild.Id, dictionary, TimeSpan.FromSeconds(10));
        }
    }
}
