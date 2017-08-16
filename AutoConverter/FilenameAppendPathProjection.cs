using Common;
using System.IO;

namespace AutoConverter
{
    public class FilenameAppendPathProjection : IPathProjection
    {
        private readonly string _toAppend;

        public string AppendedString => _toAppend;

        public FilenameAppendPathProjection(string toAppend)
        {
            _toAppend = toAppend;
        }

        public string GetPath(string path) => Path.IsPathRooted(path)
            ? Path.Combine(Path.GetDirectoryName(path),
                $"{Path.GetFileNameWithoutExtension(path)}{_toAppend}{Path.GetExtension(path)}")
            : $"{Path.GetFileNameWithoutExtension(path)}{_toAppend}{Path.GetExtension(path)}";
    }
}