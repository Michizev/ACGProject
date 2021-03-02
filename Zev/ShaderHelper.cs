using Framework;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace Zev
{

	public static class MeshGLExtension   
	{
		public static BufferGL UpdateAttribute<Type>(this MeshGL meshGL, BufferGL buffer, int location, int components, VertexAttribType type, Type[] array, bool perInstance = false) where Type : struct
		{
			meshGL.VertexArray.BindAttribute(location, buffer, components, Marshal.SizeOf(array[0]), type, perInstance);

			var attributeLocation = location;
			var Handle = buffer.Handle;


			GL.BindBuffer(BufferTarget.ArrayBuffer, buffer.Handle);
			var elementSize = Marshal.SizeOf(array[0]);
			var size =elementSize*array.Length;
			var offset = components*elementSize;
			offset = 0;
			GL.BufferSubData(BufferTarget.ArrayBuffer,new IntPtr(offset),size,array);
			/*
				if (-1 == attributeLocation) throw new ArgumentException("Invalid attribute location");
				GL.EnableVertexArrayAttrib(Handle, attributeLocation);
				GL.VertexArrayVertexBuffer(Handle, attributeLocation, buffer.Handle, new IntPtr(offset), elementByteSize);
				GL.VertexArrayAttribBinding(Handle, attributeLocation, attributeLocation);
				GL.VertexArrayAttribFormat(Handle, attributeLocation, baseTypeCount, type, normalized, 0);
				if (perInstance)
				{
					GL.VertexArrayBindingDivisor(Handle, attributeLocation, 1);
				}
			*/

			return buffer;
		}
    }
    public static class ShaderHelper
    {
		public static void ShaderInfo(ShaderProgram shader)
		{
			Console.WriteLine("UNIFORMS");
			GL.GetProgram(shader.Handle, GetProgramParameterName.ActiveUniforms, out var boi);
			for (int i = 0; i < boi; i++)
			{
				GL.GetActiveUniform(shader.Handle, i, 100, out var len, out var size, out var type, out var name);
				Console.WriteLine($"{name} {type} {size}");
			}
			Console.WriteLine("ATTRIBUTES");
			GL.GetProgram(shader.Handle, GetProgramParameterName.ActiveAttributes, out var a);
			for (int i = 0; i < a; i++)
			{
				GL.GetActiveAttrib(shader.Handle, i, 100, out var len, out var size, out var type, out var attname);
				Console.WriteLine($"{attname} {type} {size}");
			}
		}
	}
}