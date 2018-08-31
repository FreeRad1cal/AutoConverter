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
using System.Linq;
using System.Collections.Generic;

namespace AutoConverter.Tests
{
    public class InvokeHandbrakeCommandTests : IClassFixture<AutoConverterTestFixture>
    {
        public AutoConverterTestFixture Fixture { get; }

        public InvokeHandbrakeCommandTests(AutoConverterTestFixture fixture)
        {
            Fixture = fixture;
        }

        [Fact]
        public void InvokeHandbrakeCommandIsICommand()
        {
            var sut = Fixture.Command;
            Assert.IsAssignableFrom<ICommand>(sut);
        }

        [Fact]
        public void ConstructorThrowsOnNullOrEmptyArray()
        {
            Assert.Throws<ArgumentException>(() => new InvokeHandbrakeCommand(new string[] { }, 100, "c:/handbrakecli/handbrakecli.exe", 18));
            Assert.Throws<ArgumentException>(() => new InvokeHandbrakeCommand(null, 100, "c:/handbrakecli/handbrakecli.exe", 18));
        }

        [Theory]
        [UseTestFile("test.mp4", 1000)]
        [UseTestFile("test.mkv", 1000)]
        [InlineData("test.mp4")]
        [InlineData("test.mkv")]
        public void CanExecuteReturnsTrueForValidExtensions(string filename)
        {
            var sut = Fixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            Assert.True(sut.CanExecute(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), path))));
        }

        [Fact]
        public void CanExecuteReturnsFalseForFileNamesWithAppendedStringBeforeExtension()
        {
            var sut = Fixture.Command;
            Assert.False(sut.CanExecute(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), "test__CONVERTED__.mp4"))));
        }

        [Theory]
        [InlineData("__CONVERTED__test.mp4")]
        [InlineData("te__CONVERTED__st.mp4")]
        public void CanExecuteReturnsTrueWhenAppendedStringIsNotLocatedBeforeExtension(string filename)
        {
            var sut = Fixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            Assert.False(sut.CanExecute(new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), path))));
        }

        [Theory]
        [InlineData(".doc")]
        [InlineData(".xls")]
        [InlineData("")]
        public void CanExecuteReturnsFalseForInvalidExtensions(string ext)
        {
            var sut = Fixture.Command;
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
            var sut = Fixture.Command;
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
            var sut = Fixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "test.mkv");
            var pathCreated = Path.Combine(Directory.GetCurrentDirectory(), "test__CONVERTED__.mkv");
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
            var sut = Fixture.Command;
            var path = Path.Combine(Fixture.Config.WatchedPath, "test.mkv");
            var pathCreated = Path.Combine(Fixture.Config.WatchedPath, "test__CONVERTED__.mkv");
            await sut.ExecuteAsync(new FileInfo(path), CancellationToken.None);
            Assert.True(File.Exists(pathCreated));

            File.Delete(pathCreated);
        }

        [Fact]
        [UseTestFile("test.mkv", 1000)]
        public async Task ExecutionStatusChangedEventTriggeredWhenProcessStarted()
        {
            var sut = Fixture.Command;
            var path = Path.Combine(Fixture.Config.WatchedPath, "test.mkv");
            bool eventTriggered = false;
            sut.ExecutionStatusChanged += (obj, args) =>
            {
                var result = (args as ExecutionStatusChangedEventArgs).ConversionEvent;
                if (result == ExecutionEvent.Started)
                {
                    eventTriggered = true;
                }
            };

            foreach (var i in Enumerable.Range(0, 5))
            {
                eventTriggered = false;
                var task = sut.ExecuteAsync(new FileInfo(path), CancellationToken.None);
                await Task.Delay(200);
                Assert.True(eventTriggered);
                Assert.False(task.IsCanceled || task.IsFaulted);
            }
        }

        [Fact]
        [UseTestFile("test.mkv", 1000)]
        public async Task ExecutionStatusChangedEventTriggeredWhenProcessCancelled()
        {
            var sut = Fixture.Command;
            var path = Path.Combine(Directory.GetCurrentDirectory(), "test.mkv");
            
            foreach (var i in Enumerable.Range(0, 50))
            {
                var calls = new List<ExecutionEvent>();
                sut.ExecutionStatusChanged += (obj, args) =>
                {
                    calls.Add((args as ExecutionStatusChangedEventArgs).ConversionEvent);
                };
                var tcs = new CancellationTokenSource();
                var task = sut.ExecuteAsync(new FileInfo(path), tcs.Token);
                await Task.Delay(500).ContinueWith(t => tcs.Cancel());
                Assert.True(calls.Count == 2, $"the number of calls is {calls.Count}, not 2");
                Assert.True(calls[0] == ExecutionEvent.Started, $"call 0 is {calls[0]}, not 'started");
                Assert.True(calls[1] == ExecutionEvent.Cancelled, $"call 1 is {calls[1]}, not 'cancelled");
                Assert.True(task.IsCanceled, "the task is not cancelled");
            }
        }

        [Fact]
        [UseTestFile("test.mkv", 1000)]
        public async Task ExecutionStatusChangedEventTriggeredWhenProcessCompletes()
        {
            var sut = Fixture.Command;
            var path = Path.Combine(Fixture.Config.WatchedPath, "test.mkv");
            var pathCreated = Path.Combine(Fixture.Config.WatchedPath, "test__CONVERTED__.mkv");

            foreach (var i in Enumerable.Range(0, 5))
            {
                var calls = new List<ExecutionEvent>();
                sut.ExecutionStatusChanged += (obj, args) =>
                {
                    calls.Add((args as ExecutionStatusChangedEventArgs).ConversionEvent);
                };
                var task = sut.ExecuteAsync(new FileInfo(path), CancellationToken.None);
                await task;
                Assert.True(calls.Count == 2, $"the number of calls is {calls.Count}, not 2");
                Assert.True(calls[0] == ExecutionEvent.Started, $"call 0 is {calls[0]}, not 'started");
                Assert.True(calls[1] == ExecutionEvent.Completed, $"call 1 is {calls[1]}, not 'completed");
                Assert.True(task.IsCompletedSuccessfully, "the task is not completed successfully");
                File.Delete(pathCreated);
            }
        }
    }
}
