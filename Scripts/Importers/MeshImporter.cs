using Gru.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using AttributeSemantic = Gru.GLTF.Schema.AttributeSemantic;
using AttributeType = Gru.GLTF.Schema.AttributeType;
using ComponentType = Gru.GLTF.Schema.ComponentType;
using DrawMode = Gru.GLTF.Schema.DrawMode;

namespace Gru.Importers
{
    public class MeshImporter
    {
        private class MeshData
        {
            public MeshData(GLTF.Schema.Mesh mesh, IList<GLTF.Schema.Accessor> accessors)
            {
                Offset = new int[mesh.Primitives.Length];

                var totalVertices = 0;
                for (int i = 0; i < mesh.Primitives.Length; i++)
                {
                    if (mesh.Primitives[i].Attributes.TryGetValue(
                        AttributeSemantic.POSITION.ToString(), out var positionAccessor))
                    {
                        Offset[i] = totalVertices;
                        totalVertices += accessors[positionAccessor.Key].Count;
                    }
                    else
                    {
                        Debug.LogWarning("Primitive without vertices found.");
                    }
                }

                Vertices = new Vector3[totalVertices];
                Normals = mesh.Primitives.Any(p => p.Attributes.ContainsKey(AttributeSemantic.NORMAL.ToString())) ? new Vector3[totalVertices] : null;
                Tangents = mesh.Primitives.Any(p => p.Attributes.ContainsKey(AttributeSemantic.TANGENT.ToString())) ? new Vector4[totalVertices] : null;
                UV_0 = mesh.Primitives.Any(p => p.Attributes.ContainsKey(AttributeSemantic.TEXCOORD_0.ToString())) ? new Vector2[totalVertices] : null;
                UV_1 = mesh.Primitives.Any(p => p.Attributes.ContainsKey(AttributeSemantic.TEXCOORD_1.ToString())) ? new Vector2[totalVertices] : null;
                Colors_0 = mesh.Primitives.Any(p => p.Attributes.ContainsKey(AttributeSemantic.COLOR_0.ToString())) ? new Color[totalVertices] : null;
                BoneWeights_0 = mesh.Primitives.Any(p => p.Attributes.ContainsKey(AttributeSemantic.WEIGHTS_0.ToString())) ? new BoneWeight[totalVertices] : null;

                Topology = new MeshTopology[mesh.Primitives.Length];
                Indices = new int[mesh.Primitives.Length][];
            }

            public Vector3[] Vertices { get; }
            public Vector3[] Normals { get; }
            public Vector4[] Tangents { get; }
            public Vector2[] UV_0 { get; }
            public Vector2[] UV_1 { get; }
            public Color[] Colors_0 { get; }
            public BoneWeight[] BoneWeights_0 { get; }

            public MeshTopology[] Topology { get; }
            public int[][] Indices { get; }
            public int[] Offset { get; }
        }

        private IList<GLTF.Schema.Mesh> _meshSchemas;
        private IList<GLTF.Schema.Accessor> _accessors;
        private BufferImporter _bufferImporter;
        private MaterialImporter _materialImporter;

        private readonly ConcurrentDictionary<int, Lazy<Task<ImporterResults.RendererData>>> _meshes;

        public MeshImporter()
        {
            _meshes = new ConcurrentDictionary<int, Lazy<Task<ImporterResults.RendererData>>>();
        }

        public void Assign(
            IList<GLTF.Schema.Mesh> meshes,
            IList<GLTF.Schema.Accessor> accessors,
            BufferImporter bufferImporter,
            MaterialImporter materialImporter)
        {
            _meshes.Clear();

            _meshSchemas = meshes;
            _accessors = accessors;
            _bufferImporter = bufferImporter;
            _materialImporter = materialImporter;
        }

        // Must be run on main thread
        public Task<ImporterResults.RendererData> GetMeshAsync(GLTF.Schema.GLTFId meshId)
        {
            var lazyResult = _meshes.GetOrAdd(
                meshId.Key, new Lazy<Task<ImporterResults.RendererData>>(() => ConstructMeshAsync(meshId)));
            return lazyResult.Value;
        }

