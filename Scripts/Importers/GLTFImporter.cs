using Gru.Helpers;
using Gru.Loaders;
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
        /// <param name="modelFilePath">Complete path to the Gltf/Glb file.</param>
        /// <param name="fileLoader">Utility used to load additional images and buffers.</param>
        /// <param name="textureLoader">Utility used to load textures.</param>
        /// <returns>Created GameObject.</returns>
        public static Task<GameObject> ImportAsync(
            string modelFilePath, IFileLoader fileLoader, ITextureLoader textureLoader = null)
        {
            var fileExtension = Path.GetExtension(modelFilePath);

            if (fileExtension == ".gltf")
            {
                return ImportAsync(File.Open(modelFilePath, FileMode.Open), false, fileLoader, textureLoader);
            }
            else if (fileExtension == ".glb")
            {
                return ImportAsync(File.Open(modelFilePath, FileMode.Open), true, fileLoader, textureLoader);
            }
            else
            {
                throw new Exception($"Unrecognized file extension {fileExtension}");
            }
        }

        /// <summary>
        /// Imports a gltf/glb model as a Unity GameObject. Must be called from main thread.
        /// </summary>
        /// <param name="modelDataStream">Stream used to access the model data. Function takes ownership of stream.</param>
        /// <param name="isGlb">Whether the data is gltf or glb.</param>
        /// <param name="fileLoader">Utility used to load additional images and buffers.</param>
        /// <param name="textureLoader">Utility used to load textures.</param>
        /// <returns>Created GameObject.</returns>
        public static async Task<GameObject> ImportAsync(
            Stream modelDataStream, bool isGlb, IFileLoader fileLoader, ITextureLoader textureLoader = null)
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

                byte[] glbEmbeddedBuffer = null;

                if (!isGlb)
                {
                    using (var jsonReader = new JsonTextReader(new StreamReader(modelDataStream)))
                    {
                        glTFRoot = serializer.Deserialize<GLTFRoot>(jsonReader);
                    }
                }
                else
                {
                    if (!GlbParser.IsValidGlb(modelDataStream))
                    {
                        throw new Exception("Stream does not contain valid glb data.");
                    }

                    var bufferBounds = GlbParser.GetBufferChunkBounds(modelDataStream);
                    glbEmbeddedBuffer = new byte[bufferBounds.Count];
                    modelDataStream.Position = bufferBounds.Offset;
                    modelDataStream.Read(glbEmbeddedBuffer, 0, (int)bufferBounds.Count);

                    var jsonBounds = GlbParser.GetJsonChunkBounds(modelDataStream);
                    modelDataStream.Position = jsonBounds.Offset;

                    using (var jsonReader = new JsonTextReader(new StreamReader(modelDataStream)))
                    {
                        glTFRoot = serializer.Deserialize<GLTFRoot>(jsonReader);
                    }
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

                if (textureLoader == null)
                {
                    textureLoader = new SimpleTextureLoader(fileLoader);
                }

                bufferImporter = new BufferImporter(glTFRoot.Buffers, glTFRoot.BufferViews, fileLoader);
                textureImporter = new TextureImporter(glTFRoot.Textures, glTFRoot.Images, glTFRoot.Samplers, bufferImporter, textureLoader);
                materialImporter = new MaterialImporter(glTFRoot.Materials, textureImporter);
                meshImporter = new MeshImporter(glTFRoot.Meshes, glTFRoot.Accessors, bufferImporter, materialImporter);
                nodeImporter = new NodeImporter(glTFRoot.Nodes, glTFRoot.Skins, glTFRoot.Accessors, meshImporter, bufferImporter);
                animationImporter = new AnimationImporter(glTFRoot.Accessors, bufferImporter, nodeImporter);

                if (glbEmbeddedBuffer != null)
                {
                    bufferImporter.SetGlbEmbeddedBuffer(glbEmbeddedBuffer);
                }
            });

            modelDataStream.Dispose();

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