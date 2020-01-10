using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Gru.Loaders
{
    public class WebRequestFileLoader : IFileLoader
    {
        public List<float> Progress { get; }

        private readonly Action<IEnumerator> _coroutineStarter;
        private readonly Func<string, Task<string>> _defaultUriRetriever;
        private readonly Func<string, Task<string>> _fallbackUriRetriever;

        public WebRequestFileLoader(
            Action<IEnumerator> coroutineStarter,
            Func<string, Task<string>> defaultUriRetriever,
            Func<string, Task<string>> fallbackUriRetriever)
        {
            Progress = new List<float>();

            _coroutineStarter = coroutineStarter;
            _defaultUriRetriever = defaultUriRetriever;
            _fallbackUriRetriever = fallbackUriRetriever;
        }

        public async Task<byte[]> ReadContentsAsync(string relativePath)
        {
            var defaultUri = await _defaultUriRetriever(relativePath).ConfigureAwait(false);
            var defaultDataT = new TaskCompletionSource<byte[]>();

            // There is a race condition here but it's for progress reporting
            // so not worth putting a lock.
            var pi = Progress.Count;
            Progress.Add(0);

            _coroutineStarter.Invoke(Download(defaultUri, defaultDataT, pi));

            var defaultData = await defaultDataT.Task.ConfigureAwait(false);
            if (defaultData != null) { return defaultData; }

            var fallbackUri = await _fallbackUriRetriever(relativePath).ConfigureAwait(false);
            var fallbackDataT = new TaskCompletionSource<byte[]>();

            _coroutineStarter.Invoke(Download(fallbackUri, fallbackDataT, pi));

            var fallbackData = await fallbackDataT.Task.ConfigureAwait(false);
            if (fallbackData != null) { return fallbackData; }

            throw new Exception("Failed to create buffer: " + relativePath);
        }

        private IEnumerator Download(string uri, TaskCompletionSource<byte[]> resultT, int progressIndex)
        {
            var request = UnityWebRequest.Get(uri);
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
                resultT.SetResult(request.downloadHandler.data);
            }
            else
            {
                resultT.SetResult(null);
            }
        }
    }
}