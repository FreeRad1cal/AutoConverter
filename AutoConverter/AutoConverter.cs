using Microsoft.Extensions.CommandLineUtils;
using DirectoryWatcher;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace AutoConverter
{
    public static class AutoConverter
    {
        private static ILogger ConsoleLogger { get; } = new LoggerFactory()
            .AddConsole()
            .CreateLogger(nameof(AutoConverter));

        public static async Task Main(string[] args)
        {
            var config = GetConfiguration(args);

            var command = new InvokeHandbrakeCommand(config);
            command.ExecutionStatusChanged += ExecutionStatusChangedCallback;

            var watcher = new CommandExecutingDirectoryWatcher(config.WatchedPaths, command);

            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Watching [");
            foreach (var path in config.WatchedPaths)
            {
                stringBuilder.Append(path);
                stringBuilder.Append(", ");
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.Append(']');
            ConsoleLogger.LogInformation(stringBuilder.ToString());

            await watcher.Watch(config.PollingFrequency).ConfigureAwait(true);
        }

        private static void ExecutionStatusChangedCallback(object obj, EventArgs args)
        {
            var executionStatusChangedEventArgs = (ExecutionStatusChangedEventArgs) args;

            switch (executionStatusChangedEventArgs.ConversionEvent)
            {
                case ExecutionEvent.Started:
                    ConsoleLogger.LogInformation($"Converting {executionStatusChangedEventArgs.Path}...");
                    break;
                case ExecutionEvent.Completed:
                    ConsoleLogger.LogInformation($"Conversion of {executionStatusChangedEventArgs.Path} completed");
                    break;
                case ExecutionEvent.Cancelled:
                    ConsoleLogger.LogInformation($"Conversion of {executionStatusChangedEventArgs.Path} cancelled");
                    break;
            }
        }

        public static AutoConverterConfig GetConfiguration(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config.json"))
                .AddCommandLine(args)
                .Build();

            var ret = new AutoConverterConfig();
            config.Bind(ret);
            return ret;
        }
    }
}