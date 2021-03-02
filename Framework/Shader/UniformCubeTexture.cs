using Framework;
using OpenTK.Graphics.OpenGL4;
using System.Reflection;

namespace Zev
{
    public class UniformCubeTexture : Uniform<int>
    {
		public UniformCubeTexture(ShaderProgram shaderProgram, PropertyInfo property) : base(shaderProgram, property, GL.ProgramUniform1)
		{
			TextureSampler = shaderProgram.ConsumeFreeTextureUnit();
		}

		public UniformCubeTexture(ShaderProgram shaderProgram, int location) : base(shaderProgram, location, GL.ProgramUniform1)
		{
			TextureSampler = shaderProgram.ConsumeFreeTextureUnit();
		}

		/*
		public void Bind(CubeTexture texture)
		{
			GL.BindTextureUnit(TextureSampler, texture.Handle);
		}
		*/

		public int TextureSampler { get => Value; set => Value = value; }
	}
}