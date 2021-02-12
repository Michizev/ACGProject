using OpenTK.Graphics.OpenGL4;
using System;

namespace Framework
{
	public class DrawIndexed : IDrawable
	{
		private readonly VertexArray _vertexArray;

		public DrawIndexed(VertexArray vertexArray, PrimitiveType primitiveType, int indexCount, DrawElementsType indexType, int indexLocation)
		{
			_vertexArray = vertexArray ?? throw new ArgumentNullException(nameof(vertexArray));
			if (0 == indexCount) throw new ArgumentException($"{nameof(indexCount)} == 0");
			IndexCount = indexCount;
			PrimitiveType = primitiveType;
			IndexType = indexType;
			IndexLocation = indexLocation;
		}

		public int IndexCount { get; }
		public int IndexLocation { get; } = 0;
		public DrawElementsType IndexType { get; }
		public PrimitiveType PrimitiveType { get; }

		public void Draw()
		{
			_vertexArray.Bind();
			GL.DrawElements(PrimitiveType, IndexCount, IndexType, IndexLocation);
		}
	}
}
