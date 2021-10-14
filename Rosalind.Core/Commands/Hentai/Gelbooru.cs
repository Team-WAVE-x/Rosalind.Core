using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rosalind.Core.Models;
using Rosalind.Core.Preconditions;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Hentai
{
    public class Gelbooru : ModuleBase<SocketCommandContext>
    {
        private readonly ComponentService _component;

        public Gelbooru(ComponentService component)
        {
            _component = component;
        }

        [Nsfw]
        [Command("겔부루")]
        public async Task GelbooruAsync([Remainder] string tags = null)
        {
            //처음 실행시
            var result = new BooruSharp.Search.Post.SearchResult();
            var booru = new BooruSharp.Booru.Gelbooru();

            try
            {
                result = await booru.GetRandomPostAsync(tags?.Split(null));
            }
            catch (HttpRequestException)
            {
                await Context.Channel.SendMessageAsync("❌ 해당 태그가 존재하지 않습니다.");
                return;
            }
            catch (BooruSharp.Search.TooManyTags)
            {
                await Context.Channel.SendMessageAsync("❌ 태그가 너무 많습니다!");
                return;
            }

            var embed = new EmbedBuilder();
            embed.WithTitle("Gelbooru");
            embed.WithColor(Color.Red);
            embed.WithImageUrl(result.FileUrl.AbsoluteUri);
            embed.AddField("아이디", $"`{result.ID}`");
            embed.AddField("생성일", $"`{result.Creation}`");
            embed.AddField("소스", $"`{(string.IsNullOrWhiteSpace(result.Source) ? "알 수 없음" : result.Source)}`");
            embed.WithFooter(new EmbedFooterBuilder
            {
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = $"{Context.User.Username}"
            });
            embed.WithTimestamp(DateTimeOffset.Now);

            #region ReactMessage Delegate
            Action<SocketInteraction, ComponentMessage> nextAction = async delegate (SocketInteraction interaction, ComponentMessage message)
            {
                var booru = new BooruSharp.Booru.Gelbooru();
                result = await booru.GetRandomPostAsync(tags?.Split(null));

                var embed = new EmbedBuilder();
                embed.WithTitle("Gelbooru");
                embed.WithColor(Color.Red);
                embed.WithImageUrl(result.FileUrl.AbsoluteUri);
                embed.AddField("아이디", $"`{result.ID}`");
                embed.AddField("생성일", $"`{result.Creation}`");
                embed.AddField("소스", $"`{(string.IsNullOrWhiteSpace(result.Source) ? "알 수 없음" : result.Source)}`");
                embed.WithFooter(new EmbedFooterBuilder
                {
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = $"{Context.User.Username}"
                });
                embed.WithTimestamp(DateTimeOffset.Now);

                await interaction.DeferAsync();
                await interaction.ModifyOriginalResponseAsync(msg => msg.Embed = embed.Build());
            };

            Action<SocketInteraction, ComponentMessage> tagAction = async delegate
            {
                string tags = string.Join(", ", result.Tags);
                await Context.Channel.SendMessageAsync((tags.Length <= 1000 ? tags : tags.Substring(0, 1000) + "\n..."));
            };

            Action<SocketInteraction, ComponentMessage> closeAction = delegate (SocketInteraction interaction, ComponentMessage message)
            {
                _component.RemoveComponentMessage(message.MessageId);
            };
            #endregion

            var dictionary = new Dictionary<Button, Action<SocketInteraction, ComponentMessage>>
            {
                { new Button("다음 이미지", "next", new Emoji("▶️"), style: ButtonStyle.Primary), nextAction },
                { new Button("태그", "tag", new Emoji("🏷"), style: ButtonStyle.Secondary), tagAction },
                { new Button("제거", "delete", new Emoji("🛑"), style: ButtonStyle.Danger), closeAction }
            };

            await _component.SendComponentMessage(Context, dictionary, embed: embed.Build(), removeMessageAfterTimeOut: true);
        }
    }
}
