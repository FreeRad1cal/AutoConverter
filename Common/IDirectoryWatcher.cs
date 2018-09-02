using System.Threading.Tasks;

namespace Common
{
    public interface IDirectoryWatcher
    {
        Task Watch(int pollingFrequency);
        Task Cancel();
    }
}
