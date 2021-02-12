using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace Framework
{
	public sealed class UniformMatrix3 : Uniform<Matrix3>
	{
		public UniformMatrix3(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, Setter)
		{
		}
		public UniformMatrix3(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, Setter)
		{
		}

		private static void Setter(int shaderProgram, int location, Matrix3 value) => GL.ProgramUniformMatrix3(shaderProgram, location, false, ref value);

	}
}