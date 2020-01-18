using System.Threading.Tasks;

namespace Gru.Loaders
{
    public interface IBufferLoader
    {
        /// <summary>
        /// Reads a buffer as bytes.
        /// </summary>
        /// <param name="relativePath">Path to the buffer to read.
        /// Path is relative to folder which contains the .gltf/.glb file.</param>
        /// <returns>Contents of the file.</returns>
        Task<byte[]> ReadContentsAsync(string relativePath);
    }
}