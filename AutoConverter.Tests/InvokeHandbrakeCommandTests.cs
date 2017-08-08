using Common;
using Common.Commands;
using System;
using Moq;
using Xunit;
using System.IO;
using System.Linq.Expressions;
using DirectoryWatcher.UnitTests;

namespace AutoConverter.Tests
{
    public class InvokeHandbrakeCommandTests
    {
        [Fact]
        public void InvokeHandbrakeCommandIsICommand()
        {
            var sut = new InvokeHandbrakeCommand(new[] { ".mkv", ".mp4" }, 500);
            Assert.IsAssignableFrom<ICommand>(sut);
        }

        [Fact]
        public void ConstructorThrowsOnNullOrEmptyArray()
        {
            Assert.Throws<ArgumentException>(() => new InvokeHandbrakeCommand(new string[] { }, 100));
            Assert.Throws<ArgumentException>(() => new InvokeHandbrakeCommand(null, 100));
        }

        [Theory]
        [InlineData(".mkv")]
        [InlineData(".mp4")]
        public void CanExecuteReturnsTrueForValidExtensions(string ext)
        {
            var sut = new InvokeHandbrakeCommand(new[] { ".mkv", ".mp4" }, 500);
            var filename = Path.Combine(Directory.GetCurrentDirectory(), $"test{ext}");
            using (new TestFileSource(filename, 200))
            {
                Assert.True(sut.CanExecute(new FileInfo(filename)));
            }
        }

        [Theory]
        [InlineData(".doc")]
        [InlineData(".xls")]
        public void CanExecuteReturnsFalseForInvalidExtensions(string ext)
        {
            var sut = new InvokeHandbrakeCommand(new[] { ".mkv", ".mp4" }, 500);
            var filename = Path.Combine(Directory.GetCurrentDirectory(), $"test{ext}");
            using (new TestFileSource(filename, 200))
            {
                Assert.False(sut.CanExecute(new FileInfo(filename)));
            }
        }

        [Theory]
        [InlineData(200)]
        [InlineData(1000)]
        [InlineData(500)]
        public void CanExecuteReturnsFalseForSizeAboveCutoff(int size)
        {
            var sut = new InvokeHandbrakeCommand(new[] { ".mkv", ".mp4" }, 500);
            var filename = Path.Combine(Directory.GetCurrentDirectory(), "test.mkv");
            using (new TestFileSource(filename, size))
            {
                if (size > 500)
                    Assert.False(sut.CanExecute(new FileInfo(filename)));
                else
                    Assert.True(sut.CanExecute(new FileInfo(filename)));
            }
        }

    }
}
