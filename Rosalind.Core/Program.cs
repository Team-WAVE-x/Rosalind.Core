using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using Rosalind.Core.Services;

namespace Rosalind.Core;

internal static class Program
{
    public static int Main(string[] args)
    {
        var rootCommand = new RootCommand
        {
            new Option<FileInfo>(
                "--config",
                "Path of config file."),
            new Option<FileInfo>(
                "--lavalink",
                "Path of Lavalink config file.")
        };

        rootCommand.Description = "Rosalind.Core";

        rootCommand.Handler = CommandHandler.Create<FileInfo, FileInfo>((config, lavalink) =>
            new DiscordService(config, lavalink).MainAsync().GetAwaiter().GetResult());

        return rootCommand.InvokeAsync(args).Result;
    }
}