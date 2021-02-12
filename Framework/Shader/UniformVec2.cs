using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace Framework
{
	public sealed class UniformVec2 : Uniform<Vector2>
	{
		public UniformVec2(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, GL.ProgramUniform2)
		{
		}

		public UniformVec2(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, GL.ProgramUniform2)
		{
		}
	}
}