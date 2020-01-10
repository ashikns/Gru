using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// A node in the node hierarchy. When the node contains `skin`,
    /// all `mesh.primitives` must contain `JOINTS_0` and `WEIGHTS_0` attributes.
    /// A node can have either a `matrix` or any combination of `translation`/`rotation`/`scale` (TRS) properties.
    /// TRS properties are converted to matrices and postmultiplied in the `T * R * S` order to
    /// compose the transformation matrix; first the scale is applied to the vertices, then the rotation,
    /// and then the translation. If none are provided, the transform is the identity.
    /// When a node is targeted for animation (referenced by an animation.channel.target), only TRS properties may be present; `matrix` will not be present.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/node.schema.json"/>
    /// </summary>
    public class Node : GLTFChildOfRootProperty
    {
        [Preserve]
        public Node()
        {
            /* Disable default values so that it validates against schema
            Matrix = new float[]
                {
                1.0f, 0.0f, 0.0f, 0.0f,
                0.0f, 1.0f, 0.0f, 0.0f,
                0.0f, 0.0f, 1.0f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
                };

            Rotation = new float[] { 0.0f, 0.0f, 0.0f, 1.0f };
            Scale = new float[] { 1.0f, 1.0f, 1.0f };
            Translation = new float[] { 0.0f, 0.0f, 0.0f };
            */
        }

        /// <summary>
        /// The index of the camera referenced by this node.
        /// </summary>
        [JsonProperty(PropertyName = "camera")]
        public GLTFId Camera { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// The indices of this node's children.
        /// </summary>
        [JsonProperty(PropertyName = "children")]
        public GLTFId[] Children
        {
            get => children;
            set
            {
                if (value == null)
                {
                    children = value;
                }
                else if (value.Length < 1 || value.Distinct().Count() != value.Length)
                {
                    throw new Exception($"{nameof(Children)} must contain unique elements and be of length at least 1.");
                }
                else
                {
                    children = value;
                }
            }
        }

        /// <summary>
        /// The index of the skin referenced by this node. When a skin is referenced by a node within a scene,
        /// all joints used by the skin must belong to the same scene.
        /// </summary>
        [JsonProperty(PropertyName = "skin")]
        public GLTFId Skin { get; set; }

        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column-major order.
        /// </summary>
        [JsonProperty(PropertyName = "matrix")]
        public float[] Matrix
        {
            get => matrix;
            set
            {
                if (value == null)
                {
                    matrix = value;
                }
                else if (Rotation != null || Scale != null || Translation != null)
                {
                    throw new Exception($"Matrix value can only be set when TRS is null");
                }
                else if (value.Length != 16)
                {
                    throw new Exception($"{nameof(Matrix)} must be of length 16");
                }
                else
                {
                    matrix = value;
                }
            }
        }

        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        [JsonProperty(PropertyName = "mesh")]
        public GLTFId Mesh { get; set; }

        /// <summary>
        /// The node's unit quaternion rotation in the order (x, y, z, w), where w is the scalar.
        /// </summary>
        [JsonProperty(PropertyName = "rotation")]
        public float[] Rotation
        {
            get => rotation;
            set
            {
                if (value == null)
                {
                    rotation = value;
                }
                else if (Matrix != null)
                {
                    throw new Exception($"{nameof(Rotation)} can have a value only when Matrix is null");
                }
                else if (value.Length != 4 || !Array.TrueForAll(value, v => v >= -1.0f && v <= 1.0f))
                {
                    throw new Exception($"Length of {nameof(Rotation)} should be 4 and values should lie between [-1.0, 1.0]");
                }
                else
                {
                    rotation = value;
                }
            }
        }

        /// <summary>
        /// The node's non-uniform scale, given as the scaling factors along the x, y, and z axes.
        /// </summary>
        [JsonProperty(PropertyName = "scale")]
        public float[] Scale
        {
            get => scale;
            set
            {
                if (value == null)
                {
                    scale = value;
                }
                else if (Matrix != null)
                {
                    throw new Exception($"{nameof(Scale)} can have a value only when Matrix is null");
                }
                else if (value.Length != 3)
                {
                    throw new Exception($"{nameof(Scale)} must be of length 3");
                }
                else
                {
                    scale = value;
                }
            }
        }

        /// <summary>
        /// The node's translation along the x, y, and z axes.
        /// </summary>
        [JsonProperty(PropertyName = "translation")]
        public float[] Translation
        {
            get => translation;
            set
            {
                if (value == null)
                {
                    translation = value;
                }
                else if (Matrix != null)
                {
                    throw new Exception($"{nameof(Translation)} can have a value only when Matrix is null");
                }
                else if (value.Length != 3)
                {
                    throw new Exception($"{nameof(Translation)} must be of length 3");
                }
                else
                {
                    translation = value;
                }
            }
        }

        /// <summary>
        /// The weights of the instantiated Morph Target.
        /// Number of elements must match number of Morph Targets of used mesh.
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


        private GLTFId[] children;
        private float[] matrix;
        private float[] rotation;
        private float[] scale;
        private float[] translation;
        private float[] weights;

        public override bool Validate()
        {
            // Weights -> Mesh
            return (Weights == null || Mesh != null)
                // Skin -> Mesh
                && (Skin == null || Mesh != null)
                /*
                 * "not": {
                        "anyOf": [
                            { "required": [ "matrix", "translation" ] },
                            { "required": [ "matrix", "rotation" ] },
                            { "required": [ "matrix", "scale" ] }
                        ]
                    }
                 * */
                && (Matrix == null || (Rotation == null && Scale == null && Translation == null));
        }
    }
}