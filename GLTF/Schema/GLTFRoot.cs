using Newtonsoft.Json;
using System;
using System.Linq;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// The root object for a glTF asset.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/glTF.schema.json"/>
    /// </summary>
    public class GLTFRoot : GLTFProperty
    {
        [Preserve]
        public GLTFRoot() { }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// Names of glTF extensions used somewhere in this asset.
        /// </summary>
        [JsonProperty(PropertyName = "extensionsUsed")]
        public string[] ExtensionsUsed
        {
            get => extensionsUsed;
            set
            {
                if (value == null)
                {
                    extensionsUsed = value;
                }
                else if (value.Length < 1 || value.Distinct().Count() != value.Length)
                {
                    throw new Exception($"{nameof(ExtensionsUsed)} must contain unique elements and be of length at least 1.");
                }
                else
                {
                    extensionsUsed = value;
                }
            }
        }

        /// <summary>
        /// Names of glTF extensions required to properly load this asset.
        /// </summary>
        [JsonProperty(PropertyName = "extensionsRequired")]
        public string[] ExtensionsRequired
        {
            get => extensionsRequired;
            set
            {
                if (value == null)
                {
                    extensionsRequired = value;
                }
                else if (value.Length < 1 || value.Distinct().Count() != value.Length)
                {
                    throw new Exception($"{nameof(ExtensionsRequired)} must contain unique elements and be of length at least 1.");
                }
                else
                {
                    extensionsRequired = value;
                }
            }
        }

        /// <summary>
        /// An array of accessors. An accessor is a typed view into a bufferView.
        /// </summary>
        [JsonProperty(PropertyName = "accessors")]
        public Accessor[] Accessors
        {
            get => accessors;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Accessors)} cannot be less than one");
                }
                accessors = value;
            }
        }

        /// <summary>
        /// An array of keyframe animations.
        /// </summary>
        [JsonProperty(PropertyName = "animations")]
        public Animation[] Animations
        {
            get => animations;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Animations)} cannot be less than one");
                }
                animations = value;
            }
        }

        /// <summary>
        /// Metadata about the glTF asset.
        /// </summary>
        [JsonProperty(PropertyName = "asset", Required = Required.Always)]
        public Asset Asset { get; set; }

        /// <summary>
        /// An array of buffers. A buffer points to binary geometry, animation, or skins.
        /// </summary>
        [JsonProperty(PropertyName = "buffers")]
        public Buffer[] Buffers
        {
            get => buffers;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Buffers)} cannot be less than one");
                }
                buffers = value;
            }
        }

        /// <summary>
        /// An array of bufferViews. A bufferView is a view into a buffer generally representing a subset of the buffer.
        /// </summary>
        [JsonProperty(PropertyName = "bufferViews")]
        public BufferView[] BufferViews
        {
            get => bufferViews;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(BufferViews)} cannot be less than one");
                }
                bufferViews = value;
            }
        }

        /// <summary>
        /// An array of cameras. A camera defines a projection matrix.
        /// </summary>
        [JsonProperty(PropertyName = "cameras")]
        public Camera[] Cameras
        {
            get => cameras;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Cameras)} cannot be less than one");
                }
                cameras = value;
            }
        }

        /// <summary>
        /// An array of images. An image defines data used to create a texture.
        /// </summary>
        [JsonProperty(PropertyName = "images")]
        public Image[] Images
        {
            get => images;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Images)} cannot be less than one");
                }
                images = value;
            }
        }

        /// <summary>
        /// An array of materials. A material defines the appearance of a primitive.
        /// </summary>
        [JsonProperty(PropertyName = "materials")]
        public Material[] Materials
        {
            get => materials;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Materials)} cannot be less than one");
                }
                materials = value;
            }
        }

        /// <summary>
        /// An array of meshes. A mesh is a set of primitives to be rendered.
        /// </summary>
        [JsonProperty(PropertyName = "meshes")]
        public Mesh[] Meshes
        {
            get => meshes;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Meshes)} cannot be less than one");
                }
                meshes = value;
            }
        }

        /// <summary>
        /// An array of nodes.
        /// </summary>
        [JsonProperty(PropertyName = "nodes")]
        public Node[] Nodes
        {
            get => nodes;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Nodes)} cannot be less than one");
                }
                nodes = value;
            }
        }

        /// <summary>
        /// An array of samplers. A sampler contains properties for texture filtering and wrapping modes.
        /// </summary>
        [JsonProperty(PropertyName = "samplers")]
        public Sampler[] Samplers
        {
            get => samplers;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Samplers)} cannot be less than one");
                }
                samplers = value;
            }
        }

        /// <summary>
        /// The index of the default scene.
        /// </summary>
        [JsonProperty(PropertyName = "scene")]
        public GLTFId Scene { get; set; }

        /// <summary>
        /// An array of scenes.
        /// </summary>
        [JsonProperty(PropertyName = "scenes")]
        public Scene[] Scenes
        {
            get => scenes;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Scenes)} cannot be less than one");
                }
                scenes = value;
            }
        }

        /// <summary>
        /// An array of skins. A skin is defined by joints and matrices.
        /// </summary>
        [JsonProperty(PropertyName = "skins")]
        public Skin[] Skins
        {
            get => skins;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Skins)} cannot be less than one");
                }
                skins = value;
            }
        }

        /// <summary>
        /// An array of textures.
        /// </summary>
        [JsonProperty(PropertyName = "textures")]
        public Texture[] Textures
        {
            get => textures;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Textures)} cannot be less than one");
                }
                textures = value;
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays


        private string[] extensionsUsed;
        private string[] extensionsRequired;
        private Accessor[] accessors;
        private Animation[] animations;
        private Buffer[] buffers;
        private BufferView[] bufferViews;
        private Camera[] cameras;
        private Image[] images;
        private Material[] materials;
        private Mesh[] meshes;
        private Node[] nodes;
        private Sampler[] samplers;
        private Scene[] scenes;
        private Skin[] skins;
        private Texture[] textures;

        public override bool Validate()
        {
            return Asset != null
                // Scene -> Scenes
                && (Scene == null || Scenes != null);
        }
    }
}