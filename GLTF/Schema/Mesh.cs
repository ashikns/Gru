using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// A set of primitives to be rendered. A node can contain one mesh. A node's transform places the mesh in the scene.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/mesh.schema.json"/>
    /// </summary>
    public class Mesh : GLTFChildOfRootProperty
    {
        [Preserve]
        public Mesh() { }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// An array of primitives, each defining geometry to be rendered with a material.
        /// </summary>
        [JsonProperty(PropertyName = "primitives", Required = Required.Always)]
        public MeshPrimitive[] Primitives
        {
            get => primitives;
            set
            {
                if (value == null || value.Length < 1)
                {
                    throw new Exception($"{nameof(Primitives)} must be non null and of length at least 1.");
                }
                primitives = value;
            }
        }

        /// <summary>
        /// Array of weights to be applied to the Morph Targets.
        /// </summary>
        [JsonProperty(PropertyName = "weights")]
        public float[] Weights
        {
            get => weights;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Weights)} must be at least 1.");
                }
                weights = value;
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays


        private MeshPrimitive[] primitives;
        private float[] weights;

        public override bool Validate()
        {
            return Primitives != null;
        }
    }

    /// <summary>
    /// Geometry to be rendered with the given material.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/mesh.primitive.schema.json"/>
    /// </summary>
    public class MeshPrimitive : GLTFProperty
    {
        [Preserve]
        public MeshPrimitive()
        {
            Mode = DrawMode.TRIANGLES;
        }

        /// <summary>
        /// A dictionary object, where each key corresponds to mesh attribute semantic
        /// and each value is the index of the accessor containing attribute's data.
        /// </summary>
        [JsonProperty(PropertyName = "attributes", Required = Required.Always)]
        public Dictionary<string, GLTFId> Attributes
        {
            get => attributes;
            set
            {
                if (value == null || value.Count < 1)
                {
                    throw new Exception($"{nameof(Attributes)} must be non null and have at least one element.");
                }
                attributes = value;
            }
        }

        /// <summary>
        /// The index of the accessor that contains mesh indices.
        /// When this is not defined, the primitives should be rendered without indices using `drawArrays()`.
        /// When defined, the accessor must contain indices:
        /// the `bufferView` referenced by the accessor should have a `target` equal to 34963 (ELEMENT_ARRAY_BUFFER);
        /// `componentType` must be 5121 (UNSIGNED_BYTE), 5123 (UNSIGNED_SHORT) or 5125 (UNSIGNED_INT), 
        /// the latter may require enabling additional hardware support; `type` must be `\"SCALAR\"`.
        /// For triangle primitives, the front face has a counter-clockwise (CCW) winding order.
        /// Values of the index accessor must not include the maximum value for the given component type,
        /// which triggers primitive restart in several graphics APIs and would require client implementations to rebuild the index buffer.
        /// Primitive restart values are disallowed and all index values must refer to actual vertices.
        /// As a result, the index accessor's values must not exceed the following maxima: BYTE `< 255`, UNSIGNED_SHORT `< 65535`, UNSIGNED_INT `< 4294967295`.
        /// </summary>
        [JsonProperty(PropertyName = "indices")]
        public GLTFId Indices { get; set; }

        /// <summary>
        /// The index of the material to apply to this primitive when rendering.
        /// </summary>
        [JsonProperty(PropertyName = "material")]
        public GLTFId Material { get; set; }

        /// <summary>
        /// The type of primitives to render.
        /// </summary>
        [JsonProperty(PropertyName = "mode")]
        public DrawMode Mode { get; set; }

        /// <summary>
        /// An array of Morph Targets, each  Morph Target is a dictionary mapping attributes to their deviations in the Morph Target.
        /// Each key corresponds to one of the three supported attribute semantic (`POSITION`, `NORMAL`, or `TANGENT`)
        /// and each value is the index of the accessor containing the attribute displacements' data.
        /// </summary>
        [JsonProperty(PropertyName = "targets")]
        public Dictionary<string, GLTFId>[] Targets
        {
            get => targets;
            set
            {
                if (value == null)
                {
                    targets = value;
                }
                else if (value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Targets)} must be at least 1.");
                }
                else if (!Array.TrueForAll(value, v => v != null && v.Count >= 1))
                {
                    throw new Exception($"Members of {nameof(Targets)} must be non null and each have at least one element.");
                }
                else
                {
                    targets = value;
                }
            }
        }


        private Dictionary<string, GLTFId> attributes;
        private Dictionary<string, GLTFId>[] targets;

        public override bool Validate()
        {
            return Attributes != null;
        }
    }

    public enum DrawMode : int
    {
        POINTS = 0,
        LINES = 1,
        LINE_LOOP = 2,
        LINE_STRIP = 3,
        TRIANGLES = 4,
        TRIANGLE_STRIP = 5,
        TRIANGLE_FAN = 6
    }

    public enum AttributeSemantic
    {
        POSITION, // VEC3
        NORMAL, // VEC3
        TANGENT, // VEC4
        TEXCOORD_0, // VEC2
        TEXCOORD_1, // VEC2
        COLOR_0, // VEC3, VEC4
        JOINTS_0, // VEC4
        WEIGHTS_0 // VEC4
    }
}