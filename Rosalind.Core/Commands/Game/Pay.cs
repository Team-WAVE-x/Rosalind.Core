using Discord.Commands;
using Rosalind.Core.Preconditions;
using Rosalind.Core.Services;
using System;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Game;

public class Pay : ModuleBase<SocketCommandContext>
{
    private readonly SqlService _sql;

    public Pay(SqlService sql)
    {
        _sql = sql;
    }

    [Command("용돈")]
    [Cooldown(60)]
    public async Task HelpAsync()
    {
        Random random = new Random();
        int value = random.Next(1, 1000);

        _sql.AddUserCoin(Context.Guild.Id, Context.User.Id, Convert.ToUInt64(value));
        await ReplyAsync($"✅ 용돈으로 `{value:#,0}` 코인을 받았습니다!");
    }
}