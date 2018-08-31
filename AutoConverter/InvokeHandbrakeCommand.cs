using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AutoConverter
{
    public class InvokeHandbrakeCommand : ICommand
    {
        private readonly IEnumerable<string> _extensions;
        private readonly uint _minBytes;
        private readonly string _handbrakeCliPath;
        private readonly IPathProjection _pathProjection;

        public InvokeHandbrakeCommand(IEnumerable<string> extensions, uint minBytes, string handbrakeCliPath)
        {
            if (extensions == null || !extensions.Any())
            {
                throw new ArgumentException("Invalid input provided");
            }

            _extensions = extensions.ToArray();
            _minBytes = minBytes;
            _handbrakeCliPath = handbrakeCliPath;
            _pathProjection = new FilenameAppendPathProjection("__CONVERTED__");
        }

        public void Execute(object context)
        {
            ExecuteAsync(context).Wait();
        }

        public bool CanExecute(object context)
        {
            var fileInfo = (FileInfo)context;
            return _extensions.Contains(fileInfo.Extension) && fileInfo.Exists && !Regex.IsMatch(fileInfo.Name, @"^.+__CONVERTED__\.\w+$") && fileInfo.Length >= _minBytes;
        }

        public async Task ExecuteAsync(object context)
        {
            await ExecuteAsync(context, CancellationToken.None);
        }

        public async Task ExecuteAsync(object context, CancellationToken ct)
        {
            if (!CanExecute(context))
            {
                throw new ArgumentException("Input file missing or cannot be converted");
            }

            var fileInfo = (FileInfo)context;
            var startInfo = new ProcessStartInfo(_handbrakeCliPath,
                $"-i \"{fileInfo.FullName}\" -o \"{_pathProjection.GetPath(fileInfo.FullName)}\"")
            {
                CreateNoWindow = true,
                UseShellExecute = true
            };

            if (ct.IsCancellationRequested)
            {
                OnExecutionStatusChanged(fileInfo.FullName, ExecutionEvent.Cancelled);
                await Task.FromCanceled(ct);
            }

            var process = Process.Start(startInfo);
            process.EnableRaisingEvents = true;
            OnExecutionStatusChanged(fileInfo.FullName, ExecutionEvent.Started);
            var tcs = new TaskCompletionSource<bool>();
            ct.Register(() =>
            {
                if (tcs.TrySetCanceled(ct))
                {
                    process.Kill();
                    process.WaitForExit();
                    OnExecutionStatusChanged(fileInfo.FullName, ExecutionEvent.Cancelled);
                }
            });
            process.Exited += (obj, args) =>
            {
                if (process.ExitCode != 0 && tcs.TrySetCanceled())
                {
                    OnExecutionStatusChanged(fileInfo.FullName, ExecutionEvent.Cancelled);
                }

                if (tcs.TrySetResult(true))
                {
                    OnExecutionStatusChanged(fileInfo.FullName, ExecutionEvent.Completed);
                }
            };
            await tcs.Task;
        }

        protected virtual void OnExecutionStatusChanged(string path, ExecutionEvent e)
        {
            ExecutionStatusChanged?.Invoke(this, new ExecutionStatusChangedEventArgs(path, e));
        }

        public event EventHandler ExecutionStatusChanged;
    }
}
