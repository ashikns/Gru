using Gru.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Gru.Importers
{
    public class AnimationImporter
    {
        private static readonly string[] PositionPropertyNames = new string[] { "localPosition.x", "localPosition.y", "localPosition.z" };
        private static readonly string[] RotationPropertyNames = new string[] { "localRotation.x", "localRotation.y", "localRotation.z", "localRotation.w" };
        private static readonly string[] ScalePropertyNames = new string[] { "localScale.x", "localScale.y", "localScale.z" };

        private class AnimationData
        {
            public Keyframe[][] Keyframes { get; set; }
            public string[] ChannelNames { get; set; }
        }

        private readonly IList<GLTF.Schema.Accessor> _accessorSchemas;
        private readonly BufferImporter _bufferImporter;
        private readonly NodeImporter _nodeImporter;

        public AnimationImporter(
            IList<GLTF.Schema.Accessor> accessorSchemas,
            BufferImporter bufferImporter,
            NodeImporter nodeImporter)
        {
            _accessorSchemas = accessorSchemas;
            _bufferImporter = bufferImporter;
            _nodeImporter = nodeImporter;
        }

        // Must be run on main thread
        public async Task<AnimationClip> ConstructAnimationClipAsync(GLTF.Schema.Animation animationSchema, Transform root)
        {
            var animationClip = new AnimationClip
            {
                name = animationSchema.Name,
                legacy = true
            };

            var animationDatas = await Task.WhenAll(BuildAnimations(animationSchema));

            for (int i = 0; i < animationSchema.Channels.Length; i++)
            {
                var animationData = animationDatas[i];
                var channel = animationSchema.Channels[i];
                var node = await _nodeImporter.GetNodeAsync(channel.Target.Node);
                string animationPath = RelativePathFrom(node.transform, root);

                for (int ci = 0; ci < animationData.ChannelNames.Length; ci++)
                {
                    // copy all key frames data to animation curve and add it to the clip
                    AnimationCurve curve = new AnimationCurve
                    {
                        keys = animationData.Keyframes[ci]
                    };

                    animationClip.SetCurve(animationPath, typeof(Transform), animationData.ChannelNames[ci], curve);
                }
            }

            return animationClip;
        }

        private IEnumerable<Task<AnimationData>> BuildAnimations(GLTF.Schema.Animation animationSchema)
        {
            for (int i = 0; i < animationSchema.Channels.Length; i++)
            {
                var channel = animationSchema.Channels[i];
                var sampler = animationSchema.Samplers[channel.Sampler.Key];
                yield return BuildAnimationData(channel, sampler, _accessorSchemas, _bufferImporter);
            }
        }

        private static async Task<float[]> ReadTimeValsAsync(
            GLTF.Schema.AnimationSampler sampler,
            IList<GLTF.Schema.Accessor> accessors,
            BufferImporter bufferImporter)
        {
            // Input of sampler should be linear, increasing, and min max defined.
            // They may not be equally spaced however so it still has to be read from buffer.
            var input = accessors[sampler.Input.Key];
            var t_beg = input.Min[0];
            var t_end = input.Max[0];
            var timeVals = new float[input.Count];

            if (input.BufferView == null)
            {
                throw new ArgumentNullException(nameof(input.BufferView));
            }

            var inputBuffer = await bufferImporter.GetBufferViewAsync(input.BufferView);
            var inputConverter = input.GetFloatConverter();
            var offset = inputBuffer.Data.Offset + input.ByteOffset;
            var stride = inputBuffer.Stride > 0 ? inputBuffer.Stride : input.GetComponentSize();

            for (int i = 0; i < input.Count; i++)
            {
                timeVals[i] = inputConverter(inputBuffer.Data.Array, (int)(offset + i * stride));
            }

            if (!Mathf.Approximately(timeVals[0], t_beg) || !Mathf.Approximately(timeVals[input.Count - 1], t_end))
            {
                Debug.LogWarning("Read timevals does not match min and max defined.");

                // Populate with interpolated vals
                for (int i = 0; i < input.Count; i++)
                {
                    timeVals[i] = t_beg + (t_end - t_beg) / i;
                }
            }

            return timeVals;
        }

        private static async Task<AnimationData> BuildAnimationData(
            GLTF.Schema.AnimationChannel channel,
            GLTF.Schema.AnimationSampler sampler,
            IList<GLTF.Schema.Accessor> accessors,
            BufferImporter bufferImporter)
        {
            if (channel.Target.Node == null)
            {
                return null;
            }

            var animationData = new AnimationData();
            Action<float[], byte[], int> animationValueReader;

            switch (channel.Target.Path)
            {
                case GLTF.Schema.AnimationChannelPath.translation:
                    animationValueReader = ReadPositionConvert;
                    animationData.ChannelNames = PositionPropertyNames;
                    break;
                case GLTF.Schema.AnimationChannelPath.rotation:
                    animationValueReader = ReadRotationConvert;
                    animationData.ChannelNames = RotationPropertyNames;
                    break;
                case GLTF.Schema.AnimationChannelPath.scale:
                    animationValueReader = ReadScaleConvert;
                    animationData.ChannelNames = ScalePropertyNames;
                    break;
                case GLTF.Schema.AnimationChannelPath.weights:
                    Debug.LogError("Weights animation path is not supported.");
                    return null;
                default:
                    throw new ArgumentOutOfRangeException(channel.Target.Path.ToString());
            }

            var timeVals = await ReadTimeValsAsync(sampler, accessors, bufferImporter);
            if (timeVals.Length == 0)
            {
                return null;
            }

            var frameCount = timeVals.Length;
            int channelCount = animationData.ChannelNames.Length;

            animationData.Keyframes = new Keyframe[channelCount][];
            var keyframes = new List<Keyframe>[channelCount];

            for (int ci = 0; ci < channelCount; ci++)
            {
                keyframes[ci] = new List<Keyframe>(frameCount);
            }

            var accessor = accessors[sampler.Output.Key];
            var buffer = await bufferImporter.GetBufferViewAsync(accessor.BufferView);
            var offset = buffer.Data.Offset + accessor.ByteOffset;
            var componentSize = accessor.GetComponentSize();
            var stride = buffer.Stride > 0 ? buffer.Stride : componentSize * channelCount;

            var prevTime = timeVals[0] - 1;

            switch (sampler.Interpolation)
            {
                case GLTF.Schema.InterpolationType.LINEAR:
                    {
                        float[] cur = new float[channelCount];
                        float[] prev = new float[channelCount];
                        float[] next = new float[channelCount];

                        animationValueReader(cur, buffer.Data.Array, offset);
                        animationValueReader(prev, buffer.Data.Array, offset);

                        for (var i = 0; i < frameCount - 1; ++i)
                        {
                            var time = timeVals[i];
                            if (time == prevTime) { continue; }

                            var timeDelta = time - prevTime;
                            animationValueReader(next, buffer.Data.Array, (int)(offset + i * stride));

                            for (var ci = 0; ci < channelCount; ++ci)
                            {
                                keyframes[ci].Add(new Keyframe(
                                    time, cur[ci], (cur[ci] - prev[ci]) / timeDelta, (next[ci] - cur[ci]) / timeDelta));

                                prev[ci] = cur[ci];
                                cur[ci] = next[ci];
                            }

                            prevTime = time;
                        }

                        var endTime = timeVals[frameCount - 1];
                        if (endTime == prevTime) { break; }
                        var endTimeDelta = endTime - prevTime;

                        for (var ci = 0; ci < channelCount; ++ci)
                        {
                            keyframes[ci].Add(new Keyframe(endTime, cur[ci], (cur[ci] - prev[ci]) / endTimeDelta, 0));
                        }
                        break;
                    }
                case GLTF.Schema.InterpolationType.STEP:
                    {
                        float[] values = new float[channelCount];

                        for (var i = 0; i < frameCount; ++i)
                        {
                            var time = timeVals[i];
                            if (time == prevTime) { continue; }

                            animationValueReader(values, buffer.Data.Array, (int)(offset + i * stride));

                            for (var ci = 0; ci < channelCount; ++ci)
                            {
                                keyframes[ci].Add(new Keyframe(time, values[ci], float.PositiveInfinity, float.PositiveInfinity));
                            }

                            prevTime = time;
                        }
                        break;
                    }
                case GLTF.Schema.InterpolationType.CUBICSPLINE:
                    {
                        float[] inTangents = new float[channelCount];
                        float[] values = new float[channelCount];
                        float[] outTangents = new float[channelCount];

                        for (var i = 0; i < frameCount; ++i)
                        {
                            var time = timeVals[i];
                            if (time == prevTime) { continue; }

                            // For cubic spline, the output will contain 3 values per keyframe; inTangent, dataPoint, and outTangent.
                            // https://github.com/KhronosGroup/glTF/blob/master/specification/2.0/README.md#appendix-c-spline-interpolation

                            var cubicIndex = i * 3;
                            animationValueReader(inTangents, buffer.Data.Array, (int)(offset + cubicIndex * stride));
                            animationValueReader(values, buffer.Data.Array, (int)(offset + (cubicIndex + 1) * stride));
                            animationValueReader(outTangents, buffer.Data.Array, (int)(offset + (cubicIndex + 2) * stride));

                            for (var ci = 0; ci < channelCount; ++ci)
                            {
                                keyframes[ci].Add(new Keyframe(time, values[ci], inTangents[ci], outTangents[ci]));
                            }

                            prevTime = time;
                        }
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(sampler.Interpolation));
            }

            for (int ci = 0; ci < channelCount; ci++)
            {
                animationData.Keyframes[ci] = keyframes[ci].ToArray();
            }

            return animationData;
        }

        private static void ReadPositionConvert(float[] values, byte[] buffer, int offset)
        {
            values[0] = -buffer.GetFloatElement(offset);
            values[1] = buffer.GetFloatElement(offset + sizeof(float));
            values[2] = buffer.GetFloatElement(offset + sizeof(float) * 2);
        }

        private static void ReadRotationConvert(float[] values, byte[] buffer, int offset)
        {
            values[0] = buffer.GetFloatElement(offset);
            values[1] = -buffer.GetFloatElement(offset + sizeof(float));
            values[2] = -buffer.GetFloatElement(offset + sizeof(float) * 2);
            values[3] = buffer.GetFloatElement(offset + sizeof(float) * 3);
        }

        private static void ReadScaleConvert(float[] values, byte[] buffer, int offset)
        {
            values[0] = buffer.GetFloatElement(offset);
            values[1] = buffer.GetFloatElement(offset + sizeof(float));
            values[2] = buffer.GetFloatElement(offset + sizeof(float) * 2);
        }

        private static string RelativePathFrom(Transform self, Transform root)
        {
            var path = new List<string>();
            for (var current = self; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return string.Join("/", path);
                }

                path.Insert(0, current.name);
            }

            throw new Exception("no RelativePath");
        }
    }
}