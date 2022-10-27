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

        public UniformBool HasEmissive { get; }
        public UniformBool HasAO { get; }
        public UniformTexture Albedo { get; }
		public UniformTexture MetalRoughness { get; }
		public UniformTexture Normal { get; }

		public ShaderProgram Program => _shaderProgram;

        public UniformTexture Emissive { get; internal set; }

        public GBUffer()
        {
            _shaderProgram = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "gPassDef"));
            var shaderShadowMappingVars = new ShaderProgramVariables(_shaderProgram);

            Albedo = shaderShadowMappingVars.Get<UniformTexture>("albedoMap");
            MetalRoughness = shaderShadowMappingVars.Get<UniformTexture>("metalRoughnessMap");
            Normal = shaderShadowMappingVars.Get<UniformTexture>("normalMap");
			Emissive = shaderShadowMappingVars.Get<UniformTexture>("emissiveMap");
			cache = CreateGCache(2, 2);
			HasEmissive = shaderShadowMappingVars.Get<UniformBool>("hasEmissive");
			HasAO = shaderShadowMappingVars.Get<UniformBool>("hasAO");
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
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment2);
			//Metal and roughness Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment3);

			//View NormalBuffer
			cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment4);

			//View Position Buffer
			cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment5);

			//Emissive Buffer
			cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment6);
			return cache;
		}

		public void Resize(int width, int height)
		{
			cache.Dispose();
			cache = CreateGCache(width, height);
		}
	}
}
