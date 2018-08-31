using System;
using Common;
using Moq;
using Xunit;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace DirectoryWatcher.UnitTests
{
    public class CommandExecutingDirectoryWatcherTests
    {
        [Fact]
        public void SutImplementsIDirectoryWatcher()
        {
            //Fixture setup
            var commandDummy = new Mock<ICommand>();
            var sut = new CommandExecutingDirectoryWatcher(Directory.GetCurrentDirectory(), commandDummy.Object);
            //Exercise system
            //Verify outcome
            Assert.IsAssignableFrom<IDirectoryWatcher>(sut);
            //Teardown
        }

        [Fact]
        public async Task WatchTaskCancelledOnCancellationRequest()
        {
            //Fixture setup
            var commandStub = new Mock<ICommand>();
            commandStub
                .Setup(command => command.ExecuteAsync(It.IsAny<object>()))
                .Returns(Task.CompletedTask);
            var sut = new CommandExecutingDirectoryWatcher(Directory.GetCurrentDirectory(), commandStub.Object);
            //Exercise system
            var task = sut.Watch();
            await sut.Cancel().ConfigureAwait(true);

            //Verify outcome
            var e = await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await task);
            Assert.True(task.IsCanceled);
            //Teardown
        }

        [Fact]
        public async Task WatchReturnsUncompletedTaskWhenNotCancelled()
        {
            //Fixture setup
            var commandDummy = new Mock<ICommand>();
            var sut = new CommandExecutingDirectoryWatcher(Directory.GetCurrentDirectory(), commandDummy.Object);
            //Exercise system
            var task = sut.Watch();
            await Task.Delay(1000);
            //Verify outcome
            Assert.False(task.IsCanceled);
            //Teardown
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WatchCallsCommandExecuteWhenNewFileAppearsAndOnlyWhenCanExecuteReturnsTrue(bool canExecute)
        {
            //Fixture setup
            var commandStub = new Mock<ICommand>();
            bool executeCalled = false;
            commandStub
                .Setup(command => command.CanExecute(It.IsAny<object>()))
                .Returns(canExecute);
            commandStub
                .Setup(command => command.ExecuteAsync(It.IsAny<object>()))
                .Returns(Task.CompletedTask)
                .Callback(() => executeCalled = true);
            commandStub
                .Setup(command => command.ExecuteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => executeCalled = true);
            commandStub
                .Setup(command => command.Execute(It.IsAny<object>()))
                .Callback(() => executeCalled = true);
            var sut = new CommandExecutingDirectoryWatcher(Directory.GetCurrentDirectory(), commandStub.Object);
            //Exercise system
            var task = sut.Watch();
            using (new TestFileSource(Path.Combine(Directory.GetCurrentDirectory(), "test.mkv"),
                500 * 1024 * 1024))
            {
                await sut.Cancel();
                try
                {
                    await task;
                }
                catch (OperationCanceledException)
                {
                }
            }
            //Verify outcome
            if (canExecute)
                Assert.True(executeCalled);
            else
                Assert.False(executeCalled);
        }

        [Fact]
        public async Task WatchDoesNotCallCanExecuteWhenNoNewFileAppears()
        {
            //Fixture setup
            var commandStub = new Mock<ICommand>();
            bool canExecuteCalled = false;
            commandStub
                .Setup(command => command.CanExecute(It.IsAny<object>()))
                .Callback(() => canExecuteCalled = true);
            var sut = new CommandExecutingDirectoryWatcher(Directory.GetCurrentDirectory(), commandStub.Object);
            //Exercise system
            var task = sut.Watch();
            await Task.Delay(1000);
            await sut.Cancel();
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
            }
            //Verify outcome
            Assert.False(canExecuteCalled);
        }

        [Fact]
        public void ConstructorThrowsWhenInvalidDirectoryProvided()
        {
            var commandDummy = new Mock<ICommand>();
            var rand = new Random();
            Assert.Throws<ArgumentException>(() => new CommandExecutingDirectoryWatcher($"/{rand.Next().ToString()}", commandDummy.Object));
        }

        [Fact]
        public void ConstructorDoesNotThrowWhenValidDirectoryProvided()
        {
            var commandDummy = new Mock<ICommand>();
            new CommandExecutingDirectoryWatcher(Directory.GetCurrentDirectory(), commandDummy.Object);
        }
    }
}
