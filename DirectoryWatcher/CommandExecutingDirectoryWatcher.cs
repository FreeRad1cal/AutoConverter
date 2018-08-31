using System;
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
        private readonly string _root;
        private readonly ICommand _command;
        private readonly CancellationTokenSource _cts;
        private readonly ICommandProcessor _processor;
        private readonly object _syncRoot = new object();

        public CommandExecutingDirectoryWatcher(string root, ICommand command)
        {
            if (!Directory.Exists(root))
                throw new ArgumentException($"The directory {root} does not exist", nameof(root));

            _root = root;
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

        public async Task Watch(int pollingFrequency = 1000)
        {
            var ct = _cts.Token;
            var contents = GetCurrentContents();

            await Task.Run(async () =>
            {
                while (true)
                {
                    lock (_syncRoot)
                    {
                        ct.ThrowIfCancellationRequested();
                        var currentContents = GetCurrentContents();
                        foreach (var file in currentContents.Except(contents, FileInfoEqualityComparer.Instance))
                        {
                            if (_command.CanExecute(file))
                            {
                                _processor.Process(file);
                            }
                        }
                        contents = currentContents;
                    }
                    await Task.Delay(pollingFrequency);
                }
            }, ct);
        }

        private IEnumerable<FileInfo> GetCurrentContents()
        {
            return Directory
                .EnumerateFiles(_root)
                .Select(file => new FileInfo(file))
                .ToArray();

        }
    }
}
