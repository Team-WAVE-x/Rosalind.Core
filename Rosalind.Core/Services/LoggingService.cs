using Discord;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rosalind.Core.Services
{
    public class LoggingService
    {
        public Task OnLogReceived(LogMessage log)
        {
            if (log.Severity == LogSeverity.Critical)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (log.Severity == LogSeverity.Error)
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            else if (log.Severity == LogSeverity.Warning)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (log.Severity == LogSeverity.Info)
                Console.ForegroundColor = ConsoleColor.Gray;
            else if (log.Severity == LogSeverity.Verbose)
                Console.ForegroundColor = ConsoleColor.Gray;
            else if (log.Severity == LogSeverity.Debug)
                Console.ForegroundColor = ConsoleColor.DarkGray;

            Console.WriteLine("{0} {1,-11} {2}", DateTime.Now.ToString("HH:mm:ss"), log.Source, log.Message ?? "Null");

            return Task.CompletedTask;
        }
    }
}
