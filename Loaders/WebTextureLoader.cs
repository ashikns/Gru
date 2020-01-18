using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Gru.Loaders
{
    /// <summary>
    /// Creates texture from a http resource.
    /// Note that this can also be used to read files on disk since Unity WebRequest has limited support for file:/// syntax.
    /// </summary>
    public class WebTextureLoader : ITextureLoader
    {
        public List<float> Progress { get; }

        private readonly Action<IEnumerator> _coroutineStarter;
        private readonly Func<string, Task<string>> _defaultUriRetriever;
        private readonly Func<string, Task<string>> _fallbackUriRetriever;
        private readonly IBufferLoader _bufferLoader;

        public WebTextureLoader(
            Action<IEnumerator> coroutineStarter,
            Func<string, Task<string>> defaultUriRetriever,
            Func<string, Task<string>> fallbackUriRetriever,
            IBufferLoader bufferLoader)
        {
            Progress = new List<float>();

            _coroutineStarter = coroutineStarter;
            _defaultUriRetriever = defaultUriRetriever;
            _fallbackUriRetriever = fallbackUriRetriever;
            _bufferLoader = bufferLoader;
        }

        public async Task<Texture2D> CreateTexture(string relativePath, TextureTarget target)
        {
            if (target == TextureTarget.Normal || target == TextureTarget.Occlusion)
            {
                var imageData = await Task.Run(() => _bufferLoader.ReadContentsAsync(relativePath));
                var texture = new Texture2D(0, 0, TextureFormat.RGBA32, true, true);
                if (!texture.LoadImage(imageData, true))
                {
                    throw new Exception("Failed to create texture: " + relativePath);
                }
                return texture;
            }

            var defaultUri = await _defaultUriRetriever(relativePath);
            var defaultDataT = new TaskCompletionSource<Texture2D>();

            // There is a race condition here but it's for progress reporting
            // so not worth putting a lock.
            var pi = Progress.Count;
            Progress.Add(0);

            _coroutineStarter.Invoke(DownloadTexture(defaultUri, defaultDataT, pi));

            var defaultData = await defaultDataT.Task;
            if (defaultData != null) { return defaultData; }

            var fallbackUri = await _fallbackUriRetriever(relativePath);
            var fallbackDataT = new TaskCompletionSource<Texture2D>();

            _coroutineStarter.Invoke(DownloadTexture(fallbackUri, fallbackDataT, pi));

            var fallbackData = await fallbackDataT.Task;
            if (fallbackData != null) { return fallbackData; }

            throw new Exception("Failed to create texture: " + relativePath);
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
            if (!texture.LoadImage(dataCopy, true))
            {
                throw new Exception("Failed to create texture");
            }
            return texture;
        }

        private IEnumerator DownloadTexture(string uri, TaskCompletionSource<Texture2D> resultT, int progressIndex)
        {
            var request = UnityWebRequestTexture.GetTexture(uri);
            request.SendWebRequest();

            yield return null;

            while (!request.isDone)
            {
                Progress[progressIndex] = request.downloadProgress;
                yield return null;
            }

            if (!request.isHttpError)
            {
                while (!request.downloadHandler.isDone)
                {
                    yield return null;
                }

                Progress[progressIndex] = 1;
                resultT.SetResult(DownloadHandlerTexture.GetContent(request));
            }
            else
            {
                resultT.SetResult(null);
            }
        }
    }
}