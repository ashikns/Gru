using System.IO;
using System.Threading.Tasks;

namespace Gru.FileLoaders
{
    public class FileStreamLoader : IFileLoader
    {
        private string _modelDirectory;

        public FileStreamLoader(string modelDirectory)
        {
            _modelDirectory = modelDirectory;
        }

        public Task<Stream> OpenFile(string relativePath)
        {
            return Task.FromResult((Stream)File.OpenRead(Path.Combine(_modelDirectory, relativePath)));
        }
    }
}