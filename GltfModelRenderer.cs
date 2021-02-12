using Framework;
using glTFLoader.Schema;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace Example
{
	internal class GltfModelRenderer : Disposable
	{
		public GltfModelRenderer(GltfModel model, Func<string, int> attributeLoc)
		{
			_model = model;
			var bufferViews = model.ExtractBufferViews();
			foreach (var bufferView in bufferViews)
			{
				var buffer = new BufferGL();
				buffer.Set(bufferView, BufferUsageHint.StaticDraw);
				_buffersGL.Add(buffer);
			}
			foreach (var mesh in model.Gltf.Meshes)
			{
				foreach (var primitive in mesh.Primitives)
				{
					_drawablesGL[primitive] = model.Gltf.CreateDrawableGL(primitive, _buffersGL, attributeLoc);
				}
			}
		}

		public IEnumerable<(Matrix4 globalTransform, IDrawable drawable, Material? material)> TraverseSceneGraphDrawables()
		{
			foreach (var (globalTransform, primitive) in _model.TraverseSceneGraphPrimitives())
			{
				var drawable = _drawablesGL[primitive];
				yield return (globalTransform, drawable, ResolveMaterial(primitive.Material));
			}
		}

		protected override void DisposeResources()
		{
			foreach (var buf in _buffersGL) buf?.Dispose();
		}

		private readonly List<BufferGL> _buffersGL = new();
		private readonly Dictionary<MeshPrimitive, IDrawable> _drawablesGL = new();
		private readonly GltfModel _model;

		private Material? ResolveMaterial(int? id) => id.HasValue ? _model.Gltf.Materials[id.Value] : null;
	}
}
