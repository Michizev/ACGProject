using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace Framework
{
	public sealed class UniformVec4 : Uniform<Vector4>
	{
		public UniformVec4(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, GL.ProgramUniform4)
		{
		}

		public UniformVec4(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, GL.ProgramUniform4)
		{
		}
	}
}