using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public interface ICommand
    {
        void Execute(object context);
        Task ExecuteAsync(object context);
        Task ExecuteAsync(object context, CancellationToken ct);
        bool CanExecute(object context);
        event EventHandler SomethingHappened;
    }
}