        private async Task<ImporterResults.RendererData> ConstructMeshAsync(GLTF.Schema.GLTFId meshId)
        {
            var meshSchema = _meshSchemas[meshId.Key];
            var meshData = await Task.Run(() => ImportMeshData(meshSchema, _accessors, _bufferImporter));

            var unityMesh = new Mesh
            {
                name = meshSchema.Name,
                indexFormat = meshData.Vertices.Length > ushort.MaxValue ? IndexFormat.UInt32 : IndexFormat.UInt16,
                vertices = meshData.Vertices,
                normals = meshData.Normals,
                tangents = meshData.Tangents,
                uv = meshData.UV_0,
                uv2 = meshData.UV_1,
                colors = meshData.Colors_0,
                boneWeights = meshData.BoneWeights_0,

                subMeshCount = meshSchema.Primitives.Length
            };

            var materials = new Material[meshSchema.Primitives.Length];
            var defaultMaterialId = meshSchema.Primitives.FirstOrDefault(p => p.Material != null).Material;

            for (int subMeshIdx = 0; subMeshIdx < meshSchema.Primitives.Length; subMeshIdx++)
            {
                if (meshData.Indices[subMeshIdx] == null)
                {
                    continue;
                }

                unityMesh.SetIndices(meshData.Indices[subMeshIdx], meshData.Topology[subMeshIdx], subMeshIdx, false, meshData.Offset[subMeshIdx]);

                var materialId = meshSchema.Primitives[subMeshIdx].Material;
                materials[subMeshIdx] = materialId != null
                    ? await _materialImporter.GetMaterialAsync(materialId)
                    : await _materialImporter.GetMaterialAsync(defaultMaterialId);
            }

            unityMesh.RecalculateBounds();

            if (meshData.Normals == null)
            {
                unityMesh.RecalculateNormals();
            }

            unityMesh.UploadMeshData(true);

            var rendererData = new ImporterResults.RendererData
            {
                Mesh = unityMesh,
                Materials = materials
            };

            return rendererData;
        }

        private static async Task<MeshData> ImportMeshData(
            GLTF.Schema.Mesh meshSchema,
            IList<GLTF.Schema.Accessor> accessors,
            BufferImporter bufferImporter)
        {
            var meshData = new MeshData(meshSchema, accessors);
            await Task.WhenAll(ImportMeshPrimitives(meshSchema, meshData, accessors, bufferImporter));

            if (meshSchema.Weights != null)
            {
                Debug.LogError("Mesh has weights. Unsupported.");
            }

            return meshData;
        }

        private static IEnumerable<Task> ImportMeshPrimitives(
            GLTF.Schema.Mesh meshSchema,
            MeshData meshData,
            IList<GLTF.Schema.Accessor> accessors,
            BufferImporter bufferImporter)
        {
            for (int i = 0; i < meshSchema.Primitives.Length; i++)
            {
                yield return ImportMeshPrimitive(meshData, i, meshSchema.Primitives[i], accessors, bufferImporter);
            }
        }

