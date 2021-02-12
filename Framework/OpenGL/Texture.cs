using OpenTK.Graphics.OpenGL4;
using System;

namespace Framework
{
	public class Texture : Disposable, IObjectGL
	{
		public static SizedInternalFormat DepthComponent32f => (SizedInternalFormat)All.DepthComponent32f;

		public Texture(int width, int height, SizedInternalFormat format = SizedInternalFormat.Rgba8, int levels = 0, TextureTarget target = TextureTarget.Texture2D)
		{
			GL.CreateTextures(target, 1, out int handle);
			Handle = handle;
			Width = width;
			Height = height;
			var size = Math.Max(width, height);
			if(0 == levels) levels = (int)MathF.Ceiling(MathF.Log10(size) / MathF.Log10(2f));
			GL.TextureStorage2D(Handle, levels, format, width, height);
		}

		public int Handle { get; }
		public int Width { get; }
		public int Height { get; }

		public TextureWrapMode Function
		{
			get => _function;
			set
			{
				_function = value;
				GL.TextureParameter(Handle, TextureParameterName.TextureWrapS, (int)value);
				GL.TextureParameter(Handle, TextureParameterName.TextureWrapT, (int)value);
				GL.TextureParameter(Handle, TextureParameterName.TextureWrapR, (int)value);
			}
		}

		public TextureMinFilter MinFilter
		{
			get => _minFilter; 
			set
			{
				_minFilter = value;
				GL.TextureParameter(Handle, TextureParameterName.TextureMinFilter, (int)value);
			}
		}

		public TextureMagFilter MagFilter
		{
			get => _magFilter;
			set
			{
				_magFilter = value;
				GL.TextureParameter(Handle, TextureParameterName.TextureMagFilter, (int)value);
			}
		}

		protected override void DisposeResources() => GL.DeleteTexture(Handle);

		private TextureWrapMode _function;
		private TextureMinFilter _minFilter;
		private TextureMagFilter _magFilter;
	}
}
