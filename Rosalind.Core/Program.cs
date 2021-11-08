using FluentArgs;
using Rosalind.Core.Services;

namespace Rosalind.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            FluentArgsBuilder.New()
                .RegisterHelpFlag("-h", "--help")
                .Parameter("-c", "--config")
                    .WithDescription("Path of Config File")
                    .WithExamples("./Setting.json")
                    .IsOptionalWithDefault($"./Setting.json")
                .Parameter("-l", "--lavalink")
                    .WithDescription("Path of Lavalink Config File")
                    .WithExamples("./Lavalink.json")
                    .IsOptionalWithDefault($"./Lavalink.json")
                .Call(lavalink => config => new DiscordService(lavalink).MainAsync(config).GetAwaiter().GetResult())
                .Parse(args);
        }
    }
}