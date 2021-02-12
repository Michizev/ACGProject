using Framework;
using glTFLoader.Schema;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace Example
{
	public static class GltfRenderHelperGL
	{
		public static IDrawable CreateDrawableGL(this Gltf gltf, MeshPrimitive primitive, IReadOnlyList<BufferGL> buffersGL, Func<string, int> getBindingId)
		{
			var va = new VertexArray();
			var vertexCount = 0;

			foreach (var attribute in primitive.Attributes)
			{
				var accessor = gltf.Accessors[attribute.Value];
				var bindingID = getBindingId(attribute.Key);
				if (-1 != bindingID)
				{
					var bufferViewId = accessor.BufferView ?? 0;
					var bufferView = gltf.BufferViews[bufferViewId];
					var baseTypeCount = GetBaseTypeCount(accessor.Type);
					var elementBytes = bufferView.ByteStride ?? baseTypeCount * GetByteSize(accessor.ComponentType);
					vertexCount = accessor.Count;
					var type = (VertexAttribType)accessor.ComponentType;
					va.BindAttribute(bindingID, buffersGL[bufferViewId], baseTypeCount, elementBytes, type, false, accessor.Normalized, accessor.ByteOffset);
				}
			}
			var primitiveType = (PrimitiveType)primitive.Mode;
			if (primitive.Indices.HasValue)
			{
				var accessor = gltf.Accessors[primitive.Indices.Value];
				var bufferViewId = accessor.BufferView ?? 0;
				va.BindIndices(buffersGL[bufferViewId]);
				var indexType = (DrawElementsType)accessor.ComponentType;
				var indexLocation = accessor.ByteOffset;
				var indexCount = accessor.Count;
				return new DrawIndexed(va, primitiveType, indexCount, indexType, indexLocation);
			}
			return new DrawArray(va, primitiveType, vertexCount);
		}

		private static int GetByteSize(Accessor.ComponentTypeEnum type) => type switch
		{
			Accessor.ComponentTypeEnum.BYTE => 1,
			Accessor.ComponentTypeEnum.UNSIGNED_BYTE => 1,
			Accessor.ComponentTypeEnum.SHORT => 2,
			Accessor.ComponentTypeEnum.UNSIGNED_SHORT => 2,
			Accessor.ComponentTypeEnum.UNSIGNED_INT => 4,
			Accessor.ComponentTypeEnum.FLOAT => 4,
			_ => throw new NotImplementedException(),
		};

		private static int GetBaseTypeCount(Accessor.TypeEnum type) => type switch
		{
			Accessor.TypeEnum.SCALAR => 1,
			Accessor.TypeEnum.VEC2 => 2,
			Accessor.TypeEnum.VEC3 => 3,
			Accessor.TypeEnum.VEC4 => 4,
			Accessor.TypeEnum.MAT2 => 4,
			Accessor.TypeEnum.MAT3 => 9,
			Accessor.TypeEnum.MAT4 => 16,
			_ => throw new NotImplementedException(),
		};
	}
}
