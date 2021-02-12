using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.Serialization;

namespace Framework
{
	[Serializable]
	public class ShaderException : Exception
	{
		public ShaderType ShaderType { get; }

		public ShaderException()
		{
		}

		public ShaderException(string message) : base(message)
		{
		}

		public ShaderException(ShaderType shaderType, string message) : base(message)
		{
			ShaderType = shaderType;
		}

		public ShaderException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ShaderException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}