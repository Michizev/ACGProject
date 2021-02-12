using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace Framework
{
	public sealed class UniformUInt : Uniform<uint>
	{
		public UniformUInt(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, Setter)
		{
		}

		public UniformUInt(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, Setter)
		{
		}

		private static void Setter(int shaderProgram, int location, uint value) => GL.ProgramUniform1(shaderProgram, location, value);
	}
}