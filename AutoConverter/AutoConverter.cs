using System;
using System.IO;
using System.Threading.Tasks;
using Common.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.FileProviders;
using DirectoryWatcher;
using System.Threading;

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
                var extensions = new[] { ".mkv", ".mpg" };
                var command = new InvokeHandbrakeCommand(extensions, 500);
                var watcher = new CommandExecutingDirectoryWatcher(dirPath, command);
                await watcher.Watch().ConfigureAwait(true);

                return 0;
            });

            return app.Execute(args);
        }
    }
}