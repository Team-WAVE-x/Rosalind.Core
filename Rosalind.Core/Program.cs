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
                .Call(config => new DiscordService(config).MainAsync().GetAwaiter().GetResult())
                .Parse(args);
        }
    }
}