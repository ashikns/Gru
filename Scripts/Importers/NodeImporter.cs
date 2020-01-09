using Gru.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Importers
{
    public class NodeImporter
    {
        private readonly IList<GLTF.Schema.Node> _nodeSchemas;
        private readonly IList<GLTF.Schema.Skin> _skinSchemas;
        private readonly IList<GLTF.Schema.Accessor> _accessorSchemas;
        private readonly MeshImporter _meshImporter;
        private readonly BufferImporter _bufferImporter;

        private readonly Lazy<Task<GameObject>>[] _nodes;

        public NodeImporter(
            IList<GLTF.Schema.Node> nodeSchemas,
            IList<GLTF.Schema.Skin> skinSchemas,
            IList<GLTF.Schema.Accessor> accessorSchemas,
            MeshImporter meshImporter,
            BufferImporter bufferImporter)
        {
            _nodeSchemas = nodeSchemas;
            _skinSchemas = skinSchemas;
            _accessorSchemas = accessorSchemas;
            _meshImporter = meshImporter;
            _bufferImporter = bufferImporter;

            _nodes = new Lazy<Task<GameObject>>[_nodeSchemas?.Count ?? 0];
        }

        // Must be run on main thread
        public Task<GameObject> GetNodeAsync(GLTF.Schema.GLTFId nodeId)
        {
            return _nodes.ThreadSafeGetOrAdd(nodeId.Key, () => ConstructNodeAsync(nodeId));
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