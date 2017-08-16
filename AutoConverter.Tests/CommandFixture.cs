using AutoConverter;
using Common;

namespace AutoConverter.Tests
{
    public class CommandFixture
    {
        private readonly InvokeHandbrakeCommand _command;

        public CommandFixture()
        {
            _command = new InvokeHandbrakeCommand(new[] {".mp4", ".mkv"}, 500);
        }

        public ICommand Command => _command;
    }
}