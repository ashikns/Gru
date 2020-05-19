using Gru.GLTF.Schema;
using System;
using UnityEngine;

namespace Gru.Extensions
{
    public static class AccessorExtensions
    {
        public static void ReadAsUVArray(this Accessor accessor, Vector2[] outArray, int outArrayOffset, ImporterResults.BufferView bufferView, bool flipY)
        {
            if (accessor.Type != AttributeType.VEC2)
            {
                throw new Exception("Vector2 data requires VEC2 type data.");
            }
            if (accessor.ComponentType == ComponentType.UNSIGNED_INT)
            {
                throw new Exception($"{ComponentType.UNSIGNED_INT} is disallowed here.");
            }

            var floatConverter = accessor.GetFloatConverter();

            var totalOffset = bufferView.Data.Offset + accessor.ByteOffset;
            var componentSize = accessor.GetComponentSize();
            uint stride = bufferView.Stride > 0 ? bufferView.Stride : componentSize * 2;

            if (flipY)
            {
                for (int i = 0; i < accessor.Count; i++)
                {
                    // Flips the V component of the UV (1-V)
                    outArray[outArrayOffset + i].x = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride));
                    outArray[outArrayOffset + i].y = 1.0f - floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize));
                }
            }
            else
            {
                for (int i = 0; i < accessor.Count; i++)
                {
                    outArray[outArrayOffset + i].x = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride));
                    outArray[outArrayOffset + i].y = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize));
                }
            }
        }

        public static void ReadAsVector3Array(this Accessor accessor, Vector3[] outArray, int outArrayOffset, ImporterResults.BufferView bufferView, bool flipX)
        {
            if (accessor.Type != AttributeType.VEC3)
            {
                throw new Exception("Vector3 data requires VEC3 type data.");
            }

            var floatConverter = accessor.GetFloatConverter();

            var totalOffset = bufferView.Data.Offset + accessor.ByteOffset;
            var componentSize = accessor.GetComponentSize();
            uint stride = bufferView.Stride > 0 ? bufferView.Stride : componentSize * 3;

            var flipVal = flipX ? -1 : 1;

            for (int i = 0; i < accessor.Count; i++)
            {
                // Flip X
                outArray[outArrayOffset + i].x = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride));
                outArray[outArrayOffset + i].y = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize));
                outArray[outArrayOffset + i].z = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 2));
            }
        }

        public static void ReadAsVector4Array(this Accessor accessor, Vector4[] outArray, int outArrayOffset, ImporterResults.BufferView bufferView, bool flipXW)
        {
            if (accessor.Type != AttributeType.VEC4)
            {
                throw new Exception("Vector4 data requires VEC4 type data.");
            }
            if (accessor.ComponentType == ComponentType.UNSIGNED_INT)
            {
                throw new Exception($"{ComponentType.UNSIGNED_INT} is disallowed here.");
            }

            var floatConverter = accessor.GetFloatConverter();

            var totalOffset = bufferView.Data.Offset + accessor.ByteOffset;
            var componentSize = accessor.GetComponentSize();
            uint stride = bufferView.Stride > 0 ? bufferView.Stride : componentSize * 4;

            var flipVal = flipXW ? -1 : 1;

            for (int i = 0; i < accessor.Count; i++)
            {
                // Flip X, W
                outArray[outArrayOffset + i].x = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride));
                outArray[outArrayOffset + i].y = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize));
                outArray[outArrayOffset + i].z = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 2));
                outArray[outArrayOffset + i].w = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 3));
            }
        }

        public static void ReadAsColorArray(this Accessor accessor, Color[] outArray, int outArrayOffset, ImporterResults.BufferView bufferView)
        {
            if (accessor.ComponentType == ComponentType.UNSIGNED_INT)
            {
                throw new Exception($"{ComponentType.UNSIGNED_INT} is disallowed here.");
            }

            var floatConverter = accessor.GetFloatConverter();

            var totalOffset = bufferView.Data.Offset + accessor.ByteOffset;
            var componentSize = accessor.GetComponentSize();

            if (accessor.Type == AttributeType.VEC3)
            {
                uint stride = bufferView.Stride > 0 ? bufferView.Stride : componentSize * 3;

                for (int i = 0; i < accessor.Count; i++)
                {
                    outArray[outArrayOffset + i].r = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride));
                    outArray[outArrayOffset + i].g = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize));
                    outArray[outArrayOffset + i].b = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 2));
                    outArray[outArrayOffset + i].a = 1;
                }
            }
            else if (accessor.Type == AttributeType.VEC4)
            {
                uint stride = bufferView.Stride > 0 ? bufferView.Stride : componentSize * 4;

                for (int i = 0; i < accessor.Count; i++)
                {
                    outArray[outArrayOffset + i].r = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride));
                    outArray[outArrayOffset + i].g = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize));
                    outArray[outArrayOffset + i].b = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 2));
                    outArray[outArrayOffset + i].a = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 3));
                }
            }
            else
            {
                throw new Exception("Color data requires VEC3/VEC4 type data.");
            }
        }

        public static void ReadAsTriangleIndices(this Accessor accessor, int[] outArray, int outArrayOffset, ImporterResults.BufferView bufferView, bool flipWinding)
        {
            if (accessor.ComponentType != ComponentType.UNSIGNED_INT
                && accessor.ComponentType != ComponentType.UNSIGNED_SHORT
                && accessor.ComponentType != ComponentType.UNSIGNED_BYTE)
            {
                throw new Exception($"{accessor.ComponentType} is not a valid component type for indices.");
            }
            if (accessor.Type != AttributeType.SCALAR)
            {
                throw new Exception("Attribute type must be SCALAR for indices.");
            }
            if (accessor.Count % 3 != 0)
            {
                throw new Exception("Accessor count must be a multiple of 3 for triangles.");
            }

            var intConverter = accessor.GetIntConverter();

            var totalOffset = bufferView.Data.Offset + accessor.ByteOffset;
            var componentSize = accessor.GetComponentSize();

            uint stride = bufferView.Stride > 0 ? bufferView.Stride : componentSize;

            if (flipWinding)
            {
                for (int i = 0; i < accessor.Count; i += 3)
                {
                    // Reversed triangle read order
                    outArray[outArrayOffset + i] = intConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 2));
                    outArray[outArrayOffset + i + 1] = intConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize));
                    outArray[outArrayOffset + i + 2] = intConverter(bufferView.Data.Array, (int)(totalOffset + i * stride));
                }
            }
            else
            {
                for (int i = 0; i < accessor.Count; i++)
                {
                    outArray[outArrayOffset + i] = intConverter(bufferView.Data.Array, (int)(totalOffset + i * stride));
                }
            }
        }

        public static Matrix4x4[] ReadAsMatrix4x4Array(this Accessor accessor, ImporterResults.BufferView bufferView, bool convert)
        {
            if (accessor.Type != AttributeType.MAT4)
            {
                throw new Exception("Expected MAT4 type data.");
            }
            if (accessor.ComponentType == ComponentType.UNSIGNED_INT)
            {
                throw new Exception($"{ComponentType.UNSIGNED_INT} is disallowed here.");
            }

            var floatConverter = accessor.GetFloatConverter();

            var totalOffset = bufferView.Data.Offset + accessor.ByteOffset;
            var componentSize = accessor.GetComponentSize();
            uint stride = bufferView.Stride > 0 ? bufferView.Stride : componentSize * 4 * 4;

            var flipVal = convert ? -1.0f : 1.0f;

            /*
             * GLTF stores matrices in column major order.
             * 
             * Matrix when converted to Unity coord space will look like (after accessing in column major order):
             *   m00 -m01 -m02 -m03
             *  -m10  m11  m12  m13
             *  -m20  m21  m22  m23
             *  -m30  m31  m32  m33
             * */
            var matrices = new Matrix4x4[accessor.Count];
            for (int i = 0; i < accessor.Count; i++)
            {
                matrices[i].m00 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 0));
                matrices[i].m10 = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 1));
                matrices[i].m20 = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 2));
                matrices[i].m30 = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 3));

                matrices[i].m01 = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 4));
                matrices[i].m11 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 5));
                matrices[i].m21 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 6));
                matrices[i].m31 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 7));

                matrices[i].m02 = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 8));
                matrices[i].m12 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 9));
                matrices[i].m22 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 10));
                matrices[i].m32 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 11));

                matrices[i].m03 = flipVal * floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 12));
                matrices[i].m13 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 13));
                matrices[i].m23 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 14));
                matrices[i].m33 = floatConverter(bufferView.Data.Array, (int)(totalOffset + i * stride + componentSize * 15));
            }
            return matrices;
        }

        public static Func<byte[], int, float> GetFloatConverter(this Accessor accessor)
        {
            if (accessor.Normalized)
            {
                switch (accessor.ComponentType)
                {
                    case ComponentType.BYTE:
                        return (x, y) => Convert.ToSByte(x[y]) / (float)sbyte.MaxValue;
                    case ComponentType.UNSIGNED_BYTE:
                        return (x, y) => x[y] / (float)byte.MaxValue;
                    case ComponentType.SHORT:
                        return (x, y) => x.GetShortElement(y) / (float)short.MaxValue;
                    case ComponentType.UNSIGNED_SHORT:
                        return (x, y) => x.GetUShortElement(y) / (float)ushort.MaxValue;
                    case ComponentType.UNSIGNED_INT:
                        return (x, y) => x.GetUIntElement(y) / (float)uint.MaxValue;
                    case ComponentType.FLOAT:
                        return (x, y) => x.GetFloatElement(y);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(accessor.ComponentType));
                }
            }
            else
            {
                switch (accessor.ComponentType)
                {
                    case ComponentType.BYTE:
                        return (x, y) => Convert.ToSByte(x[y]);
                    case ComponentType.UNSIGNED_BYTE:
                        return (x, y) => x[y];
                    case ComponentType.SHORT:
                        return (x, y) => x.GetShortElement(y);
                    case ComponentType.UNSIGNED_SHORT:
                        return (x, y) => x.GetUShortElement(y);
                    case ComponentType.UNSIGNED_INT:
                        return (x, y) => x.GetUIntElement(y);
                    case ComponentType.FLOAT:
                        return (x, y) => x.GetFloatElement(y);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(accessor.ComponentType));
                }
            }
        }

        public static Func<byte[], int, int> GetIntConverter(this Accessor accessor)
        {
            if (accessor.ComponentType == ComponentType.UNSIGNED_INT)
            {
                Debug.Assert(accessor.Type == AttributeType.SCALAR);
                if (accessor.Max != null)
                {
                    if (accessor.Max[0] > int.MaxValue)
                    {
                        throw new Exception("Data type is unsigned int and reported max is bigger than int max.");
                    }
                }
                else
                {
                    Debug.LogWarning($"{ComponentType.UNSIGNED_INT} could be too big to be packed into int");
                }
            }

            switch (accessor.ComponentType)
            {
                case ComponentType.BYTE:
                    return (x, y) => Convert.ToSByte(x[y]);
                case ComponentType.UNSIGNED_BYTE:
                    return (x, y) => x[y];
                case ComponentType.SHORT:
                    return (x, y) => x.GetShortElement(y);
                case ComponentType.UNSIGNED_SHORT:
                    return (x, y) => x.GetUShortElement(y);
                case ComponentType.UNSIGNED_INT:
                    return (x, y) => (int)x.GetUIntElement(y);
                case ComponentType.FLOAT:
                    return (x, y) => (int)x.GetFloatElement(y);
                default:
                    throw new ArgumentOutOfRangeException(nameof(accessor.ComponentType));
            }
        }

        public static uint GetComponentSize(this Accessor accessor)
        {
            switch (accessor.ComponentType)
            {
                case ComponentType.BYTE:
                    return 1;
                case ComponentType.UNSIGNED_BYTE:
                    return 1;
                case ComponentType.SHORT:
                    return 2;
                case ComponentType.UNSIGNED_SHORT:
                    return 2;
                case ComponentType.UNSIGNED_INT:
                    return 4;
                case ComponentType.FLOAT:
                    return 4;
                default:
                    throw new ArgumentOutOfRangeException(nameof(accessor.ComponentType));
            }
        }
    }
}