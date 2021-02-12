using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Framework
{
	public static class ShaderProgramFactory
	{
		public static string ConvertName(this ShaderProgram shaderProgram, string name)
		{
			return char.ToLower(name[0]) + name[1..]; // first character to lower case
		}

		public static MyShaderProgram CreateWithProperties<MyShaderProgram>() where MyShaderProgram : ShaderProgram, new()
		{
			var shaderProgram = Create<MyShaderProgram>();

			var properties = typeof(MyShaderProgram).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var shaderVariableProperties = properties.Where(prop => typeof(ShaderVariable).IsAssignableFrom(prop.PropertyType));
			foreach (var property in shaderVariableProperties)
			{
				var instance = Activator.CreateInstance(property.PropertyType, new object[] { shaderProgram, property });
				property.SetValue(shaderProgram, instance);
			}
			return shaderProgram;
		}

		public static MyShaderProgram Create<MyShaderProgram>() where MyShaderProgram : ShaderProgram, new()
		{
			var shaderProgram = new MyShaderProgram();
			var myShaderType = typeof(MyShaderProgram);
			var shaderAttribues = myShaderType.GetCustomAttributes<ShaderSourceAttribute>();
			var shaders = new List<(ShaderType, string)>();
			foreach (var shaderSource in shaderAttribues)
			{
				shaders.Add((shaderSource.ShaderType, shaderSource.SourceCode));
			}
			shaderProgram.CompileLink(shaders);
			return shaderProgram;
		}

		public static ShaderProgram CompileLink(this ShaderProgram shaderProgram, IEnumerable<(ShaderType, string)> shaders)
		{
			var shaderId = new List<int>();
			var unique = shaders.ToDictionary(data => data.Item1, data => data.Item2); // make sure each shader type is only present once
			if (unique.Count < 1) throw new ShaderProgramException("Empty set of shaders for shader program");
			foreach ((ShaderType type, string sourceCode) in unique)
			{
				var shader = ShaderTools.CreateShader(type, sourceCode);
				GL.AttachShader(shaderProgram.Handle, shader);
				shaderId.Add(shader);
			}
			GL.LinkProgram(shaderProgram.Handle);
			foreach (var shader in shaderId)
			{
				GL.DeleteShader(shader);
			}
			var log = shaderProgram.GetShaderProgramLog();
			if (!string.IsNullOrEmpty(log))
			{
				throw new ShaderProgramException(log);
			}
			return shaderProgram;
		}
	}
}
