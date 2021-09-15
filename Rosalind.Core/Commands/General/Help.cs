using Discord;
using Discord.Commands;
using KillersLibrary.EmbedPages;
using Rosalind.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rosalind.Core.Commands.General
{
    public class Help : ModuleBase<SocketCommandContext>
    {
        private readonly Setting _setting;
        private readonly EmbedPagesService _embedPagesService;

        public Help(Setting setting, EmbedPagesService embedPagesService)
        {
            _setting = setting;
            _embedPagesService = embedPagesService;
        }

        [Command("도움말")]
        public async Task HelpAsync()
        {
            var groups = _setting.CommandGroup;
            var embeds = new List<EmbedBuilder>();
            var style = new EmbedPagesStyles()
            {
                FirstLabel = "⏪",
                BackLabel = "◀️",
                DelEmoji = "🛑",
                ForwardLabel = "▶️",
                LastLabel = "⏩",
                DeletionMessage = "🛑 대기 시간이 초과되었습니다.",
                FastChangeBtns = true,
                PageNumbers = true
            };

            foreach (var item in groups)
            {
                var embed = new EmbedBuilder();

                embed.WithTitle(item.Title);
                embed.WithDescription(item.Description);
                embed.WithColor(new Color(Convert.ToUInt32(item.Color, 16)));
                embed.WithFooter(new EmbedFooterBuilder
                {
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = Context.User.Username
                });

                foreach (var commands in item.Commands)
                {
                    embed.AddField($"{_setting.Config.Prefix}{commands.Name} {string.Join(", ", commands.Args.Select(x => "{" + x + "}"))}", commands.Description);
                }

                embeds.Add(embed);
            }

            await _embedPagesService.CreateEmbedPages(Context.Client, embeds, Context, styles: style);
        }
    }
}