using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace Framework
{
	public sealed class UniformColor : Uniform<Color4>
	{
		public UniformColor(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, GL.ProgramUniform4)
		{
		}

		public UniformColor(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, GL.ProgramUniform4)
		{
		}
	}
}