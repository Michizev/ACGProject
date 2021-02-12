using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace Framework
{
	public sealed class UniformTexture : Uniform<int>
	{
		public UniformTexture(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, GL.ProgramUniform1)
		{
			TextureSampler = shaderProgram.ConsumeFreeTextureUnit();
		}

		public UniformTexture(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, GL.ProgramUniform1)
		{
			TextureSampler = shaderProgram.ConsumeFreeTextureUnit();
		}

		public void Bind(Texture texture)
		{
			GL.BindTextureUnit(TextureSampler, texture.Handle);
		}


		public int TextureSampler { get => Value; set => Value = value; }
	}
}