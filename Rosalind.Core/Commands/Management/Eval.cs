using Discord;
using Discord.Commands;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Rosalind.Core.Preconditions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Management
{
    public class Eval : ModuleBase<SocketCommandContext>
    {
        [Developer]
        [Command("계산")]
        public async Task EvalAsync([Remainder] string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                await Context.Channel.SendMessageAsync("❌ 명령어를 입력하여 주십시오.");
                return;
            }

            var result = CSharpScript.EvaluateAsync(expression).Result;

            var embed = new EmbedBuilder()
            {
                Title = "🧮 수식 계산기",
                Color = Color.Purple,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder() { Name = "수식", Value = $"```{expression}```" },
                    new EmbedFieldBuilder() { Name = "실행 결과", Value = $"```{result}```" }
                }
            };

            await Context.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}