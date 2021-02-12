using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace Framework
{
	public sealed class UniformInt : Uniform<int>
	{
		public UniformInt(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, GL.ProgramUniform1)
		{
		}

		public UniformInt(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, GL.ProgramUniform1)
		{
		}
	}
}