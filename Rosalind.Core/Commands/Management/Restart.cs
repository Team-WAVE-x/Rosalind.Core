using Discord;
using Discord.Commands;
using Discord.Rest;
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
            RestUserMessage message = null;

            Action okAction = delegate
            {
                System.Diagnostics.Process.Start(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            };

            Action cancelAction = delegate
            {
                _component.RemoveComponentMessage(message.Id);
            };
            #endregion

            var dictionary = new Dictionary<Button, Action>
            {
                { new Button("확인", "confirm", new Emoji("✅"), style: ButtonStyle.Primary), okAction },
                { new Button("취소", "cancel", new Emoji("❌"), style: ButtonStyle.Danger), cancelAction }
            };

            message = await _component.SendComponentMessage(Context, dictionary, "❓ 봇을 재시작할까요?", removeMessageAfterTimeOut: true);
        }
    }
}