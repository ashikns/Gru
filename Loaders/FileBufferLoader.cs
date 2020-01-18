using System.IO;
using System.Threading.Tasks;

namespace Gru.Loaders
{
    /// <summary>
    /// Reads buffer from a file located on disk.
    /// </summary>
    public class FileBufferLoader : IBufferLoader
    {
        private string _modelDirectory;

        public FileBufferLoader(string modelDirectory)
        {
            _modelDirectory = modelDirectory;
        }

        public async Task<byte[]> ReadContentsAsync(string relativePath)
        {
            using (var stream = File.OpenRead(Path.Combine(_modelDirectory, relativePath)))
            {
                var buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                return buffer;
            }
        }
    }
}