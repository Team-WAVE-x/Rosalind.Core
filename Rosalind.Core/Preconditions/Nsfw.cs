using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Rosalind.Core.Preconditions
{
    public class Nsfw : PreconditionAttribute
    {
        private readonly string _errorMessage;

        public Nsfw(string errorMessage = "❌ 본 명령어는 NSFW 채널에서만 사용할 수 있습니다.")
        {
            _errorMessage = errorMessage;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var channel = context.Channel as ITextChannel;

            if (channel != null && channel.IsNsfw)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                context.Channel.SendMessageAsync(_errorMessage);
                return Task.FromResult(PreconditionResult.FromError(_errorMessage));
            }
        }
    }
}
