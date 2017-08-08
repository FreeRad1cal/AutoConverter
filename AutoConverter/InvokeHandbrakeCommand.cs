using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Commands
{
    public class InvokeHandbrakeCommand : ICommand
    {
        private readonly IEnumerable<string> _extensions;
        private readonly uint _maxMb;

        public InvokeHandbrakeCommand(IEnumerable<string> extensions, uint maxMb)
        {
            if (extensions == null || !extensions.Any())
            {
                throw new ArgumentException("Invalid input provided");
            }

            _extensions = extensions.ToArray();
            _maxMb = maxMb;
        }

        public void Execute(object context)
        {
            var fileInfo = (FileInfo) context;
            
        }

        public bool CanExecute(object context)
        {
            var fileInfo = (FileInfo) context;
            return _extensions.Contains(fileInfo.Extension) && fileInfo.Length < _maxMb;
        }

        public async Task ExecuteAsync(object context)
        {
            await Task.Run(() => Execute(context));
        }

        public async Task ExecuteAsync(object context, CancellationToken ct)
        {
            await Task.Run(() =>
            {
                Execute(context);
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                }
            }, ct);
        }

        public event EventHandler OnExecute;
    }
}
