using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Loaders
{
    public enum TextureTarget
    {
        Diffuse,
        Normal, //linear
        Occlusion, //linear
        Emission,
        Metal,
        Specular
    }

    public interface ITextureLoader
    {
        /// <summary>
        /// Creates a texture from the given image.
        /// This will be called from main thread by Gru.
        /// </summary>
        /// <param name="relativePath">Path to the image to read.
        /// Path is relative to folder which contains the .gltf/.glb file.</param>
        /// <param name="target">Specifies what the texture will be used for.</param>
        /// <returns>Created texture</returns>
        Task<Texture2D> CreateTexture(string relativePath, TextureTarget target);

        /// <summary>
        /// Creates a texture from the given image data.
        /// This is called if the image is embedded into gltf as a buffer or data uri.
        /// Will be called from main thread by Gru.
        /// </summary>
        /// <param name="imageData">Byte data which make up the image</param>
        /// <param name="mimeType">Type of image, will be either png or jpg.</param>
        /// <param name="target">Specifies what the texture will be used for.</param>
        /// <returns>Created Texture</returns>
        Task<Texture2D> CreateTexture(ArraySegment<byte> imageData, string mimeType, TextureTarget target);
    }
}