        private static async Task ImportMeshPrimitive(
            MeshData meshData,
            int primitiveIndex,
            GLTF.Schema.MeshPrimitive primitiveSchema,
            IList<GLTF.Schema.Accessor> accessors,
            BufferImporter bufferImporter)
        {
            if (!primitiveSchema.Attributes.TryGetValue(AttributeSemantic.POSITION.ToString(), out var positionAttrib))
            {
                Debug.LogWarning("Primitive without vertices.");
                return;
            }

            foreach (var attrib in primitiveSchema.Attributes)
            {
                if (!Enum.TryParse<AttributeSemantic>(attrib.Key, out var attribEnum))
                {
                    throw new Exception($"Unsupported attribute type {attrib.Key}");
                }

                var accessor = accessors[attrib.Value.Key];

                if (accessor.BufferView != null)
                {
                    var bufferView = await bufferImporter.GetBufferViewAsync(accessor.BufferView);
                    var meshDataOffset = meshData.Offset[primitiveIndex];

                    switch (attribEnum)
                    {
                        case AttributeSemantic.POSITION:
                            accessor.ReadAsVector3Array(meshData.Vertices, meshDataOffset, bufferView, true);
                            break;
                        case AttributeSemantic.NORMAL:
                            accessor.ReadAsVector3Array(meshData.Normals, meshDataOffset, bufferView, true);
                            break;
                        case AttributeSemantic.TANGENT:
                            accessor.ReadAsVector4Array(meshData.Tangents, meshDataOffset, bufferView, true);
                            break;
                        case AttributeSemantic.TEXCOORD_0:
                            accessor.ReadAsUVArray(meshData.UV_0, meshDataOffset, bufferView, true);
                            break;
                        case AttributeSemantic.TEXCOORD_1:
                            accessor.ReadAsUVArray(meshData.UV_1, meshDataOffset, bufferView, true);
                            break;
                        case AttributeSemantic.COLOR_0:
                            accessor.ReadAsColorArray(meshData.Colors_0, meshDataOffset, bufferView);
                            break;
                        case AttributeSemantic.JOINTS_0:
                            // Do nothing, handled in WEIGHTS_0
                            break;
                        case AttributeSemantic.WEIGHTS_0:
                            try
                            {
                                var jointAttrib = primitiveSchema.Attributes[AttributeSemantic.JOINTS_0.ToString()];
                                var jointAccessor = accessors[jointAttrib.Key];
                                var jointBuffer = await bufferImporter.GetBufferViewAsync(jointAccessor.BufferView);

                                CreateBoneWeightArray(meshData.BoneWeights_0, meshDataOffset, jointAccessor, accessor, jointBuffer, bufferView);
                            }
                            catch (Exception)
                            {
                                throw new Exception("Found WEIGHTS semantic but no JOINTS");
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(attribEnum));
                    }
                }
                else
                {
                    Debug.LogError("Accessor buffer view is null. Unsupported.");
                }

                if (accessor.Sparse != null)
                {
                    Debug.LogError("Sparse data in accessor. Unsupported.");
                }
            }

            if (primitiveSchema.Indices != null)
            {
                var indexAccessor = accessors[primitiveSchema.Indices.Key];
                var indexBuffer = await bufferImporter.GetBufferViewAsync(indexAccessor.BufferView);

                switch (primitiveSchema.Mode)
                {
                    case DrawMode.TRIANGLES:
                        meshData.Indices[primitiveIndex] = new int[indexAccessor.Count];
                        indexAccessor.ReadAsTriangleIndices(meshData.Indices[primitiveIndex], 0, indexBuffer, true);
                        meshData.Topology[primitiveIndex] = MeshTopology.Triangles;
                        break;
                    default:
                        throw new Exception($"Unsupported drawmode for reading indices: {primitiveSchema.Mode}");
                }
            }
            else
            {
                var vertCount = accessors[positionAttrib.Key].Count;
                meshData.Indices[primitiveIndex] = Enumerable.Range(0, vertCount).ToArray();

                switch (primitiveSchema.Mode)
                {
                    case DrawMode.TRIANGLES:
                        meshData.Topology[primitiveIndex] = MeshTopology.Triangles;
                        break;
                    case DrawMode.POINTS:
                        meshData.Topology[primitiveIndex] = MeshTopology.Points;
                        break;
                    case DrawMode.LINES:
                        meshData.Topology[primitiveIndex] = MeshTopology.Lines;
                        break;
                    case DrawMode.LINE_STRIP:
                        meshData.Topology[primitiveIndex] = MeshTopology.LineStrip;
                        break;
                    default:
                        throw new Exception("Unity does not support glTF draw mode: " + primitiveSchema.Mode);
                }
            }

            if (primitiveSchema.Targets != null)
            {
                Debug.LogError("Morph targets are present. Unsupported.");
            }
        }

