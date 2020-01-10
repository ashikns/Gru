using Newtonsoft.Json.Converters;
using UnityEngine.Scripting;

namespace Gru.GLTF.Converters
{
    internal static class PreserveConverter
    {
        [Preserve]
        private static void Preserve()
        {
            _ = new StringEnumConverter();
        }
    }
}