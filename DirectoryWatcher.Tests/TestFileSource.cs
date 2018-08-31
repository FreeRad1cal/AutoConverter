using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DirectoryWatcher.UnitTests
{
    public class TestFileSource: IDisposable
    {
        private readonly string _path;

        public TestFileSource(string path, int size)
        {
            _path = path;

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Seek(size - 1, SeekOrigin.Begin);
                    writer.Write((byte)0);
                }
            }
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                File.Delete(_path);
                disposedValue = true;
            }
        }

         ~TestFileSource()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
