using OpenTK.Graphics.OpenGL4;
using System;

namespace Framework
{
	public class DrawArray : IDrawable
	{
		private readonly VertexArray _vertexArray;

		public DrawArray(VertexArray vertexArray, PrimitiveType primitiveType, int vertexCount)
		{
			_vertexArray = vertexArray ?? throw new ArgumentNullException(nameof(vertexArray));
			if (0 == vertexCount) throw new ArgumentException($"{nameof(vertexCount)} == 0");
			PrimitiveType = primitiveType;
			VertexCount = vertexCount;
		}

		public PrimitiveType PrimitiveType { get; }
		public int VertexCount { get; }

		public void Draw()
		{
			_vertexArray.Bind();
			GL.DrawArrays(PrimitiveType, 0, VertexCount);
		}
	}
}
