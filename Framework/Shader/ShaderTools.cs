using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Diagnostics;

namespace Framework
{
	public static class ShaderTools
	{
		public static int CreateShader(ShaderType type, string shaderSource)
		{
			var shader = GL.CreateShader(type);

			GL.ShaderSource(shader, shaderSource);
			GL.CompileShader(shader);
			var log = GetShaderLog(shader);
			if (!string.IsNullOrEmpty(log))
			{
				throw new ShaderException(type, log);
			}
			return shader;
		}

		public static string GetShaderLog(int shader)
		{
			GL.GetShader(shader, ShaderParameter.CompileStatus, out int status_code);
			if (1 == status_code)
			{
				return string.Empty;
			}
			else
			{
				return GL.GetShaderInfoLog(shader);
			}
		}

		public static string GetShaderProgramLog(this ShaderProgram shaderProgram)
		{
			GL.GetProgram(shaderProgram.Handle, GetProgramParameterName.LinkStatus, out int status_code);
			if (1 == status_code)
			{
				return string.Empty;
			}
			else
			{
				return GL.GetProgramInfoLog(shaderProgram.Handle);
			}
		}

		public static ShaderProgram PrintExceptions(Func<ShaderProgram> createShaderProgram)
		{
			try
			{
				return createShaderProgram();
			}
			catch (ShaderException se)
			{
				Debug.WriteLine(se.ShaderType);
				Debug.WriteLine(se.Message);
				Debugger.Break();
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				Debugger.Break();
			}
			return null;
		}

		public static int GetCheckedAttributeLocation(this ShaderProgram shaderProgram, string name)
		{
			var location = GL.GetAttribLocation(shaderProgram.Handle, name);
			Debug.WriteLineIf(-1 == location, $"Attribute '{name}' not found in shader program {shaderProgram.GetType().Name}");
			return location;
		}

		public static int CheckedUniformLocation(this ShaderProgram shaderProgram, string name)
		{
			var location = GL.GetUniformLocation(shaderProgram.Handle, name);
			Debug.WriteLineIf(-1 == location, $"Uniform '{name}' not found in shader program {shaderProgram.GetType().Name}({shaderProgram.Handle})");
			return location;
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, uint value)
		{
			GL.ProgramUniform1(shaderProgram.Handle, location, value);
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, int value)
		{
			GL.ProgramUniform1(shaderProgram.Handle, location, value);
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, float value)
		{
			GL.ProgramUniform1(shaderProgram.Handle, location, value);
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, float[] value)
		{
			GL.ProgramUniform1(shaderProgram.Handle, location, value.Length, value.ToFloatArray());
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Vector2 value)
		{
			GL.ProgramUniform2(shaderProgram.Handle, location, value);
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Vector2[] value)
		{
			GL.ProgramUniform2(shaderProgram.Handle, location, value.Length, value.ToFloatArray());
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Vector3 value)
		{
			GL.ProgramUniform3(shaderProgram.Handle, location, value);
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Vector3[] value)
		{
			GL.ProgramUniform3(shaderProgram.Handle, location, value.Length, value.ToFloatArray());
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Color4 value)
		{
			GL.ProgramUniform4(shaderProgram.Handle, location, value);
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Vector4 value)
		{
			GL.ProgramUniform4(shaderProgram.Handle, location, value);
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Vector4[] value)
		{
			GL.ProgramUniform4(shaderProgram.Handle, location, value.Length, value.ToFloatArray());
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Matrix4 value)
		{
			GL.ProgramUniformMatrix4(shaderProgram.Handle, location, false, ref value);
		}

		public static void Uniform(this ShaderProgram shaderProgram, int location, Matrix4[] value)
		{
			GL.ProgramUniformMatrix4(shaderProgram.Handle, location, value.Length, false, value.ToFloatArray());
		}

		public static void Uniform(this ShaderProgram shaderProgram, string name, uint value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, int value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, float value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, float[] value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Vector2 value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Vector2[] value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Vector3 value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Vector3[] value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Color4 value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Vector4 value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Vector4[] value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Matrix4 value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);

		public static void Uniform(this ShaderProgram shaderProgram, string name, Matrix4[] value) => shaderProgram.Uniform(shaderProgram.CheckedUniformLocation(name), value);
	}
}
