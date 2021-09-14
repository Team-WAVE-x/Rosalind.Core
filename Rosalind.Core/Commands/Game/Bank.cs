using Discord.Commands;
using Rosalind.Core.Services;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Game
{
    public class Bank : ModuleBase<SocketCommandContext>
    {
        private readonly SqlService _sql;

        public Bank(SqlService sql)
        {
            _sql = sql;
        }

        [Command("은행")]
        public async Task BankAsync()
        {
            var user = _sql.GetUser(Context.Guild.Id, Context.User.Id);
            await ReplyAsync($"💰 {Context.User.Username}님은 현재 `{string.Format("{0:#,0}", user.Coin)}` 코인을 소지하고 있습니다.");
        }
    }
}
