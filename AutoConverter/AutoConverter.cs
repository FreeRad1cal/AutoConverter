﻿using Microsoft.Extensions.CommandLineUtils;
using DirectoryWatcher;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace AutoConverter
{
    public class AutoConverter
    {
        public static ILogger ConsoleLogger { get; } = new LoggerFactory()
            .AddConsole()
            .CreateLogger(nameof(AutoConverter));

        public static async Task Main(string[] args)
        {
            var config = GetConfiguration(args);

            var command = new InvokeHandbrakeCommand(config.Extensions, config.MinKb * 1024, config.HandbrakeCliPath, config.Quality);
            command.ExecutionStatusChanged += ExecutionStatusChangedCallback;

            var watcher = new CommandExecutingDirectoryWatcher(config.WatchedPath, command);
            Console.WriteLine($"Watching {config.WatchedPath}...");

            await watcher.Watch().ConfigureAwait(true);

            //var app = new CommandLineApplication(throwOnUnexpectedArg: true);

            //app.HelpOption("-?|-h|--help");

            //var dirPathOption = app.Option("-p|--path", "The path to the folder to be watched",
            //    CommandOptionType.SingleValue);

            //app.OnExecute(async () =>
            //{
            //    var dirPath = dirPathOption.Value();
            //    if (dirPath == null)
            //    {
            //        dirPath = Directory.GetCurrentDirectory();
            //    }

            //    var extensions = new[] { ".mkv", ".mp4" };
            //    var command = new InvokeHandbrakeCommand(extensions, 500*1024);
            //    command.ExecutionStatusChanged += ExecutionStatusChangedCallback;
            //    var watcher = new CommandExecutingDirectoryWatcher(dirPath, command);
            //    Console.WriteLine($"Watching {dirPath}...");
            //    await watcher.Watch().ConfigureAwait(true);

            //    return 0;
            //});

            //try
            //{
            //    return app.Execute(args);
            //}
            //catch (CommandParsingException)
            //{
            //    app.ShowHelp();
            //    return 1;
            //}
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