using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using System;
using System.Threading.Tasks;

namespace Rosalind.Core.Preconditions;

/// <summary>
/// 명령어를 개발자만 사용할 수 있도록 합니다.
/// </summary>
public class Developer : PreconditionAttribute
{
    private readonly string _errorMessage;

    /// <summary>
    /// 명령어를 개발자만 사용할 수 있도록 만듭니다.
    /// </summary>
    /// <param name="errorMessage">개발자가 아닌 유저가 명령어를 사용할 경우 표시할 메시지를 의미합니다.</param>
    public Developer(string errorMessage = "❌ 본 명령어는 개발자만 사용할 수 있습니다.")
    {
        _errorMessage = errorMessage;
    }
    
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var setting = services.GetRequiredService<Setting>();

        if (context.User.Id == setting.Config.DeveloperId)
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