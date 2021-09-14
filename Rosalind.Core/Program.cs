using Rosalind.Core.Services;

namespace Rosalind.Core
{
    class Program
    {
        static void Main(string[] args) => new DiscordService().MainAsync().GetAwaiter().GetResult();
    }
}