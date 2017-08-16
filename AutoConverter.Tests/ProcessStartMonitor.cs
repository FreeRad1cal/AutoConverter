using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoConverter.Tests
{
    public class ProcessStartMonitor
    {
        private readonly string _name;

        public ProcessStartMonitor(string name)
        {
            _name = name;
        }

        public async Task Monitor(Action callback, CancellationToken ct)
        {
            var current = Process.GetProcessesByName(_name).Select(process => process.Id).ToArray();

            await Task.Run(async () =>
            {
                while (true)
                {
                    ct.ThrowIfCancellationRequested();
                    var next = Process.GetProcessesByName(_name).Select(process => process.Id).ToArray();
                    if (next.Length > current.Length || next.Except(current).Any())
                    {
                        current = next;
                        callback?.Invoke();
                    }
                    await Task.Delay(50);
                }
            }, ct);
        }
    }
}