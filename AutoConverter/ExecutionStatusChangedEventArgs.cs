using System;

namespace AutoConverter
{
    public class ExecutionStatusChangedEventArgs : EventArgs
    {
        private readonly string _path;
        private readonly ExecutionEvent _conversionEvent;

        public string Path => _path;
        public ExecutionEvent ConversionEvent => _conversionEvent;

        public ExecutionStatusChangedEventArgs(string path, ExecutionEvent e)
        {
            _path = path;
            _conversionEvent = e;
        }
    }
}