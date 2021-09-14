using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Rosalind.Core.Services
{
    public class CommandHandlingService
    {
        private readonly Setting _setting;
        private readonly CommandService _command;
        private IServiceProvider _service;
        private DiscordSocketClient _client;

        public CommandHandlingService(IServiceProvider service)
        {
            _service = service;
            _setting = service.GetRequiredService<Setting>();
            _command = service.GetRequiredService<CommandService>();
            _client = service.GetRequiredService<DiscordSocketClient>();

            _client.MessageReceived += OnClientMessage;
            _command.Log += new LoggingService().OnLogReceived;
            _command.CommandExecuted += OnCommandExecuted;
        }

        public async Task InitializeAsync()
        {
            await _command.AddModulesAsync(Assembly.GetEntryAssembly(), _service);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess)
                return;

            var embed = new EmbedBuilder();
            embed.WithTitle("❌ 오류");
            embed.WithColor(Color.Red);
            embed.WithDescription($"`{result}`");
            embed.WithFooter(new EmbedFooterBuilder
            {
                IconUrl = _client.CurrentUser.GetAvatarUrl(),
                Text = _client.CurrentUser.Username
            });
            embed.WithTimestamp(DateTimeOffset.Now);

            await (context.Client.GetChannelAsync(_setting.Config.ErrorLogChannelId).Result as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());
        }

        private async Task OnClientMessage(SocketMessage socketMessage)
        {
            int argPos = 0;

            if (!(socketMessage is SocketUserMessage message) || message.Source != MessageSource.User || socketMessage.Channel is IPrivateChannel || !message.HasStringPrefix(_setting.Config.Prefix, ref argPos))
                return;

            SocketCommandContext context = new SocketCommandContext(_client, message);
            await _command.ExecuteAsync(context, argPos, _service);
        }
    }
}