using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// Joints and matrices defining a skin.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/skin.schema.json"/>
    /// </summary>
    public class Skin : GLTFChildOfRootProperty
    {
        [Preserve]
        public Skin() { }

        /// <summary>
        /// The index of the accessor containing the floating-point 4x4 inverse-bind matrices.
        /// The default is that each matrix is a 4x4 identity matrix, which implies that inverse-bind matrices were pre-applied.
        /// </summary>
        [JsonProperty(PropertyName = "inverseBindMatrices")]
        public GLTFId InverseBindMatrices { get; set; }

        /// <summary>
        /// The index of the node used as a skeleton root.
        /// The node must be the closest common root of the joints hierarchy or
        /// a direct or indirect parent node of the closest common root.
        /// </summary>
        [JsonProperty(PropertyName = "skeleton")]
        public GLTFId Skeleton { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// Indices of skeleton nodes, used as joints in this skin.
        /// The array length must be the same as the `count` property of the `inverseBindMatrices` accessor (when defined).
        /// </summary>
        [JsonProperty(PropertyName = "joints", Required = Required.Always)]
        public GLTFId[] Joints
        {
            get => joints;
            set
            {
                if (value == null || value.Length < 1 || value.Distinct().Count() != value.Length)
                {
                    throw new Exception($"{nameof(Joints)} must be non-null, contain unique elements and be of length at least 1.");
                }
                else
                {
                    joints = value;
                }
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays


        private GLTFId[] joints;

        public override bool Validate()
        {
            return Joints != null;
        }
    }
}