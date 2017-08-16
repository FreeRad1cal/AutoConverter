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
        private readonly uint _maxMb;
        private readonly IPathProjection _pathProjection;

        public InvokeHandbrakeCommand(IEnumerable<string> extensions, uint maxMb, IPathProjection pathProjection)
        {
            if (extensions == null || !extensions.Any())
            {
                throw new ArgumentException("Invalid input provided");
            }

            _extensions = extensions.ToArray();
            _maxMb = maxMb;
            _pathProjection = pathProjection;
        }

        public void Execute(object context)
        {
            ExecuteAsync(context).Wait();
        }

        public bool CanExecute(object context)
        {
            var fileInfo = (FileInfo)context;
            return _extensions.Contains(fileInfo.Extension) && Regex.IsMatch(fileInfo.Name, $@"^[\w\s]+{_pathProjection}.\w+$") && fileInfo.Exists && fileInfo.Length >= _maxMb;
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
            var startInfo = new ProcessStartInfo("HandBrakeCLI.exe",
                $"-i {fileInfo.FullName} -o {_pathProjection.GetPath(fileInfo.FullName)}");
            startInfo.CreateNoWindow = true;

            OnExecuted(fileInfo.FullName);
            using (var process = Process.Start(startInfo))
            {
                var tcs = new TaskCompletionSource<bool>();
                ct.Register(() =>
                {
                    process.Kill();
                    tcs.TrySetCanceled(ct);
                });
                process.Exited += (obj, args) => tcs.TrySetResult(true);
                if (process.HasExited)
                {
                    tcs.TrySetResult(true);
                }
                await tcs.Task;
            }
        }

        protected virtual void OnExecuted(string path)
        {
            SomethingHappened?.Invoke(this, new ConversionStartedEventArgs(path));
        }

        protected virtual void OnCompleted(string inputPath, string outputPath)
        {
            SomethingHappened?.Invoke(this, new ConversionCompletedEventArgs(inputPath, outputPath));
        }

        public event EventHandler SomethingHappened;
    }
}
