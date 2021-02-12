using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Framework
{
	public class MeshGL : Disposable
	{
		public VertexArray VertexArray { get; } = new VertexArray();
		public int IndexCount { get; private set; }

		public BufferGL AddIndices(uint[] array)
		{
			var buffer = AddBuffer(array);
			VertexArray.BindIndices(buffer);
			IndexCount = array.Length;
			return buffer;
		}

		public BufferGL AddAttribute<Type>(int location, int components, VertexAttribType type, Type[] array, bool perInstance = false) where Type : struct
		{
			var buffer = AddBuffer(array);
			VertexArray.BindAttribute(location, buffer, components, Marshal.SizeOf(array[0]), type, perInstance);
			return buffer;
		}

		public BufferGL AddAttribute<Type>(VertexAttrib attrib, Type[] array) where Type : struct
		{
			return AddAttribute(attrib.Location, attrib.Components, attrib.Type, array, attrib.PerInstance);
		}

		protected override void DisposeResources()
		{
			foreach (var buffer in _buffers) buffer.Dispose();
			VertexArray.Dispose();
		}

		private readonly List<IDisposable> _buffers = new List<IDisposable>();

		private BufferGL AddBuffer<Type>(Type[] array) where Type : struct
		{
			var buffer = new BufferGL();
			if (array != null) buffer.Set(array);
			_buffers.Add(buffer);
			return buffer;
		}
	}
}