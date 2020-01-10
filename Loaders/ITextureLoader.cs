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
        // Called from main thread
        Task<Texture2D> CreateTexture(string relativePath, TextureTarget target);

        // Called from main thread
        Task<Texture2D> CreateTexture(ArraySegment<byte> imageData, string mimeType, TextureTarget target);
    }
}