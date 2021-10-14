using Discord.Commands;
using Rosalind.Core.Models;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.General
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly Setting _setting;

        public Help(Setting setting)
        {
            _setting = setting;
        }

        [Command("도움말")]
        public async Task HelpAsync()
        {

        }
    }
}