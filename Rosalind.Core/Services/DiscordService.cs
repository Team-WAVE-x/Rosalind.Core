using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using Rosalind.Core.Modules;
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

        public DiscordService(string configFilePath)
        {
            _log = LogManager.GetLogger("RollingActivityLog");
            _setting = SettingManager.GetSetting(configFilePath);
            _service = ConfigureServices();
            _client = _service.GetRequiredService<DiscordSocketClient>();
        }

        public async Task MainAsync()
        {
            _client.Log += OnLogReceived;
            _client.Ready += OnReadyAsync;
            _service.GetRequiredService<CommandService>().Log += OnLogReceived;

            await _client.LoginAsync(TokenType.Bot, _setting.Config.Token);
            await _client.StartAsync();
            await _client.SetGameAsync($"{_setting.Config.Prefix}도움말", type: ActivityType.Listening);
            await _service.GetRequiredService<CommandHandlingService>().InitializeAsync();
            await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
        }

        private Task OnReadyAsync()
        {
            var lavaNode = _service.GetRequiredService<LavaNode>();

            if (!lavaNode.IsConnected)
            {
                return lavaNode.ConnectAsync();
            }
            else
            {
                return Task.CompletedTask;
            }
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
            var services = new ServiceCollection()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<DiscordSocketClient>(x => ActivatorUtilities.CreateInstance<DiscordSocketClient>(x, new DiscordSocketConfig { LogLevel = LogSeverity.Debug }))
                .AddSingleton<ComponentService>()
                .AddSingleton<CommandService>(x => new CommandService(new CommandServiceConfig { DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Debug }))
                .AddSingleton<SqlService>()
                .AddSingleton<LavaConfig>()
                .AddSingleton<Setting>();

            foreach (var item in _setting.LavalinkConfig.Nodes)
            {
                services.AddLavaNode(x =>
                {
                    x.SelfDeaf = _setting.LavalinkConfig.SelfDeaf;
                    x.Hostname = item.Hostname;
                    x.Port = item.Port;
                    x.Authorization = item.Password;
                });

                _log.Info($"Added Lavanode ({item.Hostname}:{item.Port})");
            }

            return services.BuildServiceProvider();
        }
    }
}