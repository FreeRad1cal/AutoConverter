using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryWatcher
{
    public interface IDirectoryWatcher
    {
        Task Watch(int pollingFrequency);
        Task Cancel();
    }
}
