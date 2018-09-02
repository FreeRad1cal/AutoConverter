using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AutoConverter
{
    public class AutoConverterConfig
    {
        public IEnumerable<string> WatchedPaths { get; set; }

        public string HandbrakeCliPath { get; set; }

        public IEnumerable<string> Extensions { get; set; }

        public int MinKb { get; set; }

        public int Quality { get; set; } = 20;

        public int PollingFrequency { get; set; } = 1000;
    }
}
