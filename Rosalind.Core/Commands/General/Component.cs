using Discord.Commands;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.General
{
    public class Component : ModuleBase<SocketCommandContext>
    {
        private readonly ComponentService _component;

        public Component(ComponentService component)
        {
            _component = component;
        }

        [Command("버튼")]
        public async Task ButtonAsync()
        {
            var dictionary = new Dictionary<Models.Button, Action>();
            dictionary.Add(new Models.Button("안녕", "test"), () => ReplyAsync("ㅎㅇ"));

            await _component.SendComponentMessage("테스트", Context, dictionary, seconds: 10, removeMessageAfterTimeOut: true);
        }
    }
}