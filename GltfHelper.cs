using Framework;
using glTFLoader;
using glTFLoader.Schema;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Example
{
	public static class GltfHelper
	{
		public static (Vector3 min, Vector3 max) CalculateSceneBounds(this Gltf gltf, IReadOnlyList<Matrix4> localNodeTransforms)
		{
			var min = Vector3.One * float.MaxValue;
			var max = Vector3.One * float.MinValue;
			foreach (var (globalTransform, nodeId) in gltf.TraverseNodeTransforms(localNodeTransforms))
			{
				var node = gltf.Nodes[nodeId];
				if (gltf.TryCalculateGlobalBounds(node, globalTransform, out var nodeMin, out var nodeMax))
				{
					min = Vector3.ComponentMin(min, nodeMin);
					max = Vector3.ComponentMax(max, nodeMax);
				}
			}
			return (min, max);
		}

		public static List<Action<float>> CreateAnimationControllers(this Gltf gltf, IReadOnlyList<byte[]> byteBuffers, IList<Matrix4> localTransforms)
		{
			var animations = gltf.Animations;
			var animationController = new List<Action<float>>();
			if (animations is null) return animationController;
			if (byteBuffers is null) throw new ArgumentNullException(nameof(byteBuffers));
			var accessorBuffers = new Dictionary<int, Array>();

			TYPE[]? GetBuffer<TYPE>(int accessorId) where TYPE : struct
			{
				if (accessorBuffers.TryGetValue(accessorId, out var buf))
				{
					return buf as TYPE[];
				}
				var accessor = gltf.Accessors[accessorId];
				var view = gltf.BufferViews[accessor.BufferView ?? 0];
				var buffer = byteBuffers[view.Buffer].FromByteArray<TYPE>(view.ByteOffset + accessor.ByteOffset, accessor.Count);
				accessorBuffers.Add(accessorId, buffer);
				return buffer;
			}

			foreach (var animation in animations)
			{
				void AddAnimationControllerTranslate(AnimationChannel channel)
				{
					var sampler = animation.Samplers[channel.Sampler];
					var bufferTimes = GetBuffer<float>(sampler.Input);
					var bufferValues = GetBuffer<Vector3>(sampler.Output);
					if (bufferTimes is null || bufferValues is null) return;
					var node = gltf.Nodes[channel.Target.Node ?? 0];
					void Interpolator(float time)
					{
						var (lower, upper) = bufferTimes.FindExistingRange(time);
						var weight = time.Normalize(bufferTimes[lower], bufferTimes[upper]);
						var interpolated = Vector3.Lerp(bufferValues[lower], bufferValues[upper], weight);
						node.Translation = interpolated.ToArray();
						localTransforms[channel.Target.Node ?? 0] = node.ExtractLocalTransform();
						//Debug.WriteLine($"{sampler.Output}: translate {interpolated}");
					}
					animationController.Add(Interpolator);
				}

				void AddAnimationControllerRotation(AnimationChannel channel)
				{
					var sampler = animation.Samplers[channel.Sampler];
					var bufferTimes = GetBuffer<float>(sampler.Input);
					var bufferValues = GetBuffer<Quaternion>(sampler.Output);
					if (bufferTimes is null || bufferValues is null) return;
					bufferValues = bufferValues.Select(value => value.Normalized()).ToArray(); //busterDrone has some not normalized quaternions
					var node = gltf.Nodes[channel.Target.Node ?? 0];
					void Interpolator(float time)
					{
						var (lower, upper) = bufferTimes.FindExistingRange(time);
						var weight = time.Normalize(bufferTimes[lower], bufferTimes[upper]);
						var interpolated = Quaternion.Slerp(bufferValues[lower], bufferValues[upper], weight);
						node.Rotation = interpolated.ToArray();
						localTransforms[channel.Target.Node ?? 0] = node.ExtractLocalTransform();
						//Debug.WriteLine($"{sampler.Output}: rotate {interpolated}");
					}
					animationController.Add(Interpolator);
				}

				void AddAnimationControllerScale(AnimationChannel channel)
				{
					var sampler = animation.Samplers[channel.Sampler];
					var bufferTimes = GetBuffer<float>(sampler.Input);
					var bufferValues = GetBuffer<Vector3>(sampler.Output);
					var node = gltf.Nodes[channel.Target.Node ?? 0];
					if (bufferTimes is null || bufferValues is null) return;
					void Interpolator(float time)
					{
						var (lower, upper) = bufferTimes.FindExistingRange(time);
						var weight = time.Normalize(bufferTimes[lower], bufferTimes[upper]);
						var interpolated = Vector3.Lerp(bufferValues[lower], bufferValues[upper], weight);
						node.Scale = interpolated.ToArray();
						localTransforms[channel.Target.Node ?? 0] = node.ExtractLocalTransform();
						//Debug.WriteLine($"{sampler.Output}: scale {interpolated}");
					}
					animationController.Add(Interpolator);
				}

				foreach (var channel in animation.Channels)
				{
					if (!channel.Target.Node.HasValue) continue;
					switch (channel.Target.Path)
					{
						case AnimationChannelTarget.PathEnum.rotation:
							AddAnimationControllerRotation(channel);
							break;
						case AnimationChannelTarget.PathEnum.translation:
							AddAnimationControllerTranslate(channel);
							break;
						case AnimationChannelTarget.PathEnum.scale:
							AddAnimationControllerScale(channel);
							break;
						default:
							break;
					}
				}
			}
			return animationController;
		}

		public static List<byte[]> ExtractBufferViews(this Gltf gltf, IReadOnlyList<byte[]> byteBuffers)
		{
			var bufferViewBuffers = new List<byte[]>();
			foreach (var bufferView in gltf.BufferViews)
			{
				var destBuffer = new byte[bufferView.ByteLength];
				Array.Copy(byteBuffers[bufferView.Buffer], bufferView.ByteOffset, destBuffer, 0, bufferView.ByteLength);
				bufferViewBuffers.Add(destBuffer);
			}
			return bufferViewBuffers;
		}

		public static List<byte[]> ExtractByteBuffers(this Gltf gltf, Func<string, Stream> externalReferenceSolver)
		{
			byte[] ToBuffer(string name)
			{
				using var stream = externalReferenceSolver(name);
				return new BinaryReader(stream).ReadBytes((int)stream.Length);
			}
			var byteBuffers = new List<byte[]>();
			for (int i = 0; i < gltf.Buffers.Length; ++i)
			{
				var buffer = Interface.LoadBinaryBuffer(gltf, i, ToBuffer);
				byteBuffers.Add(buffer);
			}
			return byteBuffers;
		}

		public static Matrix4[] ExtractLocalTransforms(this Gltf gltf) => gltf.Nodes.Select(node => node.ExtractLocalTransform()).ToArray();

		public static Matrix4 ExtractLocalTransform(this Node node)
		{
			var translation = Matrix4.CreateTranslation(node.Translation[0], node.Translation[1], node.Translation[2]);
			var rotation = Matrix4.CreateFromQuaternion(new Quaternion(node.Rotation[0], node.Rotation[1], node.Rotation[2], node.Rotation[3]));
			var scale = Matrix4.CreateScale(node.Scale[0], node.Scale[1], node.Scale[2]);
			var m = node.Matrix;
			var transform = new Matrix4(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15]);
			//transform.Transpose();
			var trs = scale * rotation * translation;
#if DEBUG
			//if(Matrix4.Identity != trs && Matrix4.Identity != transform) Debugger.Break();
			//if(0 > transform.Determinant) Debugger.Break();
#endif
			return transform * trs; //order does not matter because one is identity
		}

		public static IEnumerable<(Matrix4 globalTransform, int nodeId)> TraverseNodeTransforms(this Gltf gltf, IReadOnlyList<Matrix4> localNodeTransforms)
		{
			foreach (var scene in gltf.Scenes)
			{
				foreach (var output in gltf.TraverseNodeTransforms(scene.Nodes, localNodeTransforms, Matrix4.Identity)) yield return output;
			}
		}

		public static IEnumerable<(Matrix4 globalTransform, int nodeId)> TraverseNodeTransforms(this Gltf gltf, IReadOnlyList<int> nodes, IReadOnlyList<Matrix4> localNodeTransforms, Matrix4 parentTransformation)
		{
			if (nodes is null) yield break;
			foreach (var nodeId in nodes)
			{
				var localTransform = localNodeTransforms[nodeId];
				var globalTransform = localTransform * parentTransformation;
				var node = gltf.Nodes[nodeId];
				yield return (globalTransform, nodeId);
				foreach (var output in gltf.TraverseNodeTransforms(node.Children, localNodeTransforms, globalTransform)) yield return output;
			}
		}

		public static bool TryCalculateLocalBounds(this Gltf gltf, in MeshPrimitive primitive, out Vector3 min, out Vector3 max)
		{
			if (primitive.Attributes.TryGetValue("POSITION", out var attrPos))
			{
				var accessor = gltf.Accessors[attrPos];
				min = new Vector3(accessor.Min[0], accessor.Min[1], accessor.Min[2]);
				max = new Vector3(accessor.Max[0], accessor.Max[1], accessor.Max[2]);
				return true;
			}
			min = Vector3.One * float.MaxValue;
			max = Vector3.One * float.MinValue;
			return false;
		}
		
		public static bool TryCalculateLocalBounds(this Gltf gltf, in glTFLoader.Schema.Mesh mesh, out Vector3 min, out Vector3 max)
		{
			min = Vector3.One * float.MaxValue;
			max = Vector3.One * float.MinValue;
			var hasBounds = false;
			foreach (var primitive in mesh.Primitives)
			{
				if (gltf.TryCalculateLocalBounds(primitive, out var primMin, out var primMax))
				{
					min = Vector3.ComponentMin(min, primMin);
					max = Vector3.ComponentMax(max, primMax);
					hasBounds = true;
				}
			}
			return hasBounds;
		}

		public static bool TryCalculateGlobalBounds(this Gltf gltf, in Node node, Matrix4 globalTransform, out Vector3 min, out Vector3 max)
		{
			min = Vector3.One * float.MaxValue;
			max = Vector3.One * float.MinValue;
			if (node.Mesh.HasValue)
			{
				var mesh = gltf.Meshes[node.Mesh.Value];
				if (gltf.TryCalculateLocalBounds(mesh, out var meshMin, out var meshMax))
				{
					meshMin = Vector3.TransformPosition(meshMin, globalTransform);
					min = Vector3.ComponentMin(min, meshMin);
					meshMax = Vector3.TransformPosition(meshMax, globalTransform);
					max = Vector3.ComponentMax(max, meshMax);
					return true;
				}
			}
			return false;
		}
	}
}
