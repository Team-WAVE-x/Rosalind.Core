using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Rosalind.Core.Models;
using Victoria;

namespace Rosalind.Core.Services;

/// <summary>
/// 이 서비스는 프로그램을 구성하고 DI를 수행합니다.
/// </summary>
public class DiscordService
{
    private readonly ILog _log;
    private readonly Setting _setting;
    private readonly ServiceProvider _service;
    private readonly LavalinkSetting _lavalink;
    private readonly DiscordSocketClient _client;

    /// <summary>
    /// 이 서비스의 생성자입니다. 설정을 불러와 클라이언트를 시작합니다.
    /// </summary>
    /// <param name="config">설정 파일의 정보를 담고 있습니다.</param>
    /// <param name="lavalink">라바링크 설정 파일의 정보를 담고 있습니다.</param>
    public DiscordService(FileSystemInfo config, FileSystemInfo lavalink)
    {
        _log = LogManager.GetLogger("RollingActivityLog");

        string jsonString = File.ReadAllText(Path.GetFullPath(lavalink.FullName));
        _lavalink = JsonConvert.DeserializeObject<LavalinkSetting>(jsonString);
        
        jsonString = File.ReadAllText(Path.GetFullPath(config.FullName));
        _setting = JsonConvert.DeserializeObject<Setting>(jsonString);

        _service = ConfigureServices();
        _setting = _service.GetRequiredService<Setting>();
        _client = _service.GetRequiredService<DiscordSocketClient>();
    }

    /// <summary>
    /// 클라이언트가 구성되면 시작되는 메서드입니다.
    /// </summary>
    public async Task MainAsync()
    {
        _client.Log += OnLogReceived;
        _client.Ready += OnReadyAsync;
        _service.GetRequiredService<CommandService>().Log += OnLogReceived;

        if (_setting != null)
        {
            await _client.LoginAsync(TokenType.Bot, _setting.Config.Token);
            await _client.StartAsync();
            await _client.SetGameAsync($"{_setting.Config.Prefix}도움말", type: ActivityType.Listening);
        }

        await _service.GetRequiredService<CommandHandlingService>().InitializeAsync();
        await Task.Delay(Timeout.Infinite).ConfigureAwait(false);
    }

    private Task OnReadyAsync()
    {
        var lavaNode = _service.GetRequiredService<LavaNode>();
        return !lavaNode.IsConnected ? lavaNode.ConnectAsync() : Task.CompletedTask;
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

    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection()
            .AddSingleton<CommandHandlingService>()
            .AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Debug }))
            .AddSingleton<ComponentService>()
            .AddSingleton(_ => new CommandService(new CommandServiceConfig { DefaultRunMode = RunMode.Async, LogLevel = LogSeverity.Debug }))
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