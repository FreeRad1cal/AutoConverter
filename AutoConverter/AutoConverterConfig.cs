using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutoConverter
{
    public class AutoConverterConfig
    {
        public string WatchedPath { get; set; }

        public string HandbrakeCliPath { get; set; }

        public IEnumerable<string> Extensions { get; set; }

        public int MinKb { get; set; }

        public int Quality { get; set; }
    }
}
