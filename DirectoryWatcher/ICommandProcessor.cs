using Common;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryWatcher
{
    public interface ICommandProcessor
    {
        Task Completion { get; }
        void Process(object obj);
    }
}