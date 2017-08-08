using Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace DirectoryWatcher
{
    internal class AsyncCommandProcessor : ICommandProcessor
    {
        private BufferBlock<object> _head;
        private ActionBlock<object> _tail;

        public Task Completion => _tail.Completion;

        public AsyncCommandProcessor(ICommand command, CancellationToken ct)
        {
            _head = new BufferBlock<object>();
            _tail = new ActionBlock<object>(async obj => await command.ExecuteAsync(obj, ct), new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
            });
            _head.LinkTo(_tail, new DataflowLinkOptions()
            {
                PropagateCompletion = true
            });
            ct.Register(() => _head.Complete());
        }

        public void Process(object obj)
        {
            _head.Post(obj);
        }
    }
}
