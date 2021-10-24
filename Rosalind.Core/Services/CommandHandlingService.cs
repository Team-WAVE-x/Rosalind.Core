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

            switch (result.Error)
            {
                case CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync("❌ 인자의 수가 올바르지 않습니다.");
                    break;
                case CommandError.Exception:
                    await context.Channel.SendMessageAsync("❌ 명령어 실행 도중 알 수 없는 오류가 발생했습니다.");

                    var embed = new EmbedBuilder();
                    embed.WithTitle("❌ 예외 발생");
                    embed.AddField("날짜", DateTime.Now.ToString("D"));
                    embed.AddField("내용", result.ErrorReason);
                    embed.WithColor(Color.Red);
                    embed.WithFooter(new EmbedFooterBuilder
                    {
                        IconUrl = context.User.GetAvatarUrl(ImageFormat.Png, 128),
                        Text = $"{context.User.Username}"
                    });
                    embed.WithTimestamp(DateTimeOffset.Now);

                    await (context.Client.GetChannelAsync(_setting.Config.ErrorLogChannelId).Result as ISocketMessageChannel).SendMessageAsync(embed: embed.Build());

                    break;
                case CommandError.MultipleMatches:
                    await context.Channel.SendMessageAsync("❌ 인자 분석중 오류가 발생하였습니다.");
                    break;
                case CommandError.ObjectNotFound:
                    await context.Channel.SendMessageAsync("❌ 인자를 찾을 수 없습니다.");
                    break;
                case CommandError.ParseFailed:
                    await context.Channel.SendMessageAsync("❌ 인자 분석에 실패하였습니다.");
                    break;
                case CommandError.UnknownCommand:
                    await context.Channel.SendMessageAsync("❌ 알 수 없는 명령어입니다.");
                    break;
                case CommandError.Unsuccessful:
                    await context.Channel.SendMessageAsync("❌ 명령어 실행 도중 알 수 없는 오류가 발생했습니다.");
                    break;
            }
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