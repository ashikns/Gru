using System.IO;
using System.Threading.Tasks;

namespace Gru.FileLoaders
{
    public interface IFileLoader
    {
        /// <summary>
        /// Opens the give file for reading. e.g. a FileStream
        /// </summary>
        /// <param name="relativePath">Path to the file to be opened.
        /// Path is relative to folder which contains the .gltf/.glb file.</param>
        /// <returns>Stream which can read the file contents.</returns>
        Task<Stream> OpenFile(string relativePath);
    }
}