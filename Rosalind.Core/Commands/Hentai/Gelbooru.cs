using Discord;
using Discord.Commands;
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
        private readonly ReactService _react;

        public Gelbooru(ReactService react)
        {
            _react = react;
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

            var message = await Context.Channel.SendMessageAsync(embed: embed.Build()); //메시지 전송하고 객체 캡처
            var tagMessage = await Context.Channel.SendMessageAsync($"태그: `{string.Join(", ", result.Tags)}`"); //태그 메시지도 전송하고 캡처

            #region ReactMessage Delegate
            Action nextAction = async delegate
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

                await message.ModifyAsync(msg => msg.Embed = embed.Build());
                await tagMessage.ModifyAsync(msg => msg.Content = $"태그: `{string.Join(", ", result.Tags)}`");
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

            _react.AddReactionMessage(message, Context.User.Id, Context.Guild.Id, dictionary, TimeSpan.FromMinutes(5));
        }
    }
}
