using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Loaders
{
    public interface ITextureLoader
    {
        // Called from main thread
        Task<Texture2D> CreateTexture(string relativePath, bool isLinear);

        // Called from main thread
        Task<Texture2D> CreateTexture(ArraySegment<byte> imageData, string mimeType, bool isLinear);
    }
}