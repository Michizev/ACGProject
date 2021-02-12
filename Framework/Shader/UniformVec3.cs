using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Reflection;

namespace Framework
{
	public sealed class UniformVec3 : Uniform<Vector3>
	{
		public UniformVec3(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, GL.ProgramUniform3)
		{
		}

		public UniformVec3(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, GL.ProgramUniform3)
		{
		}
	}
}