using Gru.GLTF.Converters;
using Newtonsoft.Json;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// GLTF Id. Used as an index into the collections on <see cref="GLTFRoot"/>
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/glTFid.schema.json"/>
    /// </summary>
    [JsonConverter(typeof(GLTFIdConverter))]
    public class GLTFId : IEquatable<GLTFId>
    {
        [Preserve]
        [JsonConstructor]
        public GLTFId(int key)
        {
            Key = key;
        }

        /// <summary>
        /// Value of the GLTFId.
        /// </summary>
        public int Key
        {
            get => key;
            private set
            {
                if (value < 0) { throw new Exception($"{nameof(Key)} can't be less than zero"); }
                key = value;
            }
        }

        private int key;

        public bool Equals(GLTFId other)
        {
            if (other is null) { return false; }
            return Key == other.Key;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GLTFId);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public static bool operator ==(GLTFId arg1, GLTFId arg2)
        {
            if (ReferenceEquals(arg1, arg2)) return true;
            return Equals(arg1, arg2);
        }

        public static bool operator !=(GLTFId arg1, GLTFId arg2)
        {
            if (ReferenceEquals(arg1, arg2)) return false;
            return !Equals(arg1, arg2);
        }
    }
}