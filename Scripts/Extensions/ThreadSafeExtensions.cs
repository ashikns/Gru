using System;
using System.Threading;

namespace Gru.Extensions
{
    public static class ThreadSafeExtensions
    {
        public static T ThreadSafeGetOrAdd<T>(this Lazy<T>[] array, int index, Func<T> factory) where T : class
        {
            var result = array[index];
            if (result != null)
            {
                return result.Value;
            }

            result = new Lazy<T>(() => factory());
            var existing = Interlocked.CompareExchange(ref array[index], result, null);

            return existing == null ? result.Value : existing.Value;
        }
    }
}