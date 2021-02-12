using OpenTK.Graphics.OpenGL4;
using System;

namespace Framework
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class ShaderSourceAttribute : Attribute
	{
		public ShaderSourceAttribute(ShaderType shaderType, string resourceName)
		{
			ShaderType = shaderType;
			SourceCode = Resource.LoadString(resourceName);
		}

		public ShaderType ShaderType { get; }
		public string SourceCode { get; }
	}
}