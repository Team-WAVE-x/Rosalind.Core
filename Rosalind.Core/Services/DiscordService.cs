using Discord;
using Discord.Commands;
using Discord.WebSocket;
using KillersLibrary;
using KillersLibrary.EmbedPages;
using Microsoft.Extensions.DependencyInjection;
using Rosalind.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rosalind.Core.Services
{
    public class DiscordService
    {
        private Setting _setting;
        private ServiceProvider _service;
        private DiscordSocketClient _client;

        public DiscordService()
        {
            _service = ConfigureServices();
            _setting = _service.GetRequiredService<Setting>();
            _client = _service.GetRequiredService<DiscordSocketClient>();
        }

        public async Task MainAsync()
        {
            _setting.GetConfig($"{AppDomain.CurrentDomain.BaseDirectory}\\Setting.json");
            _client.Log += new LoggingService().OnLogReceived;
            _service.GetRequiredService<CommandService>().Log += new LoggingService().OnLogReceived;

            await _client.LoginAsync(TokenType.Bot, _setting.Config.Token);
            await _client.StartAsync();
            await _client.SetGameAsync($"{_setting.Config.Prefix}도움말", null, ActivityType.Listening);
            await _service.GetRequiredService<CommandHandlingService>().InitializeAsync();
            await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
        }

        public ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<MultiButtonsService>()
                .AddSingleton<DiscordSocketClient>(x => ActivatorUtilities.CreateInstance<DiscordSocketClient>(x, new DiscordSocketConfig { LogLevel = LogSeverity.Debug, AlwaysAcknowledgeInteractions = true }))
                .AddSingleton<EmbedPagesService>()
                .AddSingleton<CommandService>(x => ActivatorUtilities.CreateInstance<CommandService>(x, new CommandServiceConfig { DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Debug }))
                .AddSingleton<ReactService>()
                .AddSingleton<SqlService>()
                .AddSingleton<Setting>()
                .BuildServiceProvider();
        }
    }
}
