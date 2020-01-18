using System.Threading.Tasks;

namespace Gru.Loaders
{
    public interface IBufferLoader
    {
        /// <summary>
        /// Reads the data from file as bytes.
        /// </summary>
        /// <param name="relativePath">Path to the file to be opened.
        /// Path is relative to folder which contains the .gltf/.glb file.</param>
        /// <returns>Contents of the file.</returns>
        Task<byte[]> ReadContentsAsync(string relativePath);
    }
}