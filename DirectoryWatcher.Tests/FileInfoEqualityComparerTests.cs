using Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DirectoryWatcher.UnitTests
{
    public class FileInfoEqualityComparerTests
    {
        [Fact]
        void FileInfoEqualityComparerIsIEqualityComparer()
        {
            var comparer = FileInfoEqualityComparer.Instance;
            Assert.IsAssignableFrom<IEqualityComparer<FileInfo>>(comparer);
        }

        [Fact]
        void FileInfoEqualityComparerIsSingleton()
        {
            var result = Enumerable
                .Range(0, 2000)
                .Select(async i =>
                {
                    var instance1 = await Task.Run(() => FileInfoEqualityComparer.Instance);
                    var instance2 = await Task.Run(() => FileInfoEqualityComparer.Instance);
                    return ReferenceEquals(instance1, instance2);
                })
                .Select(task => task.Result)
                .Aggregate((first, second) => first && second);

            Assert.True(result);
        }

        [Theory]
        [InlineData("file1", "file2", 200, 200)]
        [InlineData("file1", "file2", 200, 300)]
        void EqualsReturnsFalseForTwoDifferentFiles(string fileName1, string fileName2, int size1, int size2)
        {
            var comparer = FileInfoEqualityComparer.Instance;
            using (new TestFileSource(fileName1, size1))
            {
                using (new TestFileSource(fileName2, size2))
                {
                    var path1 = Path.Combine(Directory.GetCurrentDirectory(), fileName1);
                    var path2 = Path.Combine(Directory.GetCurrentDirectory(), fileName2);
                    Assert.False(comparer.Equals(new FileInfo(path1), new FileInfo(path2)));
                }
            }
        }

        [Fact]
        void EqualsReturnsTrueForTheSameFile()
        {
            var comparer = FileInfoEqualityComparer.Instance;
            using (new TestFileSource("file", 200))
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "file");
                Assert.True(comparer.Equals(new FileInfo(path), new FileInfo(path)));
            }
        }
    }
}