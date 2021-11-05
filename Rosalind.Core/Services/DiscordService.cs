using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Victoria;

namespace Rosalind.Core.Services
{
    public class DiscordService
    {
        private ILog _log;
        private Setting _setting;
        private ServiceProvider _service;
        private DiscordSocketClient _client;

        private readonly string configFilePath;

        public DiscordService(string configFilePath)
        {
            this.configFilePath = configFilePath;

            _log = LogManager.GetLogger("RollingActivityLog");
            _service = ConfigureServices();
            _setting = _service.GetRequiredService<Setting>();
            _client = _service.GetRequiredService<DiscordSocketClient>();
        }

        public async Task MainAsync()
        {
            _setting.GetConfig(Path.GetFullPath(configFilePath));
            _client.Log += OnLogReceived;
            _service.GetRequiredService<CommandService>().Log += OnLogReceived;

            await _client.LoginAsync(TokenType.Bot, _setting.Config.Token);
            await _client.StartAsync();
            await _client.SetGameAsync($"{_setting.Config.Prefix}도움말", type: ActivityType.Listening);
            await _service.GetRequiredService<CommandHandlingService>().InitializeAsync();
            await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
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

        public ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<DiscordSocketClient>(x => ActivatorUtilities.CreateInstance<DiscordSocketClient>(x, new DiscordSocketConfig { LogLevel = LogSeverity.Debug }))
                .AddSingleton<ComponentService>()
                .AddSingleton<CommandService>(x => ActivatorUtilities.CreateInstance<CommandService>(x, new CommandServiceConfig { DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Debug }))
                .AddSingleton<SqlService>()
                .AddSingleton<LavaConfig>()
                .AddSingleton<LavaNode>()
                .AddSingleton<Setting>()
                .AddLavaNode(x => {
                    x.SelfDeaf = true;
                    x.Hostname = "127.0.0.1";
                    x.Port = 8080;
                })
                .BuildServiceProvider();
        }
    }
}