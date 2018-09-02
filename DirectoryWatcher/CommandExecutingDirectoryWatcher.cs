using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Common;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace DirectoryWatcher
{
    public class CommandExecutingDirectoryWatcher : IDirectoryWatcher
    {
        private readonly IEnumerable<string> _paths;
        private readonly ICommand _command;
        private readonly CancellationTokenSource _cts;
        private readonly ICommandProcessor _processor;
        private readonly object _syncRoot = new object();

        public CommandExecutingDirectoryWatcher(IEnumerable<string> paths, ICommand command)
        {
            _paths = paths;
            _command = command;
            _cts = new CancellationTokenSource();
            _processor = new AsyncCommandProcessor(_command, _cts.Token);
        }

        public async Task Cancel()
        {
            lock (_syncRoot)
            {
                _cts.Cancel();
            }
            await _processor.Completion;
        }

        public async Task Watch(int pollingFrequency)
        {
            var ct = _cts.Token;

            var tasks = _paths
                .Select(path => WatchDirectory(path, ct, pollingFrequency));

            AppDomain.CurrentDomain.ProcessExit += (sender, args) => _cts.Cancel();
            await Task.WhenAny(tasks);
        }

        private IEnumerable<FileInfo> GetCurrentContents(string path)
        {
            return Directory
                .EnumerateFiles(path)
                .Select(file => new FileInfo(file))
                .ToArray();

        }

        private async Task WatchDirectory(string path, CancellationToken ct, int pollingFrequency)
        {
            await Task.Run(async () =>
            {
                var content = GetCurrentContents(path);
                while (true)
                {
                    lock (_syncRoot)
                    {
                        ct.ThrowIfCancellationRequested();
                        var currentContents = GetCurrentContents(path);
                        foreach (var file in currentContents.Except(content, FileInfoEqualityComparer.Instance))
                        {
                            if (_command.CanExecute(file))
                            {
                                _processor.Process(file);
                            }
                        }

                        content = currentContents;
                    }

                    await Task.Delay(pollingFrequency, ct);
                }
            }, ct);
        }
    }
}
