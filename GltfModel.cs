using glTFLoader;
using glTFLoader.Schema;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Example
{
	public class GltfModel
	{
		public GltfModel(Stream streamGltf, Func<string, Stream> externalReferenceSolver)
		{
			Gltf = Interface.LoadModel(streamGltf);
			_localNodeTransforms = Gltf.ExtractLocalTransforms();
			_byteBuffers = Gltf.ExtractByteBuffers(externalReferenceSolver);
			_animationControllers = Gltf.CreateAnimationControllers(_byteBuffers, _localNodeTransforms);
		}

		public List<byte[]> ExtractBufferViews() => Gltf.ExtractBufferViews(_byteBuffers);

		public (Vector3 min, Vector3 max) CalcSceneBounds() => Gltf.CalculateSceneBounds(_localNodeTransforms);

		public Gltf Gltf { get; }

		public IEnumerable<(Matrix4, MeshPrimitive primitive)> TraverseSceneGraphPrimitives()
		{
			foreach (var (globalTransform, nodeId) in Gltf.TraverseNodeTransforms(_localNodeTransforms))
			{
				var node = Gltf.Nodes[nodeId];
				if (node.Mesh.HasValue)
				{
					var mesh = Gltf.Meshes[node.Mesh.Value];
					foreach(var primitive in mesh.Primitives)
					{
						yield return (globalTransform, primitive);
					}
				}
			}
		}

		public void UpdateAnimations(float totalSeconds)
		{
			//for each animation channel update the respective local transform of the node with current time
			foreach (var animationController in _animationControllers)
			{
				animationController(totalSeconds);
			}
		}

		private readonly List<byte[]> _byteBuffers;
		private readonly List<Action<float>> _animationControllers;
		private readonly Matrix4[] _localNodeTransforms;
	}
}
