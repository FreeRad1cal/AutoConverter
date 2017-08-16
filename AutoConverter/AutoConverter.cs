using Microsoft.Extensions.CommandLineUtils;
using DirectoryWatcher;
using System;

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
                if (dirPathOption.Value() == null)
                {
                    app.ShowHelp();
                    return 1;
                }

                var dirPath = dirPathOption.Value();
                var extensions = new[] { ".mkv", ".mpg" };
                var command = new InvokeHandbrakeCommand(extensions, 500, new FilenameAppendPathProjection("__CONVERTED__"));
                command.SomethingHappened += SomethingHappenedCallback;
                var watcher = new CommandExecutingDirectoryWatcher(dirPath, command);
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

        private static void SomethingHappenedCallback(object obj, EventArgs args)
        {
            switch (args)
            {
                case ConversionStartedEventArgs val:
                    Console.WriteLine($"Converting {val.Path}...");
                    break;
                case ConversionCompletedEventArgs val:
                    Console.WriteLine("Conversion completed.");
                    break;
                default:
                    break;
            }
        }
    }
}