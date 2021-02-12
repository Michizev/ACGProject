using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace Framework
{
	public sealed class UniformFloat : Uniform<float>
	{
		public UniformFloat(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, GL.ProgramUniform1)
		{
		}

		public UniformFloat(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, GL.ProgramUniform1)
		{
		}
	}
}