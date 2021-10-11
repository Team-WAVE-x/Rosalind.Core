using Discord;
using Discord.Commands;
using Rosalind.Core.Preconditions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Management
{
    public class Cmd : ModuleBase<SocketCommandContext>
    {
        [Developer]
        [Command("명령어")]
        public async Task CmdAsync([Remainder] string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                await Context.Channel.SendMessageAsync("❌ 명령어를 입력하여 주십시오.");
                return;
            }

            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C {command}",
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            process.Start();

            var result = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            var embed = new EmbedBuilder()
            {
                Title = "👨‍💻 명령 프롬프트",
                Color = Color.Blue,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder() { Name = "명령어", Value = $"```{command}```" },
                    new EmbedFieldBuilder() { Name = "실행 결과", Value = $"```{(result.Length <= 1000 ? result : result.Substring(0, 1000) + "\n...")}```" }
                }
            };

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}