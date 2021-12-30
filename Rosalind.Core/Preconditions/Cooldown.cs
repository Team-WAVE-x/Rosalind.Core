using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rosalind.Core.Preconditions;

/// <summary>
/// 명령어의 쿨다운을 지정합니다.
/// </summary>
public class Cooldown : PreconditionAttribute
{
    private readonly int _cooldownLength;
    private readonly string _errorMessage;

    /// <summary>
    /// 쿨다운을 설정합니다.
    /// </summary>
    /// <param name="cooldownLength">쿨다운의 길이를 초로 정의합니다.</param>
    /// <param name="errorMessage">쿨다운이 끝나지 않았을 때 표시할 메시지를 의미합니다.</param>
    public Cooldown(int cooldownLength, string errorMessage = "❌ 본 명령어를 다시 사용하시려면 {TIME}초 더 기다리셔야 해요!")
    {
        _cooldownLength = cooldownLength;
        _errorMessage = errorMessage;
    }
    
    /// <summary>
    /// 유저가 명령어를 사용할 수 있는지 확인합니다.
    /// </summary>
    /// <param name="context">명령어의 컨텍스트</param>
    /// <param name="command">명령어 객체</param>
    /// <param name="services">서비스</param>
    /// <returns>명령어가 사용 가능 한지 반환합니다.</returns>
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
    {
        var stackCooldownTimer = new List<DateTimeOffset>();
        var stackCooldownTarget = new List<SocketGuildUser>();

        if (stackCooldownTarget.Contains(context.User as SocketGuildUser))
        {
            if (stackCooldownTimer[stackCooldownTarget.IndexOf(context.Message.Author as SocketGuildUser)].AddSeconds(_cooldownLength) >= DateTimeOffset.Now)
            {
                int secondsLeft = (int)(stackCooldownTimer[stackCooldownTarget.IndexOf(context.Message.Author as SocketGuildUser)].AddSeconds(_cooldownLength) - DateTimeOffset.Now).TotalSeconds;
                string errorMessage = _errorMessage.Replace("{TIME}", secondsLeft.ToString());

                context.Channel.SendMessageAsync(errorMessage);
                return Task.FromResult(PreconditionResult.FromError(errorMessage));
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
        else
        {
            stackCooldownTarget.Add(context.User as SocketGuildUser);
            stackCooldownTimer.Add(DateTimeOffset.Now);

            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}