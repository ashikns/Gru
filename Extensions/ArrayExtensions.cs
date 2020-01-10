using UnityEngine;

namespace Gru.Extensions
{
    public static class ArrayExtensions
    {
        public static Color ToUnityColor(this float[] colorVals)
        {
            if (colorVals.Length == 4)
            {
                return new Color(colorVals[0], colorVals[1], colorVals[2], colorVals[3]);
            }
            else if (colorVals.Length == 3)
            {
                return new Color(colorVals[0], colorVals[1], colorVals[2]);
            }
            else
            {
                throw new System.Exception("Array size does not match expected");
            }
        }

        public static Vector3 ToUnityVector3Raw(this float[] vectorVals)
        {
            return new Vector3(vectorVals[0], vectorVals[1], vectorVals[2]);
        }

        public static Vector3 ToUnityVector3Convert(this float[] vectorVals)
        {
            return new Vector3(-vectorVals[0], vectorVals[1], vectorVals[2]);
        }

        public static Quaternion ToUnityQuaternionRaw(this float[] quaternionVals)
        {
            return new Quaternion(quaternionVals[0], quaternionVals[1], quaternionVals[2], quaternionVals[3]);
        }

        public static Quaternion ToUnityQuaternionConvert(this float[] quaternionVals)
        {
            return new Quaternion(quaternionVals[0], -quaternionVals[1], -quaternionVals[2], quaternionVals[3]);
        }

        public static void ToUnityTRSConvert(this float[] matrix, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            Debug.Assert(matrix.Length == 16);

            /*
             * GLTF stores matrices in column major order.
             * 
             * Matrix when converted to Unity coord space will look like (after accessing in column major order):
             *   m00 -m01 -m02 -m03
             *  -m10  m11  m12  m13
             *  -m20  m21  m22  m23
             *  -m30  m31  m32  m33
             *  
             *  Instead of calculating this (unity) matrix, we're directly extracting the vector from gltf space matrix
             * */

            position = new Vector3(-matrix[12], matrix[13], matrix[14]);

            var c0 = new Vector3(matrix[0], -matrix[1], -matrix[2]);
            var c1 = new Vector3(-matrix[4], matrix[5], matrix[6]);
            var c2 = new Vector3(-matrix[8], matrix[9], matrix[10]);

            rotation = Quaternion.LookRotation(c2, c1);

            var mirror = Vector3.Dot(Vector3.Cross(c0, c1), c2) < 0.0f ? -1.0f : 1.0f;

            scale = new Vector3(c0.magnitude * mirror, c1.magnitude, c2.magnitude);
        }

        public static Matrix4x4 ToUnityMatrixConvert(this float[] matrix)
        {
            Debug.Assert(matrix.Length == 16);

            /*
             * GLTF stores matrices in column major order.
             * 
             * Matrix when converted to Unity coord space will look like (after accessing in column major order):
             *   m00 -m01 -m02 -m03
             *  -m10  m11  m12  m13
             *  -m20  m21  m22  m23
             *  -m30  m31  m32  m33
             * */

            return new Matrix4x4(
                new Vector4(matrix[0], -matrix[1], -matrix[2], -matrix[3]),
                new Vector4(-matrix[4], matrix[5], matrix[6], matrix[7]),
                new Vector4(-matrix[8], matrix[9], matrix[10], matrix[11]),
                new Vector4(-matrix[12], matrix[13], matrix[14], matrix[15]));
        }

        public static unsafe float GetFloatElement(this byte[] buffer, int byteOffset)
        {
            fixed (byte* offsetBuffer = &buffer[byteOffset])
            {
                return *(float*)offsetBuffer;
            }
        }

        public static unsafe short GetShortElement(this byte[] buffer, int byteOffset)
        {
            fixed (byte* offsetBuffer = &buffer[byteOffset])
            {
                return *(short*)offsetBuffer;
            }
        }

        public static unsafe ushort GetUShortElement(this byte[] buffer, int byteOffset)
        {
            fixed (byte* offsetBuffer = &buffer[byteOffset])
            {
                return *(ushort*)offsetBuffer;
            }
        }

        public static unsafe uint GetUIntElement(this byte[] buffer, int byteOffset)
        {
            fixed (byte* offsetBuffer = &buffer[byteOffset])
            {
                return *(uint*)offsetBuffer;
            }
        }
    }
}