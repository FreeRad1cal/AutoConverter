using System;

namespace AutoConverter
{
    internal class ExecutionStatusChangedEventArgs : EventArgs
    {
        private readonly string _path;
        private readonly ConversionEvent _conversionEvent;

        public string Path => _path;
        public ConversionEvent ConversionEvent => _conversionEvent;

        public ExecutionStatusChangedEventArgs(string path, ConversionEvent e)
        {
            _path = path;
            _conversionEvent = e;
        }
    }
}