using OpenTK.Graphics.OpenGL4;

namespace Framework
{
	public class ShaderProgram : Disposable, IObjectGL
	{
		public ShaderProgram() => Handle = GL.CreateProgram();

		public int Handle { get; }

		public void Bind()
		{
			GL.UseProgram(Handle);
		}

		public int ConsumeFreeTextureUnit()
		{
			++freeTextureUnit;
			return freeTextureUnit - 1;
		}

		protected override void DisposeResources()
		{
			GL.DeleteProgram(Handle);
		}

		private int freeTextureUnit = 0;
	}
}
