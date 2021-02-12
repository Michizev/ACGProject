using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace Framework
{
	public sealed class UniformMatrix4 : Uniform<Matrix4>
	{
		public UniformMatrix4(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, Setter)
		{
		}

		public UniformMatrix4(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, Setter)
		{
		}

		private static void Setter(int shaderProgram, int location, Matrix4 value) => GL.ProgramUniformMatrix4(shaderProgram, location, false, ref value);
	}
}