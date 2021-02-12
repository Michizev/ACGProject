using OpenTK.Graphics.OpenGL4;
using System;
using System.Reflection;

namespace Framework
{
	public class VertexAttrib : ShaderVariable
	{
		public VertexAttrib(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, GetCheckedLocation(shaderProgram, property))
		{
			var attribute = property.GetCustomAttribute<VertexAttribAttribute>();
			if (attribute is null) throw new ArgumentException($"Attribute '{property.Name}' has no attributes specified");
			Components = attribute.Components;
			Type = attribute.Type;
			PerInstance = attribute.PerInstance;
		}

		public VertexAttrib(ShaderProgram program, int location, int components, VertexAttribType type, bool perInstance = false) : base(program, location)
		{
			Components = components;
			Type = type;
			PerInstance = perInstance;
		}

		public int Components { get; }
		public VertexAttribType Type { get; }
		public bool PerInstance { get; }

		private static int GetCheckedLocation(ShaderProgram shaderProgram, PropertyInfo property)
		{
			var name = shaderProgram.ConvertName(property.Name);
			return shaderProgram.GetCheckedAttributeLocation(name);
		}
	}
}