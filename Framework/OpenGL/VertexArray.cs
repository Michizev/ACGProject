using OpenTK.Graphics.OpenGL4;
using System;

namespace Framework
{
	/// <summary>
	/// Create a vertex array object for interpreting our buffer data.
	/// </summary>
	public class VertexArray : Disposable, IObjectGL
	{
		public VertexArray()
		{
			GL.CreateVertexArrays(1, out int handle);
			Handle = handle;
		}

		public int Handle { get; }

		public void BindIndices(BufferGL buffer)
		{
			GL.VertexArrayElementBuffer(Handle, buffer.Handle);
		}

		public void Bind()
		{
			GL.BindVertexArray(Handle); // activate vertex array; from now on state is stored;
		}

		public void BindAttribute(int attributeLocation, BufferGL buffer, int baseTypeCount, int elementByteSize, VertexAttribType type, bool perInstance = false, bool normalized = false, int offset = 0)
		{
			if (-1 == attributeLocation) throw new ArgumentException("Invalid attribute location");
			GL.EnableVertexArrayAttrib(Handle, attributeLocation);
			GL.VertexArrayVertexBuffer(Handle, attributeLocation, buffer.Handle, new IntPtr(offset), elementByteSize);
			GL.VertexArrayAttribBinding(Handle, attributeLocation, attributeLocation);
			GL.VertexArrayAttribFormat(Handle, attributeLocation, baseTypeCount, type, normalized, 0);
			if (perInstance)
			{
				GL.VertexArrayBindingDivisor(Handle, attributeLocation, 1);
			}
		}

		protected override void DisposeResources() => GL.DeleteVertexArray(Handle);
	}
}