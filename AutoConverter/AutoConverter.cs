using Microsoft.Extensions.CommandLineUtils;
using DirectoryWatcher;
using System;
using System.IO;

namespace AutoConverter
{
    class AutoConverter
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: true);

            app.HelpOption("-?|-h|--help");

            var dirPathOption = app.Option("-p|--path", "The path to the folder to be watched",
                CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                var dirPath = dirPathOption.Value();
                if (dirPath == null)
                {
                    dirPath = Directory.GetCurrentDirectory();
                }

                var extensions = new[] { ".mkv", ".mp4" };
                var command = new InvokeHandbrakeCommand(extensions, 500*1024);
                command.ExecutionStatusChanged += ExecutionStatusChangedCallback;
                var watcher = new CommandExecutingDirectoryWatcher(dirPath, command);
                Console.WriteLine($"Watching {dirPath}...");
                await watcher.Watch().ConfigureAwait(true);

                return 0;
            });

            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException)
            {
                app.ShowHelp();
                return 1;
            }
        }

        private static void ExecutionStatusChangedCallback(object obj, EventArgs args)
        {
            var executionStatusChangedEventArgs = (ExecutionStatusChangedEventArgs) args;

            switch (executionStatusChangedEventArgs.ConversionEvent)
            {
                case ExecutionEvent.Started:
                    Console.WriteLine($"{GetTimestamp()} Converting {executionStatusChangedEventArgs.Path}...");
                    break;
                case ExecutionEvent.Completed:
                    Console.WriteLine($"{GetTimestamp()} Conversion of {executionStatusChangedEventArgs.Path} completed");
                    break;
                case ExecutionEvent.Cancelled:
                    Console.WriteLine($"{GetTimestamp()} Conversion of {executionStatusChangedEventArgs.Path} cancelled");
                    break;
                default:
                    break;
            }
        }

        private static string GetTimestamp()
        {
            return $"[{DateTime.Now.ToShortTimeString()}]";
        }
    }
}