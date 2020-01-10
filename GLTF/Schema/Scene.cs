using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// The root nodes of a scene.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/scene.schema.json"/>
    /// </summary>
    public class Scene : GLTFChildOfRootProperty
    {
        [Preserve]
        public Scene() { }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// The indices of each root node.
        /// </summary>
        [JsonProperty(PropertyName = "nodes")]
        public GLTFId[] Nodes
        {
            get => nodes;
            set
            {
                if (value == null)
                {
                    nodes = value;
                }
                else if (value.Length < 1 || value.Distinct().Count() != value.Length)
                {
                    throw new Exception($"{nameof(Nodes)} must contain unique elements and be of length at least 1.");
                }
                else
                {
                    nodes = value;
                }
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays


        private GLTFId[] nodes;
    }
}