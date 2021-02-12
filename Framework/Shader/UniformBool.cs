using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace Framework
{
	public sealed class UniformBool : Uniform<bool>
	{
		public UniformBool(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, Setter)
		{
		}

		public UniformBool(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, Setter)
		{
		}

		private static void Setter(int shaderProgram, int location, bool value) => GL.ProgramUniform1(shaderProgram, location, value ? 1 : 0);
	}
}