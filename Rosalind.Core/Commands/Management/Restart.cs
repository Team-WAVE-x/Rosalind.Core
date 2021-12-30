using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rosalind.Core.Models;
using Rosalind.Core.Preconditions;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.Management
{
    public class Restart : ModuleBase<SocketCommandContext>
    {
        private readonly ComponentService _component;

        public Restart(ComponentService component)
        {
            _component = component;
        }

        [Developer]
        [Command("재시작")]
        public async Task RestartAsync()
        {
            #region ReactMessage Delegate
            Action<SocketInteraction, ComponentMessage> okAction = delegate
            {
                System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            };

            Action<SocketInteraction, ComponentMessage> cancelAction = delegate (SocketInteraction interaction, ComponentMessage message)
            {
                ComponentService.RemoveComponentMessage(message.MessageId);
            };
            #endregion

            var dictionary = new Dictionary<Button, Action<SocketInteraction, ComponentMessage>>
            {
                { new Button("확인", "confirm", new Emoji("✅"), style: ButtonStyle.Primary), okAction },
                { new Button("취소", "cancel", new Emoji("❌"), style: ButtonStyle.Danger), cancelAction }
            };

            await _component.SendComponentMessage(Context, dictionary, "❓ 봇을 재시작할까요?", removeMessageAfterTimeOut: true);
        }
    }
}