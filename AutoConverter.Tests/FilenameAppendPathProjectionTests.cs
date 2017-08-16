using Common;
using Xunit;

namespace AutoConverter.Tests
{
    public class FilenameAppendPathProjectionTests
    {
        [Fact]
        void ConvertedFilePathProjectionIsIPathProjection()
        {
            var resolver = new FilenameAppendPathProjection("__TEST__");
            Assert.IsAssignableFrom<IPathProjection>(resolver);
        }

        [Fact]
        void GetPathReturnsAbosolutePathForAbsoluteInputPath()
        {
            var resolver = new FilenameAppendPathProjection("__TEST__");
            var path = @"C:\foo\bar\baz.exe";
            Assert.Equal(@"C:\foo\bar\baz__TEST__.exe", resolver.GetPath(path));
        }

        [Fact]
        void GetPathReturnsRelativePathForRelativeInputPath()
        {
            var resolver = new FilenameAppendPathProjection("__TEST__");
            var path = @"baz.exe";
            Assert.Equal(@"baz__TEST__.exe", resolver.GetPath(path));
        }
    }
}