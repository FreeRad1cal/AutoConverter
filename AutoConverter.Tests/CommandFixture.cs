using AutoConverter;
using Common;

namespace AutoConverter.Tests
{
    public class CommandFixture
    {
        private readonly InvokeHandbrakeCommand _command;

        public CommandFixture()
        {
            _command = new InvokeHandbrakeCommand(new[] {".mp4", ".mkv"}, 500,
                new FilenameAppendPathProjection("__ADMIN__"));
        }

        public ICommand Command => _command;
    }
}