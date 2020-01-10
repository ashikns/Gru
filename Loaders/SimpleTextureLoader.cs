using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Loaders
{
    public class SimpleTextureLoader : ITextureLoader
    {
        private readonly IFileLoader _fileLoader;

        public SimpleTextureLoader(IFileLoader fileLoader)
        {
            _fileLoader = fileLoader;
        }

        public async Task<Texture2D> CreateTexture(string relativePath, TextureTarget target)
        {
            var isLinear = target == TextureTarget.Normal || target == TextureTarget.Occlusion;
            var imageData = await _fileLoader.ReadContentsAsync(relativePath);
            var texture = new Texture2D(0, 0, TextureFormat.RGBA32, true, isLinear);
            texture.LoadImage(imageData, true);
            return texture;
        }

        public async Task<Texture2D> CreateTexture(ArraySegment<byte> imageData, string mimeType, TextureTarget target)
        {
            byte[] dataCopy = null;
            var isLinear = target == TextureTarget.Normal || target == TextureTarget.Occlusion;

            if (imageData.Offset == 0)
            {
                dataCopy = imageData.Array;
            }
            else
            {
                await Task.Run(() =>
                {
                    dataCopy = new byte[imageData.Count];
                    Buffer.BlockCopy(imageData.Array, imageData.Offset, dataCopy, 0, imageData.Count);
                });
            }

            var texture = new Texture2D(0, 0, TextureFormat.RGBA32, true, isLinear);
            texture.LoadImage(dataCopy, true);
            return texture;
        }
    }
}