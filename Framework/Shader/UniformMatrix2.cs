using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace Framework
{
	public sealed class UniformMatrix2 : Uniform<Matrix2>
	{
		public UniformMatrix2(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, Setter)
		{
		}
		public UniformMatrix2(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, Setter)
		{
		}

		private static void Setter(int shaderProgram, int location, Matrix2 value) => GL.ProgramUniformMatrix2(shaderProgram, location, false, ref value);
	}
}