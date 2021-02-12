using OpenTK.Graphics.OpenGL4;
using System;

namespace Framework
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class VertexAttribAttribute : Attribute
	{
		public VertexAttribAttribute(int components, VertexAttribType type = VertexAttribType.Float, bool perInstance = false)
		{
			Components = components;
			Type = type;
			PerInstance = perInstance;
		}

		public int Components { get; }
		public VertexAttribType Type { get; }
		public bool PerInstance { get; }
	}
}