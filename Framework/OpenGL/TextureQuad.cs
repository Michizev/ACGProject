using OpenTK.Graphics.OpenGL4;
using System.Globalization;
using System.Threading;

namespace Framework
{
	/// <summary>
	/// Draw a texture on a quad
	/// </summary>
	public class TextureQuad
	{
		/// <summary>Initializes a new instance of the <see cref="TextureQuad"/> class.</summary>
		/// <param name="screenBounds">The screen bounds.</param>
		/// <param name="colorMappingGlslExpression">A color (vec4 -> vec4) mapping GLSL expression.</param>
		public TextureQuad(float minX, float minY, float sizeX, float sizeY, string colorMappingGlslExpression = "color = color;")
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture; // for correct float conversion
			string sVertexShader = @"
				#version 330 core
				out vec2 uv; 
				void main() 
				{
					const vec2 vertices[4] = vec2[4](vec2(0.0, 0.0),
						vec2( 1.0, 0.0),
						vec2( 0.0,  1.0),
						vec2( 1.0,  1.0));

					uv = vertices[gl_VertexID];
					vec2 pos = REPLACE;
					gl_Position = vec4(pos, -1.0, 1.0);
				}";
			sVertexShader = sVertexShader.Replace("REPLACE", $"vec2({minX}, {minY}) + uv * vec2({sizeX}, {sizeY})");
			string sFragmentShd = @"#version 330 core
				uniform sampler2D inputImage;
				in vec2 uv;
				out vec4 fragColor;
				void main() 
				{
					vec4 color = textureLod(inputImage, uv, 0.0);
					colorMappingExpression
					fragColor = color;
				}";
			sFragmentShd = sFragmentShd.Replace("colorMappingExpression", colorMappingGlslExpression);
			shaderProgram = new ShaderProgram();
			shaderProgram.CompileLink(new (ShaderType, string)[] { (ShaderType.VertexShader, sVertexShader), (ShaderType.FragmentShader, sFragmentShd) });
		}

		/// <summary>
		/// Draws the specified texture.
		/// </summary>
		/// <param name="texture">The texture.</param>
		public void Draw(Texture texture)
		{
			GL.BindTextureUnit(0, texture.Handle);
			shaderProgram.Bind();
			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
		}

		private readonly ShaderProgram shaderProgram;
	}
}