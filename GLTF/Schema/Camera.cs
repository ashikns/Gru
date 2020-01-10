using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// A camera's projection. A node can reference a camera to apply a transform to place the camera in the scene.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/camera.schema.json"/>
    /// </summary>
    public class Camera : GLTFChildOfRootProperty
    {
        [Preserve]
        public Camera() { }

        /// <summary>
        /// An orthographic camera containing properties to create an orthographic projection matrix.
        /// </summary>
        [JsonProperty(PropertyName = "orthographic")]
        public CameraOrthographic Orthographic { get; set; }

        /// <summary>
        /// A perspective camera containing properties to create a perspective projection matrix.
        /// </summary>
        [JsonProperty(PropertyName = "perspective")]
        public CameraPerspective Perspective { get; set; }

        /// <summary>
        /// Specifies if the camera uses a perspective or orthographic projection.
        /// Based on this, either the camera's `perspective` or `orthographic` property will be defined.
        /// </summary>
        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public CameraType Type { get; set; }


        public override bool Validate()
        {
            return (Orthographic != null && Perspective == null)
                || (Orthographic == null && Perspective != null);
        }
    }

    /// <summary>
    /// An orthographic camera containing properties to create an orthographic projection matrix.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/camera.orthographic.schema.json"/>
    /// </summary>
    public class CameraOrthographic : GLTFProperty
    {
        [Preserve]
        public CameraOrthographic() { }

        /// <summary>
        /// The floating-point horizontal magnification of the view. Must not be zero.
        /// </summary>
        [JsonProperty(PropertyName = "xmag", Required = Required.Always)]
        public float Xmag
        {
            get => xmag;
            set
            {
                if (value == 0.0f) { throw new Exception($"{nameof(Xmag)} cannot be zero."); }
                xmag = value;
            }
        }

        /// <summary>
        /// The floating-point vertical magnification of the view. Must not be zero.
        /// </summary>
        [JsonProperty(PropertyName = "ymag", Required = Required.Always)]
        public float Ymag
        {
            get => ymag;
            set
            {
                if (value == 0.0f) { throw new Exception($"{nameof(Ymag)} cannot be zero."); }
                ymag = value;
            }
        }

        /// <summary>
        /// The floating-point distance to the far clipping plane. `zfar` must be greater than `znear`.
        /// </summary>
        [JsonProperty(PropertyName = "zfar", Required = Required.Always)]
        public float Zfar
        {
            get => zfar;
            set
            {
                if (value <= 0.0f || value <= Znear)
                {
                    throw new Exception($"{nameof(Zfar)} must be greater than zero and {nameof(Znear)}.");
                }
                zfar = value;
            }
        }

        /// <summary>
        /// The floating-point distance to the near clipping plane.
        /// </summary>
        [JsonProperty(PropertyName = "znear", Required = Required.Always)]
        public float Znear
        {
            get => znear;
            set
            {
                if (value < 0.0f || value >= Zfar)
                {
                    throw new Exception($"{nameof(Znear)} must be less than {nameof(Zfar)} but cannot be less than zero.");
                }
                znear = value;
            }
        }


        private float xmag;
        private float ymag;
        private float zfar;
        private float znear;

        public override bool Validate()
        {
            return Xmag != 0 && Ymag != 0 && Zfar > 0;
        }
    }

    /// <summary>
    /// A perspective camera containing properties to create a perspective projection matrix.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/camera.perspective.schema.json"/>
    /// </summary>
    public class CameraPerspective : GLTFProperty
    {
        [Preserve]
        public CameraPerspective() { }

        /// <summary>
        /// The floating-point aspect ratio of the field of view.
        /// When this is undefined, the aspect ratio of the canvas is used.
        /// </summary>
        [JsonProperty(PropertyName = "aspectRatio")]
        public float AspectRatio
        {
            get => aspectRatio;
            set
            {
                if (value <= 0.0f) { throw new Exception($"{nameof(AspectRatio)} must be greater than zero"); }
                aspectRatio = value;
            }
        }

        /// <summary>
        /// The floating-point vertical field of view in radians.
        /// </summary>
        [JsonProperty(PropertyName = "yfov", Required = Required.Always)]
        public float Yfov
        {
            get => yfov;
            set
            {
                if (value <= 0.0f) { throw new Exception($"{nameof(Yfov)} must be greater than zero"); }
                yfov = value;
            }
        }

        /// <summary>
        /// The floating-point distance to the far clipping plane.
        /// When defined, `zfar` must be greater than `znear`.
        /// If `zfar` is undefined, runtime must use infinite projection matrix.
        /// </summary>
        [JsonProperty(PropertyName = "zfar")]
        public float Zfar
        {
            get => zfar;
            set
            {
                if (value <= 0.0f || value <= Znear)
                {
                    throw new Exception($"{nameof(Zfar)} must be greater than zero and {nameof(Znear)}.");
                }
                zfar = value;
            }
        }

        /// <summary>
        /// The floating-point distance to the near clipping plane.
        /// </summary>
        [JsonProperty(PropertyName = "znear", Required = Required.Always)]
        public float Znear
        {
            get => znear;
            set
            {
                if (value <= 0.0f) { throw new Exception($"{nameof(Znear)} must be greater than zero"); }
                znear = value;
            }
        }


        private float aspectRatio;
        private float yfov;
        private float zfar;
        private float znear;

        public override bool Validate()
        {
            return Yfov > 0 && Znear > 0;
        }
    }

    public enum CameraType
    {
        perspective,
        orthographic
    }
}