using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using UnityEngine.Scripting;

namespace Gru.GLTF.Schema
{
    /// <summary>
    /// A keyframe animation.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/animation.schema.json"/>
    /// </summary>
    public class Animation : GLTFChildOfRootProperty
    {
        [Preserve]
        public Animation() { }

#pragma warning disable CA1819 // Properties should not return arrays
        /// <summary>
        /// An array of channels, each of which targets an animation's sampler at a node's property.
        /// Different channels of the same animation can't have equal targets.
        /// </summary>
        [JsonProperty(PropertyName = "channels", Required = Required.Always)]
        public AnimationChannel[] Channels
        {
            get => channels;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Channels)} must be at least one");
                }
                channels = value;
            }
        }

        /// <summary>
        /// An array of samplers that combines input and output accessors 
        /// with an interpolation algorithm to define a keyframe graph (but not its target).
        /// </summary>
        [JsonProperty(PropertyName = "samplers", Required = Required.Always)]
        public AnimationSampler[] Samplers
        {
            get => samplers;
            set
            {
                if (value != null && value.Length < 1)
                {
                    throw new Exception($"Length of {nameof(Samplers)} must be at least one");
                }
                samplers = value;
            }
        }
#pragma warning restore CA1819 // Properties should not return arrays


        private AnimationChannel[] channels;
        private AnimationSampler[] samplers;

        public override bool Validate()
        {
            return Channels != null && Samplers != null;
        }
    }

    /// <summary>
    /// Targets an animation's sampler at a node's property.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/animation.channel.schema.json"/>
    /// </summary>
    public class AnimationChannel : GLTFProperty
    {
        [Preserve]
        public AnimationChannel() { }

        /// <summary>
        /// The index of a sampler in this animation used to compute the value for the target,
        /// e.g., a node's translation, rotation, or scale (TRS).
        /// </summary>
        [JsonProperty(PropertyName = "sampler", Required = Required.Always)]
        public GLTFId Sampler { get; set; }

        /// <summary>
        /// The index of the node and TRS property to target.
        /// </summary>
        [JsonProperty(PropertyName = "target", Required = Required.Always)]
        public AnimationChannelTarget Target { get; set; }


        public override bool Validate()
        {
            return Sampler != null && Target != null;
        }
    }

    /// <summary>
    /// The index of the node and TRS property that an animation channel targets.
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/animation.channel.target.schema.json"/>
    /// </summary>
    public class AnimationChannelTarget : GLTFProperty
    {
        [Preserve]
        public AnimationChannelTarget() { }

        /// <summary>
        /// The index of the node to target.
        /// </summary>
        [JsonProperty(PropertyName = "node")]
        public GLTFId Node { get; set; }

        /// <summary>
        /// The name of the node's TRS property to modify, or the \"weights\" of the Morph Targets it instantiates.
        /// For the \"translation\" property, the values that are provided by the samplerare the translation along the x, y, and z axes. 
        /// For the \"rotation\" property, the values are a quaternion in the order (x, y, z, w), where w is the scalar. 
        /// For the \"scale\" property, the values are the scaling factors along the x, y, and z axes.
        /// </summary>
        [JsonProperty(PropertyName = "path", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public AnimationChannelPath Path { get; set; }
    }

    /// <summary>
    /// Combines input and output accessors with an interpolation algorithm to define a keyframe graph (but not its target).
    /// <see href="https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/schema/animation.sampler.schema.json"/>
    /// </summary>
    public class AnimationSampler : GLTFProperty
    {
        [Preserve]
        public AnimationSampler()
        {
            Interpolation = InterpolationType.LINEAR;
        }

        /// <summary>
        /// The index of an accessor containing keyframe input values, e.g., time.
        /// That accessor must have componentType `FLOAT`.
        /// The values represent time in seconds with `time[0] >= 0.0`, and strictly increasing values, i.e., `time[n + 1] > time[n]`.
        /// </summary>
        [JsonProperty(PropertyName = "input", Required = Required.Always)]
        public GLTFId Input { get; set; }

        /// <summary>
        /// Interpolation algorithm.
        /// </summary>
        [JsonProperty(PropertyName = "interpolation")]
        [JsonConverter(typeof(StringEnumConverter))]
        public InterpolationType Interpolation { get; set; }

        /// <summary>
        /// The index of an accessor containing keyframe output values.
        /// When targeting translation or scale paths, the `accessor.componentType` of the output values must be `FLOAT`.
        /// When targeting rotation or morph weights, the `accessor.componentType` of the output values must be `FLOAT` or normalized integer.
        /// For weights, each output element stores `SCALAR` values with a count equal to the number of morph targets.
        /// </summary>
        [JsonProperty(PropertyName = "output", Required = Required.Always)]
        public GLTFId Output { get; set; }

        public override bool Validate()
        {
            return Input != null && Output != null;
        }
    }

    public enum AnimationChannelPath
    {
        translation,
        rotation,
        scale,
        weights
    }

    public enum InterpolationType
    {
        /// <summary>
        /// The animated values are linearly interpolated between keyframes.
        /// When targeting a rotation, spherical linear interpolation (slerp) should be used to interpolate quaternions.
        /// The number output of elements must equal the number of input elements.
        /// </summary>
        LINEAR,
        /// <summary>
        /// The animated values remain constant to the output of the first keyframe,until the next keyframe.
        /// The number of output elements must equal the number of input elements.
        /// </summary>
        STEP,
        /// <summary>
        /// The animation's interpolation is computed using a cubic spline with specified tangents.
        /// The number of output elements must equal three times the number of input elements.
        /// For each input element, the output stores three elements, an in-tangent, a spline vertex, and an out-tangent.
        /// There must be at least two keyframes when using this interpolation.
        /// </summary>
        CUBICSPLINE
    }
}