using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Rosalind.Core.Preconditions
{
    public class Cooldown : PreconditionAttribute
    {
        private readonly int _cooldownLength;
        private readonly string _errorMessage;

        public Cooldown(int cooldownLength, string errorMessage = "❌ 본 명령어를 다시 사용하시려면 {TIME}초 더 기다리셔야 해요!")
        {
            _cooldownLength = cooldownLength;
            _errorMessage = errorMessage;
        }

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
}
