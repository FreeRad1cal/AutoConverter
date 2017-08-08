using System.IO;
using System.Reflection;
using Xunit.Sdk;

namespace DirectoryWatcher.UnitTests
{
    public class UseMkvFileAttribute: BeforeAfterTestAttribute
    {
        private readonly int _size;
        private readonly string _fileName;

        public UseMkvFileAttribute(int size, string fileName)
        {
            _size = size;
            _fileName = fileName;
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            using (var file = File.Create($"/{_fileName}"))
            {
                file.Write(new byte[_size], 0, _size);
            }
            base.Before(methodUnderTest);
        }

        public override void After(MethodInfo methodUnderTest)
        {
            File.Delete($"/{_fileName}");
        }
    }
}