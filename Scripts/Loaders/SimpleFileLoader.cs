using System.IO;
using System.Threading.Tasks;

namespace Gru.Loaders
{
    public class SimpleFileLoader : IFileLoader
    {
        private string _modelDirectory;

        public SimpleFileLoader(string modelDirectory)
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