using Gru.Helpers;
using Gru.Loaders;
using Gru.MaterialMaps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using GLTFExtension = Gru.GLTF.Schema.GLTFExtension;
using GLTFRoot = Gru.GLTF.Schema.GLTFRoot;

namespace Gru.Importers
{
    public class ImportOptions
    {
        public static ImportOptions MakeDefault(IBufferLoader bufferLoader)
        {
            return new ImportOptions
            {
                BufferLoader = bufferLoader,
                TextureLoader = new FileTextureLoader(bufferLoader),
                MetalRoughFactory = () => new MetallicRoughnessMap(),
                SpecGlossFactory = () => new SpecularGlossinessMap(),
                ActiveOnImport = true
            };
        }

        public IBufferLoader BufferLoader { get; set; }
        public ITextureLoader TextureLoader { get; set; }
        public Func<IMetallicRoughnessMap> MetalRoughFactory { get; set; }
        public Func<ISpecularGlossinessMap> SpecGlossFactory { get; set; }
        public bool ActiveOnImport { get; set; }
    }

    /// <summary>
    /// See <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/figures/dictionary-objects.png"/>
    /// for a top level view of dependencies (which decides import order).
    /// </summary>
    public class GLTFImporter
    {
        private readonly Stream _modelData;

        private GLTFRoot _gltf;
        private byte[] _embeddedBuffer;

        /// <summary>
        /// Create an importer that can read the specified model.
        /// </summary>
        /// <param name="modelData">Stream used to access the model data. Class takes ownership of stream.</param>
        public GLTFImporter(Stream modelData)
        {
            _modelData = modelData;
        }

        /// <summary>
        /// Creates an in memory representation of a gltf model.
        /// </summary>
        /// <returns>Object that represents the gltf model</returns>
        public GLTFRoot Parse()
        {
            if (_gltf != null) { return _gltf; }

            var glbDetails = GlbParser.Parse(_modelData);
            JsonSerializer serializer = new JsonSerializer();

            if (glbDetails == null)
            {
                _modelData.Position = 0;
                using (var jsonReader = new JsonTextReader(new StreamReader(_modelData)))
                {
                    _gltf = serializer.Deserialize<GLTFRoot>(jsonReader);
                }
            }
            else
            {
                if (glbDetails.HasEmbeddedBuffer)
                {
                    _embeddedBuffer = new byte[glbDetails.BufferChunkLength];
                    _modelData.Position = glbDetails.BufferChunkStart;
                    _modelData.Read(_embeddedBuffer, 0, glbDetails.BufferChunkLength);
                }

                _modelData.Position = glbDetails.JsonChunkStart;
                using (var jsonReader = new JsonTextReader(new StreamReader(_modelData)))
                {
                    _gltf = serializer.Deserialize<GLTFRoot>(jsonReader);
                }
            }

            _modelData.Dispose();
            return _gltf;
        }

        /// <summary>
        /// Gets the list of files that are referred to by the model.
        /// </summary>
        /// <returns>The list of files</returns>
        public IEnumerable<string> ReferredFiles()
        {
            var gltf = Parse();
            var files = new List<string>();

            if (gltf.Buffers != null)
            {
                foreach (var buffer in gltf.Buffers)
                {
                    if (!string.IsNullOrEmpty(buffer.Uri) && !UriHelper.TryParseDataUri(buffer.Uri, out _))
                    {
                        files.Add(buffer.Uri);
                    }
                }
            }

            if (gltf.Images != null)
            {
                foreach (var image in gltf.Images)
                {
                    if (!string.IsNullOrEmpty(image.Uri) && !UriHelper.TryParseDataUri(image.Uri, out _))
                    {
                        files.Add(image.Uri);
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// Imports a gltf/glb model as a Unity GameObject. Must be called from main thread.
        /// </summary>
        /// <param name="importOptions">Customizations applied to importer</param>
        /// <returns>Created Gameobject</returns>
        public async Task<GameObject> ImportAsync(ImportOptions importOptions)
        {
            var glTFRoot = _gltf ?? await Task.Run(() => Parse());

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

            var bufferImporter = new BufferImporter(glTFRoot.Buffers, glTFRoot.BufferViews, importOptions.BufferLoader);
            var textureImporter = new TextureImporter(glTFRoot.Textures, glTFRoot.Images, glTFRoot.Samplers, bufferImporter, importOptions.TextureLoader);
            var materialImporter = new MaterialImporter(glTFRoot.Materials, textureImporter, importOptions.MetalRoughFactory, importOptions.SpecGlossFactory);
            var meshImporter = new MeshImporter(glTFRoot.Meshes, glTFRoot.Accessors, bufferImporter, materialImporter);
            var nodeImporter = new NodeImporter(glTFRoot.Nodes, glTFRoot.Skins, glTFRoot.Accessors, meshImporter, bufferImporter);
            var animationImporter = new AnimationImporter(glTFRoot.Accessors, bufferImporter, nodeImporter);

            if (_embeddedBuffer != null)
            {
                bufferImporter.SetGlbEmbeddedBuffer(_embeddedBuffer);
            }

            var sceneSchema = glTFRoot.Scenes[glTFRoot.Scene.Key];
            var sceneObj = new GameObject(!string.IsNullOrEmpty(sceneSchema.Name)
                ? sceneSchema.Name : $"GLTFScene_{glTFRoot.Scene.Key}");
            sceneObj.SetActive(false);

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

            if (importOptions.ActiveOnImport)
            {
                sceneObj.SetActive(true);
            }
            return sceneObj;
        }

        /// <summary>
        /// Imports a gltf/glb model stored on the local filesystem as a Unity GameObject. Must be called from main thread.
        /// </summary>
        /// <param name="modelFilePath">Complete path to the Gltf/Glb file.</param>
        /// <returns>Created GameObject.</returns>
        public static Task<GameObject> ImportAsync(string modelFilePath)
        {
            var importer = new GLTFImporter(File.Open(modelFilePath, FileMode.Open));
            importer.Parse();
            return importer.ImportAsync(ImportOptions.MakeDefault(new FileBufferLoader(Path.GetDirectoryName(modelFilePath))));
        }
    }
}