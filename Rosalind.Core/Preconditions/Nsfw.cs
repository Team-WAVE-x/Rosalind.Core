using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Rosalind.Core.Preconditions;

/// <summary>
/// 명령어를 NSFW 채널에서만 사용할 수 있도록 합니다.
/// </summary>
public class Nsfw : PreconditionAttribute
{
    private readonly string _errorMessage;

    /// <summary>
    /// 명령어를 NSFW 채널에서만 사용할 수 있도록 만듭니다.
    /// </summary>
    /// <param name="errorMessage">NSFW 채널이 아닌 곳에서 명령어를 실행했을 때 표시할 메시지입니다.</param>
    public Nsfw(string errorMessage = "❌ 본 명령어는 NSFW 채널에서만 사용할 수 있습니다.")
    {
        _errorMessage = errorMessage;
    }
    
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        if (context.Channel is ITextChannel {IsNsfw: true})
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