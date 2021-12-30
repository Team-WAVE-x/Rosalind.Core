using Discord;
using Discord.Commands;
using Rosalind.Core.Services;
using System;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Game;

public class Rank : ModuleBase<SocketCommandContext>
{
    private readonly SqlService _sql;

    public Rank(SqlService sql)
    {
        _sql = sql;
    }

    [Command("랭킹")]
    public async Task RankAsync()
    {
        var message = await Context.Channel.SendMessageAsync("🧮 계산중...");
        var ranking = _sql.GetRanking(Context.Guild.Id, 20);
        var embed = new EmbedBuilder();

        embed.WithTitle("💰 서버 코인 랭킹");
        embed.WithColor(Color.LightOrange);
        embed.WithFooter(new EmbedFooterBuilder
        {
            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png),
            Text = Context.User.Username
        });
        embed.WithTimestamp(DateTimeOffset.Now);

        if (ranking.Count == 0)
        {
            embed.WithDescription("❌ 데이터가 존재하지 않습니다.");
        }
        else
        {
            for (int i = 0; i < ranking.Count; i++)
            {
                var user = await Context.Client.Rest.GetUserAsync(ranking[i].UserId);

                embed.AddField($"{i + 1}등",
                    ranking[i].UserId == Context.User.Id
                        ? $"**{user.Username}#{user.Discriminator}** - `{ranking[i].Coin:n0}` 코인"
                        : $"{user.Username}#{user.Discriminator} - `{ranking[i].Coin:n0}` 코인");
            }
        }

        await message.ModifyAsync(msg => { msg.Content = string.Empty; msg.Embed = embed.Build(); });
    }
}