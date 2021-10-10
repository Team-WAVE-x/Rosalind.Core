using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Rosalind.Core.Services
{
    public class CommandHandlingService
    {
        private ILog _log;
        private Setting _setting;
        private CommandService _command;
        private IServiceProvider _service;
        private DiscordSocketClient _client;

        public CommandHandlingService(IServiceProvider service)
        {
            _service = service;
            _setting = service.GetRequiredService<Setting>();
            _command = service.GetRequiredService<CommandService>();
            _client = service.GetRequiredService<DiscordSocketClient>();

            _log = LogManager.GetLogger("RollingActivityLog");
            _client.MessageReceived += OnClientMessage;
            _command.Log += OnLogReceived;
            _command.CommandExecuted += OnCommandExecuted;
        }

        private Task OnLogReceived(LogMessage log)
        {
            if (log.Severity == LogSeverity.Critical)
                _log.Fatal(log.Message ?? "Null");
            else if (log.Severity == LogSeverity.Error)
                _log.Error(log.Message ?? "Null");
            else if (log.Severity == LogSeverity.Warning)
                _log.Warn(log.Message ?? "Null");
            else if (log.Severity == LogSeverity.Info)
                _log.Info(log.Message ?? "Null");
            else if (log.Severity == LogSeverity.Verbose)
                _log.Info(log.Message ?? "Null");
            else if (log.Severity == LogSeverity.Debug)
                _log.Debug(log.Message ?? "Null");

            return Task.CompletedTask;
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