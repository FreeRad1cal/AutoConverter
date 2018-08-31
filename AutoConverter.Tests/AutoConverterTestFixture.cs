using AutoConverter;
using Castle.Core.Configuration;
using Common;

namespace AutoConverter.Tests
{
    public class AutoConverterTestFixture
    {
        private readonly InvokeHandbrakeCommand _command;
        
        public AutoConverterTestFixture()
        {
            Config = AutoConverter.GetConfiguration(new string[] { });
            _command = new InvokeHandbrakeCommand(Config.Extensions, Config.MinKb, Config.HandbrakeCliPath, Config.Quality);
        }

        public AutoConverterConfig Config { get; }
        public ICommand Command => _command;
    }
}