using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System.Reflection;

namespace Framework
{
	public class ShaderStorageBlock : ShaderVariable
	{
		public ShaderStorageBlock(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, GetLocation(shaderProgram, property))
		{
		}

		public ShaderStorageBlock(ShaderProgram program, int location) : base(program, location)
		{
		}

		private static int GetLocation(ShaderProgram shaderProgram, PropertyInfo property)
		{
			var name = shaderProgram.ConvertName(property.Name);
			var location = GL.GetProgramResourceIndex(shaderProgram.Handle, ProgramInterface.ShaderStorageBlock, name);
			Debug.WriteLineIf(-1 == location, $"Shader storage block '{name}' not found in shader program");
			return location;
		}
	}
}