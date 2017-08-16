using Common;
using System;
using Moq;
using Xunit;
using System.IO;
using DirectoryWatcher.UnitTests;
using System.Threading;
using System.Threading.Tasks;
using AutoConverter;
using Xunit.Sdk;

namespace AutoConverter.Tests
{
    public class InvokeHandbrakeCommandTests : IClassFixture<CommandFixture>
    {
        public CommandFixture CommandFixture { get; }

        public InvokeHandbrakeCommandTests(CommandFixture commandFixture)
        {
            CommandFixture = commandFixture;
        }

        [Fact]
        public void InvokeHandbrakeCommandIsICommand()
        {
            var sut = CommandFixture.Command;
            Assert.IsAssignableFrom<ICommand>(sut);
        }

        [Fact]
        public void ConstructorThrowsOnNullOrEmptyArray()
        {
            Assert.Throws<ArgumentException>(() => new InvokeHandbrakeCommand(new string[] { }, 100, new Mock<IPathProjection>().Object));
            Assert.Throws<ArgumentException>(() => new InvokeHandbrakeCommand(null, 100, new Mock<IPathProjection>().Object));
        }

        [Theory]
        [UseTestFile("test.mp4", 1000)]
        [UseTestFile("test.mkv", 1000)]
        [InlineData("test.mp4")]
        [InlineData("test.mkv")]
        public void CanExecuteReturnsTrueForValidExtensions(string filename)
        {
            var sut = CommandFixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            Assert.True(sut.CanExecute(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), path))));
        }

        [Fact]
        public void CanExecuteReturnsFalseForFileNamesWithAppendedStringBeforeExtension()
        {
            var sut = CommandFixture.Command;
            Assert.False(sut.CanExecute(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "test__ADMIN__.mp4"))));
        }

        [Theory]
        [InlineData("__ADMIN__test.mp4")]
        [InlineData("te__ADMIN__st.mp4")]
        public void CanExecuteReturnsTrueWhenAppendedStringIsNotLocatedBeforeExtension(string filename)
        {
            var sut = CommandFixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            Assert.False(sut.CanExecute(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), path))));
        }

        [Theory]
        [InlineData(".doc")]
        [InlineData(".xls")]
        [InlineData("")]
        public void CanExecuteReturnsFalseForInvalidExtensions(string ext)
        {
            var sut = CommandFixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"test{ext}");
            Assert.False(sut.CanExecute(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), path))));
        }

        [Theory]
        [UseTestFile("test200.mkv", 200)]
        [UseTestFile("test1000.mkv", 1000)]
        [UseTestFile("test500.mkv", 500)]
        [InlineData(200)]
        [InlineData(1000)]
        [InlineData(500)]
        public void CanExecuteReturnsFalseForSizeBelowCutoff(int size)
        {
            var sut = CommandFixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), $"test{size}.mkv");
            if (size < 500)
            {
                Assert.False(sut.CanExecute(new FileInfo(path)));
            }
            else
            {
                Assert.True(sut.CanExecute(new FileInfo(path)));
            }
        }

        [Fact]
        [UseTestFile("test.mkv", 1000)]
        public async Task ExecuteAsyncDoesNotInvokeProcessWhenCancelledCancellationTokenProvided()
        {
            var sut = CommandFixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "test.mkv");
            var pathCreated = Path.Combine(Directory.GetCurrentDirectory(), "test__ADMIN__.mkv");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            try
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    async () => await sut.ExecuteAsync(new FileInfo(path), cts.Token));
                Assert.False(File.Exists(pathCreated));
            }
            catch (FalseException)
            {
                File.Delete(pathCreated);
                throw;
            }
        }

        [Fact]
        [UseTestFile("test.mkv", 1000)]
        public async Task ExecuteAsyncInvokesProcessCorrectly()
        {
            var sut = CommandFixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "test.mkv");
            var pathCreated = Path.Combine(Directory.GetCurrentDirectory(), "test__ADMIN__.mkv");
            await sut.ExecuteAsync(new FileInfo(path), CancellationToken.None);
            Assert.True(File.Exists(pathCreated));

            File.Delete(pathCreated);
        }
    }
}
