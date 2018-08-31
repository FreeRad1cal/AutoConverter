using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Common
{
    public sealed class FileInfoEqualityComparer: EqualityComparer<FileInfo>
    {
        public static FileInfoEqualityComparer Instance { get; } = new FileInfoEqualityComparer();

        private FileInfoEqualityComparer() { }

        public override bool Equals(FileInfo x, FileInfo y)
        {
            var xProps = GetPropertyValues(x);
            var yProps = GetPropertyValues(y);
            var result = xProps
                .Zip(yProps, (xp, yp) => xp.Equals(yp))
                .Aggregate((first, second) => first && second);

            return result;
        }

        public override int GetHashCode(FileInfo obj)
        {
            var props = GetPropertyValues(obj);
            var hashCode = props
                .Select(prop => prop.GetHashCode())
                .Aggregate((hash1, hash2) => hash1 ^ hash2);
            return hashCode;
        }

        private object[] GetPropertyValues(FileInfo obj)
        {
            return obj
                .GetType()
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Select(prop => prop.GetValue(obj))
                .Where(prop => !(prop is FileSystemInfo))
                .ToArray();
        }
    }
}