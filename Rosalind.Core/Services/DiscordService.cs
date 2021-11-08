using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
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
        private LavalinkSetting _lavalink;
        private DiscordSocketClient _client;

        public DiscordService(string lavalinkConfigFilePath)
        {
            _log = LogManager.GetLogger("RollingActivityLog");

            string jsonString = File.ReadAllText(Path.GetFullPath(lavalinkConfigFilePath));
            _lavalink = JsonConvert.DeserializeObject<LavalinkSetting>(jsonString);

            _service = ConfigureServices();
            _setting = _service.GetRequiredService<Setting>();
            _client = _service.GetRequiredService<DiscordSocketClient>();
        }

        public async Task MainAsync(string configFilePath)
        {
            string jsonString = File.ReadAllText(Path.GetFullPath(configFilePath));
            _setting = JsonConvert.DeserializeObject<Setting>(jsonString);

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
                _log.Fatal(log.Message);
            else if (log.Severity == LogSeverity.Error)
                _log.Error(log.Message);
            else if (log.Severity == LogSeverity.Warning)
                _log.Warn(log.Message);
            else if (log.Severity == LogSeverity.Info)
                _log.Info(log.Message);
            else if (log.Severity == LogSeverity.Verbose)
                _log.Info(log.Message);
            else if (log.Severity == LogSeverity.Debug)
                _log.Debug(log.Message);

            return Task.CompletedTask;
        }

        public ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<DiscordSocketClient>(x => new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Debug }))
                .AddSingleton<ComponentService>()
                .AddSingleton<CommandService>(x => new CommandService(new CommandServiceConfig { DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Debug }))
                .AddSingleton<SqlService>()
                .AddSingleton<LavaConfig>()
                .AddSingleton<Setting>();

            foreach (var item in _lavalink.Nodes)
            {
                services.AddLavaNode(x =>
                {
                    x.SelfDeaf = _lavalink.SelfDeaf;
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