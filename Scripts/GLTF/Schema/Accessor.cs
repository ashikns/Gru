using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// A typed view into a bufferView. A bufferView contains raw binary data.
    /// An accessor provides a typed view into a bufferView or a subset of a bufferView.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/accessor.schema.json"/>
    /// </summary>
    public class Accessor : GLTFChildOfRootProperty
    {
        [Preserve]
        public Accessor()
        {
            ByteOffset = 0;
            Normalized = false;
        }

        /// <summary>
        /// The index of the bufferView. When not defined, accessor must be initialized with zeros;
        /// `sparse` property or extensions could override zeros with actual values.
        /// </summary>
        [JsonProperty(PropertyName = "bufferView")]
        public GLTFId BufferView { get; set; }

        /// <summary>
        /// The offset relative to the start of the bufferView in bytes.
        /// This must be a multiple of the size of the component datatype.
        /// </summary>
        [JsonProperty(PropertyName = "byteOffset")]
        public int ByteOffset
        {
            get => byteOffset;
            set
            {
                if (value < 0) { throw new Exception($"{nameof(ByteOffset)} cannot be less than zero"); }
                byteOffset = value;
            }
        }
        /// <summary>
        /// The datatype of components in the attribute.
        /// 5125 (UNSIGNED_INT) is only allowed when the accessor contains indices,
        /// i.e., the accessor is only referenced by `primitive.indices`.
        /// </summary>
        [JsonProperty(PropertyName = "componentType", Required = Required.Always)]
        public ComponentType ComponentType { get; set; }

        /// <summary>
        /// Specifies whether integer data values should be normalized (`true`) to [0, 1] (for unsigned types) 
        /// or [-1, 1] (for signed types), or converted directly (`false`) when they are accessed.
        /// E.g, do (unsigned) bytes 0 and 255 mean 0.0 and 255.0 (not normalized) or 0.0 and 1.0 (normalized).
        /// This property is defined only for accessors that contain vertex attributes or animation output data.
        /// </summary>
        [JsonProperty(PropertyName = "normalized")]
        public bool Normalized { get; set; }

        /// <summary>
        /// The number of attributes referenced by this accessor, 
        /// not to be confused with the number of bytes or number of components.
        /// </summary>
        [JsonProperty(PropertyName = "count", Required = Required.Always)]
        public int Count
        {
            get => count;
            set
            {
                if (value < 1) { throw new Exception($"{nameof(Count)} cannot be less than one"); }
                count = value;
            }
        }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public AttributeType Type { get; set; }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// Maximum value of each component in this attribute.
        /// Array elements must be treated as having the same data type as accessor's `componentType`.
        /// Both min and max arrays have the same length.
        /// The length is determined by the value of the type property; it can be 1, 2, 3, 4, 9, or 16.
        /// `normalized` property has no effect on array values: they always correspond to the actual values stored in the buffer.
        /// When accessor is sparse, this property must contain max values of accessor data with sparse substitution applied.
        /// </summary>
        [JsonProperty(PropertyName = "max")]
        public float[] Max
        {
            get => max;
            set
            {
                if (value != null && (value.Length < 1 || value.Length > 16))
                {
                    throw new Exception($"Length of {nameof(Max)} should be between 1 and 16");
                }
                max = value;
            }
        }

        /// <summary>
        /// Minimum value of each component in this attribute.
        /// More details: <seealso cref="max"/>
        /// </summary>
        [JsonProperty(PropertyName = "min")]
        public float[] Min
        {
            get => min;
            set
            {
                if (value != null && (value.Length < 1 || value.Length > 16))
                {
                    throw new Exception($"Length of {nameof(Min)} should be between 1 and 16");
                }
                min = value;
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays

        /// <summary>
        /// Sparse storage of attributes that deviate from their initialization value.
        /// </summary>
        [JsonProperty(PropertyName = "sparse")]
        public AccessorSparse Sparse { get; set; }


        private int byteOffset;
        private int count;
        private float[] max;
        private float[] min;

        public override bool Validate()
        {
            // ByteOffset -> BufferView
            return (ByteOffset == 0 || BufferView != null)
                // Required: Count
                && count >= 1;
        }
    }

    /// <summary>
    /// Sparse storage of attributes that deviate from their initialization value.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/accessor.sparse.schema.json"/>
    /// </summary>
    public class AccessorSparse : GLTFProperty
    {
        [Preserve]
        public AccessorSparse() { }

        /// <summary>
        /// The number of attributes encoded in this sparse accessor.
        /// </summary>
        [JsonProperty(PropertyName = "count", Required = Required.Always)]
        public int Count
        {
            get => count;
            set
            {
                if (value < 1) { throw new Exception($"{nameof(Count)} cannot be less than one"); }
                count = value;
            }
        }

        /// <summary>
        /// Index array of size `count` that points to those accessor attributes 
        /// that deviate from their initialization value. Indices must strictly increase.
        /// </summary>
        [JsonProperty(PropertyName = "indices", Required = Required.Always)]
        public AccessorSparseIndices Indices { get; set; }

        /// <summary>
        /// Array of size `count` times number of components, 
        /// storing the displaced accessor attributes pointed by `indices`. 
        /// Substituted values must have the same `componentType` and number of components as the base accessor.
        /// </summary>
        [JsonProperty(PropertyName = "values", Required = Required.Always)]
        public AccessorSparseValues Values { get; set; }


        private int count;

        public override bool Validate()
        {
            return count >= 1 && Indices != null && Values != null;
        }
    }

    /// <summary>
    /// Indices of those attributes that deviate from their initialization value.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/accessor.sparse.indices.schema.json"/>
    /// </summary>
    public class AccessorSparseIndices : GLTFProperty
    {
        [Preserve]
        public AccessorSparseIndices()
        {
            ByteOffset = 0;
        }

        /// <summary>
        /// The index of the bufferView with sparse indices.
        /// Referenced bufferView can't have ARRAY_BUFFER or ELEMENT_ARRAY_BUFFER target.
        /// </summary>
        [JsonProperty(PropertyName = "bufferView", Required = Required.Always)]
        public GLTFId BufferView { get; set; }

        /// <summary>
        /// The offset relative to the start of the bufferView in bytes. Must be aligned.
        /// </summary>
        [JsonProperty(PropertyName = "byteOffset")]
        public int ByteOffset
        {
            get => byteOffset;
            set
            {
                if (value < 0) { throw new Exception($"{nameof(ByteOffset)} cannot be less than zero"); }
                byteOffset = value;
            }
        }

        /// <summary>
        /// The indices data type.  
        /// Valid values correspond to: `5121` (UNSIGNED_BYTE), `5123` (UNSIGNED_SHORT), `5125` (UNSIGNED_INT).
        /// </summary>
        [JsonProperty(PropertyName = "componentType", Required = Required.Always)]
        public ComponentType ComponentType { get; set; }


        private int byteOffset;

        public override bool Validate()
        {
            return BufferView != null;
        }
    }

    /// <summary>
    /// Array of size `accessor.sparse.count` times number of components 
    /// storing the displaced accessor attributes pointed by `accessor.sparse.indices`.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/accessor.sparse.values.schema.json"/>
    /// </summary>
    public class AccessorSparseValues : GLTFProperty
    {
        [Preserve]
        public AccessorSparseValues()
        {
            ByteOffset = 0;
        }

        /// <summary>
        /// The index of the bufferView with sparse values. 
        /// Referenced bufferView can't have ARRAY_BUFFER or ELEMENT_ARRAY_BUFFER target.
        /// </summary>
        [JsonProperty(PropertyName = "bufferView", Required = Required.Always)]
        public GLTFId BufferView { get; set; }

        /// <summary>
        /// The offset relative to the start of the bufferView in bytes. Must be aligned.
        /// </summary>
        [JsonProperty(PropertyName = "byteOffset")]
        public int ByteOffset
        {
            get => byteOffset;
            set
            {
                if (value < 0) { throw new Exception($"{nameof(ByteOffset)} cannot be less than zero"); }
                byteOffset = value;
            }
        }


        private int byteOffset;

        public override bool Validate()
        {
            return BufferView != null;
        }
    }

    public enum ComponentType : int
    {
        BYTE = 5120,
        UNSIGNED_BYTE = 5121,
        SHORT = 5122,
        UNSIGNED_SHORT = 5123,
        UNSIGNED_INT = 5125,
        FLOAT = 5126
    }

    public enum AttributeType
    {
        SCALAR,
        VEC2,
        VEC3,
        VEC4,
        MAT2,
        MAT3,
        MAT4
    }
}