using Gru.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Importers
{
    public class NodeImporter
    {
        private IList<GLTF.Schema.Node> _nodeSchemas;
        private IList<GLTF.Schema.Skin> _skinSchemas;
        private IList<GLTF.Schema.Accessor> _accessorSchemas;
        private MeshImporter _meshImporter;
        private BufferImporter _bufferImporter;

        private readonly ConcurrentDictionary<int, Lazy<Task<GameObject>>> _nodes;

        public NodeImporter()
        {
            _nodes = new ConcurrentDictionary<int, Lazy<Task<GameObject>>>();
        }

        public void Assign(
            IList<GLTF.Schema.Node> nodeSchemas,
            IList<GLTF.Schema.Skin> skinSchemas,
            IList<GLTF.Schema.Accessor> accessorSchemas,
            MeshImporter meshImporter,
            BufferImporter bufferImporter)
        {
            _nodes.Clear();

            _nodeSchemas = nodeSchemas;
            _skinSchemas = skinSchemas;
            _accessorSchemas = accessorSchemas;
            _meshImporter = meshImporter;
            _bufferImporter = bufferImporter;
        }

        // Must be run on main thread
        public Task<GameObject> GetNodeAsync(GLTF.Schema.GLTFId nodeId)
        {
            var lazyResult = _nodes.GetOrAdd(
                nodeId.Key, new Lazy<Task<GameObject>>(() => ConstructNodeAsync(nodeId)));
            return lazyResult.Value;
        }

        private async Task<GameObject> ConstructNodeAsync(GLTF.Schema.GLTFId nodeId)
        {
            var nodeSchema = _nodeSchemas[nodeId.Key];
            var node = new GameObject(string.IsNullOrEmpty(nodeSchema.Name) ? $"Node_{nodeId.Key}" : nodeSchema.Name);
            node.SetActive(false);

            if (nodeSchema.Matrix != null)
            {
                nodeSchema.Matrix.ToUnityTRSConvert(out var position, out var rotation, out var scale);
                node.transform.localPosition = position;
                node.transform.localRotation = rotation;
                node.transform.localScale = scale;
            }
            else
            {
                node.transform.localPosition = nodeSchema.Translation?.ToUnityVector3Convert() ?? Vector3.zero;
                node.transform.localRotation = nodeSchema.Rotation?.ToUnityQuaternionConvert() ?? Quaternion.identity;
                node.transform.localScale = nodeSchema.Scale?.ToUnityVector3Raw() ?? Vector3.one;
            }

            if (nodeSchema.Children != null)
            {
                var children = await Task.WhenAll(ConstructNodes(nodeSchema.Children));
                foreach (var child in children)
                {
                    child.transform.SetParent(node.transform, false);
                    child.SetActive(true);
                }
            }

            if (nodeSchema.Mesh != null)
            {
                var rendererData = await _meshImporter.GetMeshAsync(nodeSchema.Mesh);

                if (nodeSchema.Skin != null || nodeSchema.Weights != null)
                {
                    var renderer = node.AddComponent<SkinnedMeshRenderer>();
                    renderer.sharedMesh = rendererData.Mesh;
                    renderer.sharedMaterials = rendererData.Materials;
                    renderer.quality = SkinQuality.Auto;

                    if (nodeSchema.Skin != null)
                    {
                        await SetupBones(_skinSchemas[nodeSchema.Skin.Key], renderer);
                    }
                    if (nodeSchema.Weights != null)
                    {
                        Debug.LogError("Node has weights. Unsupported.");
                    }
                }
                else
                {
                    var meshFilter = node.AddComponent<MeshFilter>();
                    meshFilter.sharedMesh = rendererData.Mesh;
                    var renderer = node.AddComponent<MeshRenderer>();
                    renderer.sharedMaterials = rendererData.Materials;
                }
            }

            return node;
        }

        private IEnumerable<Task<GameObject>> ConstructNodes(IList<GLTF.Schema.GLTFId> nodeIds)
        {
            for (int i = 0; i < nodeIds.Count; i++)
            {
                yield return GetNodeAsync(nodeIds[i]);
            }
        }

        private async Task SetupBones(GLTF.Schema.Skin skinSchema, SkinnedMeshRenderer skinnedMesh)
        {
            var boneCount = skinSchema.Joints.Length;
            var bones = new Transform[boneCount];

            for (int i = 0; i < boneCount; i++)
            {
                var node = await GetNodeAsync(skinSchema.Joints[i]);
                bones[i] = node.transform;
            }

            skinnedMesh.bones = bones;

            if (skinSchema.InverseBindMatrices != null)
            {
                var boneAccessor = _accessorSchemas[skinSchema.InverseBindMatrices.Key];
                var bindPoses = await Task.Run(async () =>
                {
                    var boneBuffer = await _bufferImporter.GetBufferViewAsync(boneAccessor.BufferView);
                    return boneAccessor.ReadAsMatrix4x4Array(boneBuffer, true);
                });
                skinnedMesh.sharedMesh.bindposes = bindPoses;
            }
            else
            {
                skinnedMesh.sharedMesh.bindposes = Enumerable.Repeat(Matrix4x4.identity, boneCount).ToArray();
            }

            if (skinSchema.Skeleton != null)
            {
                var rootBoneNode = await GetNodeAsync(skinSchema.Skeleton);
                skinnedMesh.rootBone = rootBoneNode.transform;
            }
            else
            {
                Debug.LogError("No rootbone specified. Auto rootbone unsupported.");
            }
        }
    }
}