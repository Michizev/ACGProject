using Framework;
using OpenTK.Graphics.OpenGL4;
using Zev;

namespace Example
{
    class GBUffer
	{
		private const string ResourceDir = nameof(Example) + ".content.";
		private ShaderProgram _shaderProgram;
		public FrameBufferGL cache;

        public UniformTexture Albedo { get; }
		public UniformTexture MetalRoughness { get; }
		public UniformTexture Normal { get; }

		public ShaderProgram Program => _shaderProgram;

        public GBUffer()
        {
            _shaderProgram = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "gPassDef"));
            var shaderShadowMappingVars = new ShaderProgramVariables(_shaderProgram);

            Albedo = shaderShadowMappingVars.Get<UniformTexture>("albedoMap");
            MetalRoughness = shaderShadowMappingVars.Get<UniformTexture>("metalRoughnessMap");
            Normal = shaderShadowMappingVars.Get<UniformTexture>("normalMap");

			cache = CreateGCache(2, 2);

            var shader = _shaderProgram;
            ShaderHelper.ShaderInfo(shader);
        }



        public void Bind()
		{
			_shaderProgram.Bind();
		}

		private static FrameBufferGL CreateGCache(int width, int height)
		{
			var cache = new FrameBufferGL(true);

			//GL.GetFramebufferAttachmentParameter(0, FramebufferAttachment.Depth, FramebufferParameterName.FramebufferAttachmentColorEncoding, out var par);
			//Console.WriteLine(par);
			//DepthBuffer
			//Framework.Texture.DepthComponent32f
			//cache.Attach(new Framework.Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);
			cache.Attach(new Framework.Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);
			//Position Color Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba32f), FramebufferAttachment.ColorAttachment0);
			//Normal Color Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment1);
			//Color and Specular Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba8), FramebufferAttachment.ColorAttachment2);
			//Metal and roughness Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba8), FramebufferAttachment.ColorAttachment3);

			//Raw Position buffer
			cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment4);
			return cache;
		}

		public void Resize(int width, int height)
		{
			cache.Dispose();
			cache = CreateGCache(width, height);
		}
	}
}
