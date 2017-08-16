using System;

namespace AutoConverter
{
    internal class ConversionCompletedEventArgs : EventArgs
    {
        private string _inputPath;
        private string _outputPath;

        public string InputPath => _inputPath;
        public string OutputPath => _outputPath;

        public ConversionCompletedEventArgs(string inputPath, string outputPath)
        {
            _inputPath = inputPath;
            _outputPath = outputPath;
        }
    }
}