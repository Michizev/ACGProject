using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace Framework
{
	public class BufferGL : Disposable, IObjectGL
	{
		public BufferGL()
		{
			GL.CreateBuffers(1, out int handle);
			Handle = handle;
		}

		public int Handle { get; }

		public void Set<DataType>(DataType[] data, BufferUsageHint usageHint = BufferUsageHint.StaticDraw) where DataType : struct
		{
			if (0 == data.Length) throw new ArgumentException("Empty array");
			var elementSize = Marshal.SizeOf(data[0]);
			var byteSize = elementSize * data.Length;
			GL.NamedBufferData(Handle, byteSize, data, usageHint); //copy data over to GPU
		}

		public void Set(IntPtr data, int byteSize, BufferUsageHint usageHint = BufferUsageHint.StaticDraw)
		{
			GL.NamedBufferData(Handle, byteSize, data, usageHint); //copy data over to GPU
		}

		protected override void DisposeResources() => GL.DeleteBuffer(Handle);
	}
}