        // We could just read the accessor to arrays and pass them here, but that would be two additional arrays created per submesh
        private static void CreateBoneWeightArray(
            BoneWeight[] outArray, int outArrayOffset, GLTF.Schema.Accessor jointAccessor, GLTF.Schema.Accessor weightAccessor, ImporterResults.BufferView jointBuffer, ImporterResults.BufferView weightBuffer)
        {
            if (jointAccessor.BufferView == null)
            {
                throw new ArgumentNullException(nameof(jointAccessor.BufferView));
            }
            if (weightAccessor.BufferView == null)
            {
                throw new ArgumentNullException(nameof(weightAccessor.BufferView));
            }
            if (jointAccessor.Type != AttributeType.VEC4 || weightAccessor.Type != AttributeType.VEC4)
            {
                throw new Exception("Vector4 data requires VEC4 type data.");
            }
            if (jointAccessor.ComponentType == ComponentType.UNSIGNED_INT || weightAccessor.ComponentType == ComponentType.UNSIGNED_INT)
            {
                throw new Exception($"{ComponentType.UNSIGNED_INT} is disallowed here.");
            }
            if (jointAccessor.Count != weightAccessor.Count)
            {
                throw new Exception("Joints and Weights must be of same length");
            }

            var jointConverter = jointAccessor.GetIntConverter();
            var jointOffset = jointBuffer.Data.Offset + jointAccessor.ByteOffset;
            var jointComponentSize = jointAccessor.GetComponentSize();
            uint jointStride = jointBuffer.Stride > 0 ? jointBuffer.Stride : jointComponentSize * 4;

            var weightConverter = weightAccessor.GetFloatConverter();
            var weightOffset = weightBuffer.Data.Offset + weightAccessor.ByteOffset;
            var weightComponentSize = weightAccessor.GetComponentSize();
            uint weightStride = weightBuffer.Stride > 0 ? weightBuffer.Stride : weightComponentSize * 4;

            for (int i = 0; i < weightAccessor.Count; i++)
            {
                outArray[outArrayOffset + i].boneIndex0 = jointConverter(jointBuffer.Data.Array, (int)(jointOffset + i * jointStride));
                outArray[outArrayOffset + i].boneIndex1 = jointConverter(jointBuffer.Data.Array, (int)(jointOffset + i * jointStride + jointComponentSize));
                outArray[outArrayOffset + i].boneIndex2 = jointConverter(jointBuffer.Data.Array, (int)(jointOffset + i * jointStride + jointComponentSize * 2));
                outArray[outArrayOffset + i].boneIndex3 = jointConverter(jointBuffer.Data.Array, (int)(jointOffset + i * jointStride + jointComponentSize * 3));

                var weight0 = weightConverter(weightBuffer.Data.Array, (int)(weightOffset + i * weightStride));
                var weight1 = weightConverter(weightBuffer.Data.Array, (int)(weightOffset + i * weightStride + weightComponentSize));
                var weight2 = weightConverter(weightBuffer.Data.Array, (int)(weightOffset + i * weightStride + weightComponentSize * 2));
                var weight3 = weightConverter(weightBuffer.Data.Array, (int)(weightOffset + i * weightStride + weightComponentSize * 3));
                var weightSum = weight0 + weight1 + weight2 + weight3;

                if (!Mathf.Approximately(weightSum, 0))
                {
                    // normalize weights (built-in normalize function only normalizes three components)
                    outArray[outArrayOffset + i].weight0 = weight0 / weightSum;
                    outArray[outArrayOffset + i].weight1 = weight1 / weightSum;
                    outArray[outArrayOffset + i].weight2 = weight2 / weightSum;
                    outArray[outArrayOffset + i].weight3 = weight3 / weightSum;
                }
                else
                {
                    outArray[outArrayOffset + i].weight0 = weight0;
                    outArray[outArrayOffset + i].weight1 = weight1;
                    outArray[outArrayOffset + i].weight2 = weight2;
                    outArray[outArrayOffset + i].weight3 = weight3;
                }
            }
        }
    }
}