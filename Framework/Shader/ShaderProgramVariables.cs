using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Zev;

namespace Framework
{
	public class ShaderProgramVariables : IShaderProgramVariables
	{
		public ShaderProgramVariables(ShaderProgram shaderProgram)
		{
			//load uniforms
			GL.GetProgram(shaderProgram.Handle, GetProgramParameterName.ActiveUniforms, out int uniformCount);
			for (int i = 0; i < uniformCount; ++i)
			{
				GL.GetActiveUniform(shaderProgram.Handle, i, 255, out int _, out int _, out var type, out string uniformName);
				var location = shaderProgram.CheckedUniformLocation(uniformName);
				_shaderVariables[uniformName] = CreateUniform(shaderProgram, location, type);
			}
			//load attributes
			GL.GetProgram(shaderProgram.Handle, GetProgramParameterName.ActiveAttributes, out int attributeCount);
			for (int i = 0; i < attributeCount; ++i)
			{
				GL.GetActiveAttrib(shaderProgram.Handle, i, 255, out int _, out int _, out var type, out string attributeName);
				var location = GL.GetAttribLocation(shaderProgram.Handle, attributeName);
				_shaderVariables[attributeName] = new VertexAttrib(shaderProgram, location, ExtractCount(type), ExtractType(type));
			}
			ShaderProgram = shaderProgram;
			//TODO: other types like Shader storage blocks, uniform blocks
		}

		/// <summary>
		/// Returns the shader variable with the given type and name and returns null if the shader variable is not found
		/// </summary>
		/// <typeparam name="VariableType"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public VariableType Get<VariableType>(string name) where VariableType : ShaderVariable
		{
			if (_shaderVariables.TryGetValue(name, out var instance))
			{
				if (instance is VariableType casted)
				{
					return casted;
				}
				throw new ArgumentException($"Shader variable type miss match! Requested {typeof(VariableType).Name}, but found {instance.GetType().Name}");
			}
			return null;
		}

		/// <summary>
		/// Returns the shader variable with the given type and name and returns a default implementation if the shader variable is not found
		/// </summary>
		/// <typeparam name="VariableType"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public VariableType GetWithDefault<VariableType>(string name) where VariableType : ShaderVariable
		{
			var variable = Get<VariableType>(name);
			if (variable is null)
			{
				Debug.WriteLine($"Shader variable '{name}' not found.");
				var type = typeof(VariableType);
				var newInstance = Activator.CreateInstance(type, new object[] { ShaderProgram, -1 }); //TODO: HACK Attrib has no such constructor
				return (VariableType)newInstance;
			}
			return variable;
		}

		public ShaderProgram ShaderProgram { get; }

		private readonly Dictionary<string, ShaderVariable> _shaderVariables = new Dictionary<string, ShaderVariable>();

		private static ShaderVariable CreateUniform(ShaderProgram shaderProgram, int location, ActiveUniformType type)
		{
			return type switch
			{
				ActiveUniformType.Bool => new UniformBool(shaderProgram, location),
				ActiveUniformType.Int => new UniformInt(shaderProgram, location),
				ActiveUniformType.UnsignedInt => new UniformUInt(shaderProgram, location),
				ActiveUniformType.Float => new UniformFloat(shaderProgram, location),
				ActiveUniformType.FloatVec2 => new UniformVec2(shaderProgram, location),
				ActiveUniformType.FloatVec3 => new UniformVec3(shaderProgram, location),
				ActiveUniformType.FloatVec4 => new UniformVec4(shaderProgram, location),
				ActiveUniformType.FloatMat2 => new UniformMatrix2(shaderProgram, location),
				ActiveUniformType.FloatMat3 => new UniformMatrix3(shaderProgram, location),
				ActiveUniformType.FloatMat4 => new UniformMatrix4(shaderProgram, location),
				ActiveUniformType.Sampler2D => new UniformTexture(shaderProgram, location),
				ActiveUniformType.SamplerCube => new UniformCubeTexture(shaderProgram,location),
				_ => throw new ArgumentException("Case not implemented"),
			};
		}

		private static int ExtractCount(ActiveAttribType type)
		{
			var name = type.ToString();
			if (name.EndsWith("Vec2")) return 2;
			if (name.EndsWith("Vec3")) return 3;
			if (name.EndsWith("Vec4")) return 4;
			return type switch
			{
				ActiveAttribType.Int or ActiveAttribType.UnsignedInt or ActiveAttribType.Float => 1,
				_ => throw new ArgumentException("Case not implemented"),
			};
		}

		private static VertexAttribType ExtractType(ActiveAttribType type)
		{
			var name = type.ToString();
			if (name.StartsWith(ActiveAttribType.Float.ToString())) return VertexAttribType.Float;
			if (name.StartsWith(ActiveAttribType.Int.ToString())) return VertexAttribType.Int;
			if (name.StartsWith(ActiveAttribType.UnsignedInt.ToString())) return VertexAttribType.UnsignedInt;
			throw new ArgumentException("Case not implemented");
		}
	}
}
