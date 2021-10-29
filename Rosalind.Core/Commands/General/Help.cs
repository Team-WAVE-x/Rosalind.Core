using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rosalind.Core.Models;
using Rosalind.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.General
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly Setting _setting;
        private readonly ComponentService _component;

        public Help(Setting setting, ComponentService component)
        {
            _setting = setting;
            _component = component;
        }

        [Command("도움말")]
        public async Task HelpAsync()
        {
            int page = 0;

            Embed GetPageEmbed(int page)
            {
                var embed = new EmbedBuilder();
                embed.WithTitle(_setting.CommandGroup[page].Title);
                embed.WithColor(new Color(Convert.ToUInt32(_setting.CommandGroup[page].Color, 16)));
                embed.WithDescription(_setting.CommandGroup[page].Description);
                embed.WithFooter(new EmbedFooterBuilder
                {
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = $"{Context.User.Username} 페이지 {page + 1}/{_setting.CommandGroup.Length}"
                });
                embed.WithCurrentTimestamp();

                foreach (var item in _setting.CommandGroup[page].Commands)
                {
                    embed.AddField($"{_setting.Config.Prefix}{item.Name} {string.Join(", ", item.Args.Select(x => "{" + x + "}"))}", item.Description);
                }

                return embed.Build();
            }

            #region Component Message Delegate
            Action<SocketInteraction, ComponentMessage> lastAction = delegate (SocketInteraction interaction, ComponentMessage message)
            {
                page = 0;

                interaction.DeferAsync();
                interaction.ModifyOriginalResponseAsync(msg => msg.Embed = GetPageEmbed(page));
            };

            Action<SocketInteraction, ComponentMessage> previousAction = delegate (SocketInteraction interaction, ComponentMessage message)
            {
                if (page != 0)
                    page -= 1;

                interaction.DeferAsync();
                interaction.ModifyOriginalResponseAsync(msg => msg.Embed = GetPageEmbed(page));
            };

            Action<SocketInteraction, ComponentMessage> stopAction = delegate (SocketInteraction interaction, ComponentMessage message)
            {
                _component.RemoveComponentMessage(message.MessageId);
            };

            Action<SocketInteraction, ComponentMessage> nextAction = delegate (SocketInteraction interaction, ComponentMessage message)
            {
                if (page != _setting.CommandGroup.Length - 1)
                    page += 1;

                interaction.DeferAsync();
                interaction.ModifyOriginalResponseAsync(msg => msg.Embed = GetPageEmbed(page));
            };

            Action<SocketInteraction, ComponentMessage> frontAction = delegate (SocketInteraction interaction, ComponentMessage message)
            {
                page = _setting.CommandGroup.Length - 1;

                interaction.DeferAsync();
                interaction.ModifyOriginalResponseAsync(msg => msg.Embed = GetPageEmbed(page));
            };
            #endregion

            var dictionary = new Dictionary<Button, Action<SocketInteraction, ComponentMessage>>()
            {
                { new Button("⏮", "last", style: ButtonStyle.Primary), lastAction },
                { new Button("◀", "previous", style: ButtonStyle.Success), previousAction },
                { new Button("⏹", "stop", style: ButtonStyle.Danger), stopAction },
                { new Button("▶", "next", style: ButtonStyle.Success), nextAction },
                { new Button("⏭", "front", style: ButtonStyle.Primary), frontAction }
            };

            await _component.SendComponentMessage(Context, dictionary, embed: GetPageEmbed(page), removeMessageAfterTimeOut: true);
        }
    }
}