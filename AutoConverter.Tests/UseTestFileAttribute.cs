using DirectoryWatcher.UnitTests;
using System;
using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace AutoConverter.Tests
{
    public class UseTestFileAttribute: BeforeAfterTestAttribute
    {
        private IDisposable _fileSource;
        private readonly int _size;
        private readonly string _path;

        public UseTestFileAttribute(string fileName, int size)
        {
            _size = size;
            _path = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            _fileSource = new TestFileSource(_path, _size);
            base.Before(methodUnderTest);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            _fileSource.Dispose();
        }
    }
}