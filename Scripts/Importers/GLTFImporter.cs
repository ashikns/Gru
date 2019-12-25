using Gru.FileLoaders;
using Gru.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using GLTFExtension = Gru.GLTF.Schema.GLTFExtension;
using GLTFRoot = Gru.GLTF.Schema.GLTFRoot;

namespace Gru.Importers
{
    /// <summary>
    /// See <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/figures/dictionary-objects.png"/>
    /// for a top level view of dependencies (which decides import order).
    /// </summary>
    public static class GLTFImporter
    {
        /// <summary>
        /// Imports a gltf/glb model as a Unity GameObject. Must be called from main thread.
        /// </summary>
        /// <param name="modelFile">Complete path to the Gltf/Glb file.</param>
        /// <param name="fileLoader">Utility used to load additional images and buffers</param>
        public static async Task<GameObject> ImportAsync(string modelFile, IFileLoader fileLoader)
        {
            GLTFRoot glTFRoot = null;
            BufferImporter bufferImporter = null;
            TextureImporter textureImporter = null;
            MaterialImporter materialImporter = null;
            MeshImporter meshImporter = null;
            NodeImporter nodeImporter = null;
            AnimationImporter animationImporter = null;

            await Task.Run(() =>
            {
                JsonSerializer serializer = new JsonSerializer();
                var fileExtension = Path.GetExtension(modelFile);

                if (fileExtension == ".gltf")
                {
                    using (var stream = File.Open(modelFile, FileMode.Open))
                    using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
                    {
                        glTFRoot = serializer.Deserialize<GLTFRoot>(jsonReader);
                    }
                }
                else if (fileExtension == ".glb")
                {
                    using (var stream = File.Open(modelFile, FileMode.Open))
                    {
                        if (!GlbParser.IsValidGlb(stream))
                        {
                            throw new Exception("Stream does not contain valid glb data.");
                        }

                        var (Offset, Count) = GlbParser.GetJsonChunkBounds(stream);
                        stream.Position = Offset;

                        using (var jsonReader = new JsonTextReader(new StreamReader(stream)))
                        {
                            glTFRoot = serializer.Deserialize<GLTFRoot>(jsonReader);
                        }
                    }
                }
                else
                {
                    throw new Exception($"Unrecognized file extension {fileExtension}");
                }

                if (glTFRoot.Asset.Version != "2.0")
                {
                    throw new Exception($"GLTF version mismatch. Expected 2.0, got {glTFRoot.Asset.Version}");
                }
                if (glTFRoot.ExtensionsRequired != null)
                {
                    foreach (var extension in glTFRoot.ExtensionsRequired)
                    {
                        if (!Enum.TryParse<GLTFExtension>(extension, out _))
                        {
                            throw new Exception($"Required extension {extension} is not supported");
                        }
                    }
                }
                if (glTFRoot.ExtensionsUsed != null)
                {
                    foreach (var extension in glTFRoot.ExtensionsUsed)
                    {
                        if (!Enum.TryParse<GLTFExtension>(extension, out _))
                        {
                            Debug.LogWarning($"Used extension {extension} is not supported");
                        }
                    }
                }

                bufferImporter = new BufferImporter(glTFRoot.Buffers, glTFRoot.BufferViews, fileLoader, Path.GetFileName(modelFile));
                textureImporter = new TextureImporter(glTFRoot.Textures, glTFRoot.Images, glTFRoot.Samplers, bufferImporter, fileLoader);
                materialImporter = new MaterialImporter(glTFRoot.Materials, textureImporter);
                meshImporter = new MeshImporter(glTFRoot.Meshes, glTFRoot.Accessors, bufferImporter, materialImporter);
                nodeImporter = new NodeImporter(glTFRoot.Nodes, glTFRoot.Skins, glTFRoot.Accessors, meshImporter, bufferImporter);
                animationImporter = new AnimationImporter(glTFRoot.Accessors, bufferImporter, nodeImporter);
            });

            var sceneSchema = glTFRoot.Scenes[glTFRoot.Scene.Key];
            var sceneObj = new GameObject(!string.IsNullOrEmpty(sceneSchema.Name)
                ? sceneSchema.Name : $"GLTFScene_{glTFRoot.Scene.Key}");

            foreach (var nodeId in sceneSchema.Nodes)
            {
                var node = await nodeImporter.GetNodeAsync(nodeId);
                node.transform.SetParent(sceneObj.transform, false);
                node.SetActive(true);
            }

            if (glTFRoot.Animations != null && glTFRoot.Animations.Length > 0)
            {
                var animation = sceneObj.AddComponent<Animation>();

                for (int i = 0; i < glTFRoot.Animations.Length; ++i)
                {
                    var clip = await animationImporter.ConstructAnimationClipAsync(glTFRoot.Animations[i], sceneObj.transform);
                    clip.wrapMode = WrapMode.Loop;
                    animation.AddClip(clip, clip.name);

                    if (i == 0)
                    {
                        animation.clip = clip;
                        animation.Play();
                    }
                }
            }

            return sceneObj;
        }
    }
}