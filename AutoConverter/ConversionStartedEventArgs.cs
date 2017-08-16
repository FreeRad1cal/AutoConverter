using System;

namespace AutoConverter
{
    internal class ConversionStartedEventArgs : EventArgs
    {
        private string _path;

        public string Path => _path;

        public ConversionStartedEventArgs(string path)
        {
            _path = path;
        }
    }
}