using Example.Zev;
using Framework;
using glTFLoader.Schema;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.IO;
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
            ShaderInfo(shader);
        }

        private static void ShaderInfo(ShaderProgram shader)
        {
            Console.WriteLine("UNIFORMS");
            GL.GetProgram(shader.Handle, GetProgramParameterName.ActiveUniforms, out var boi);
            for (int i = 0; i < boi; i++)
            {
                GL.GetActiveUniform(shader.Handle, i, 100, out var len, out var size, out var type, out var name);
                Console.WriteLine($"{name} {type} {size}");
            }
            Console.WriteLine("ATTRIBUTES");
            GL.GetProgram(shader.Handle, GetProgramParameterName.ActiveUniforms, out var a);
            for (int i = 0; i < a; i++)
            {
                GL.GetActiveAttrib(shader.Handle, i, 100, out var len, out var size, out var type, out var attname);
                Console.WriteLine($"{attname} {type} {size}");
            }
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
			//TODO what the fuck is the depth here?
			cache.Attach(new Framework.Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);
			//Position Color Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
			//Normal Color Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment1);
			//Color and Specular Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba8), FramebufferAttachment.ColorAttachment2);
			//Metal and roughness Buffer
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba8), FramebufferAttachment.ColorAttachment3);
			return cache;
		}

		public void Resize(int width, int height)
		{
			cache.Dispose();
			cache = CreateGCache(width, height);
		}
	}

	class GLTFImage
	{
		public string Name { get; private set; }

	}
	class GLTFTextures
    {
		GltfModel _model { get; }
		string rootPath;

		public Dictionary<int, Framework.Texture> TextureToTextureHandle = new Dictionary<int, Framework.Texture>();
        public GLTFTextures(GltfModel model, string rootPath)
        {
            _model = model;
            this.rootPath = rootPath;
        }

        public void Load()
        {
			var imageNamesPath = new Dictionary<string, string>();

			foreach (var i in _model.Gltf.Images)
			{
				imageNamesPath.Add(i.Name, i.Uri);
			}

			//var imgs = new Dictionary<int,ImageMagick.MagickImage>();
			foreach (var t in _model.Gltf.Textures)
			{
				Console.WriteLine(t.Name + " " + t.Source);
				if (t.Source is not null)
				{
					var imgName = _model.Gltf.Images[(int)t.Source].Name;
					var path = rootPath + '\\' + imageNamesPath[imgName];
					Console.WriteLine(path);
					using var s = File.OpenRead(path);

					Console.WriteLine(s.CanRead);

					//imgs.Add((int)t.Source, new ImageMagick.MagickImage(path));

					var tex = Framework.TextureLoader.Load(s);

					TextureToTextureHandle.Add((int)t.Source, tex);
				}

			}
		}

    }
	class LightPassShader
    {
		private readonly ShaderProgram _lightPassDef;
		public LightPassShader(string ResourceDir)
		{
			_lightPassDef = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "lightPassDef"));
			var vars = new ShaderProgramVariables(_lightPassDef);

			PosMap = vars.Get<UniformTexture>("positionMap");
			AlbedoMap = vars.Get<UniformTexture>("albedoMap");
			MetalRoughtnessMap = vars.Get<UniformTexture>("metalRoughness");
			NormalMap = vars.Get<UniformTexture>("normalMap");

			EnvMap = vars.Get<UniformCubeTexture>("envMap");

			CameraPos = vars.Get<UniformVec3>("cameraPosition");
			/*
			 * uniform sampler2D positionMap;
uniform sampler2D albedoMap;
uniform sampler2D metalRoughness;
uniform sampler2D normalMap;

uniform samplerCube envMap;
			 * 
			 * 
			 * */
		}

        public UniformTexture PosMap { get; }
        public UniformTexture AlbedoMap { get; }
        public UniformTexture MetalRoughtnessMap { get; }
        public UniformTexture NormalMap { get; }
        public UniformCubeTexture EnvMap { get; }
        public UniformVec3 CameraPos { get; internal set; }

        public void Bind()
	{
		_lightPassDef.Bind();
	}
}
internal class View : Disposable
{
	private const string ResourceDir = nameof(Example) + ".content.";
	GLTFState state;
        private LightPassShader lightDefShader;

        public View()
	{
		gbuffer = new GBUffer();
		state = new GLTFState();
		lightDefShader = new LightPassShader(ResourceDir);
		var ferris = "ferris_wheel_animated.gltf";
		var test = "anotherTest.gltf";
		var cube = "cube.glb";
		var wube = "wubeFolder.wube.gltf";

		var target = ResourceDir + wube;
		var baseDir = ExtractBaseDir(target);

		using (var stream = Resource.LoadStream(target))
		{
			_model = new GltfModel(stream, (string externalName) => Resource.LoadStream(baseDir + externalName));
		}

		var path = wube;
		path = path.Replace('.','\\');
		var index = path.LastIndexOf('\\');
		path = path.Substring(0, index);
		path = Path.GetDirectoryName(path);

		var root = "content";
		textures = new GLTFTextures(_model, root+"\\"+path);
		textures.Load();


		foreach (var m in _model.Gltf.Materials)
		{
			Console.WriteLine(m.PbrMetallicRoughness.BaseColorTexture.Index);
		}


		_shaderProgram = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "noTexture"));


		_PostProcessProgram = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "postProcessing"));

		var shaderToUse = gbuffer.Program;

		var location = GL.GetAttribLocation(shaderToUse.Handle, "texcoord_0");

		int AttributeLoc(string name)
		{
			var attributeName = name.ToLowerInvariant();
			var location = shaderToUse.GetCheckedAttributeLocation(attributeName);
			return location;
		}
		_modelRenderer = new GltfModelRenderer(_model, AttributeLoc);

		var shaderShadowMappingVars = new ShaderProgramVariables(_shaderProgram);


		(sceneMin, sceneMax) = _model.CalcSceneBounds();
		var size = sceneMax - sceneMin;
		OrbitingCamera.Distance = 1.01f * size.Length;
		OrbitingCamera.Target = sceneMin + 0.5f * size;
		GL.Enable(EnableCap.DepthTest);
		GL.Enable(EnableCap.CullFace);

		cache = CreateCache(2, 2);
		cache2 = CreateCache(2, 2);
		cache1 = CreateCache(2, 2);


		overlayCacheTexture = new TextureQuad(-1, -1, 0.5f, 0.5f);
		overlayTextureRight = new TextureQuad(0.5f, -1, 0.5f, 0.5f, "color = color*0.5+vec4(0.5);");
		overlayDepthTexture = new TextureQuad(-0.2f, -1, 0.5f, 0.5f, "color = vec4(vec3(pow(color.r, 32.0)), 1.0);");

		var shaderPostProcessingVars = new ShaderProgramVariables(_PostProcessProgram);
		shaderPostProcessingTexImage = shaderPostProcessingVars.Get<UniformTexture>("texImage");
		shaderPostProcessingLastTexImage = shaderPostProcessingVars.Get<UniformTexture>("lastTexImage");
		shaderPostProcessingDepthImage = shaderPostProcessingVars.Get<UniformTexture>("depthTex");


		GLTFObjectHelper.GetNames(_model.Gltf);

		var cubeData = new CubeTextureLoaderData
		{
			ResourceDir = nameof(Example) + ".content."
		};

			int skybox = 2;
			switch(skybox)
            {
				case 0:
					cubeData.LoadPreset(CubeTextureLoaderData.CubeMapPresets.FULLNAMECAP);
					cubeData.GenerateNames("Daylight Box_", "bmp");
					break;
				case 1:
					cubeData.LoadPreset(CubeTextureLoaderData.CubeMapPresets.SPACESCAPE);
					cubeData.GenerateNames("tssd", "png");
					break;
				case 2:
					cubeData.LoadPreset(CubeTextureLoaderData.CubeMapPresets.NAME);
					cubeData.GenerateNames("", "jpg");
					break;
			}

		


		
		//cubeData.GenerateNames("Daylight Box_", "bmp");

		

		skyboxTexture = CubeTextureLoader.Load(cubeData);
		box = new Skybox(skyboxTexture);
		}

        private static string ExtractBaseDir(string target)
        {
            var splits = target.Split('.');

            var baseDir = "";
            for (int i = 0; i < splits.Length - 2; i++)
            {
                baseDir += $"{splits[i]}.";
            }

            return baseDir;
        }

        public OrbitingCamera OrbitingCamera { get; } = new OrbitingCamera(10f);
		public bool Active { get; set; } = true;

        private readonly GltfModel _model;
		private readonly GltfModelRenderer _modelRenderer;
		private readonly ShaderProgram _shaderProgram;
        
        private readonly ShaderProgram _PostProcessProgram;
        private Vector3 sceneMin;
		private Vector3 sceneMax;
		private FrameBufferGL cache;
        private FrameBufferGL cache1;
        private FrameBufferGL cache2;
        private float _aspect;
		private readonly TextureQuad overlayCacheTexture;
        private readonly TextureQuad overlayTextureRight;
        private readonly TextureQuad overlayDepthTexture;
        private readonly UniformTexture shaderPostProcessingTexImage;
		private readonly UniformTexture shaderPostProcessingLastTexImage;
		private readonly UniformTexture shaderPostProcessingDepthImage;
		private int cacheToUse = 0;

		private VertexArray emptyVA = new VertexArray();
		(FrameBufferGL,FrameBufferGL) GetPingPongCache()
        {
			if(cacheToUse==1)
            {
				cacheToUse = 0;
				return (cache2, cache1);
            }
            else
            {
				cacheToUse = 1;
				return (cache1, cache2);
			}

        }
		internal void Draw(float time)
		{
			if (!Active) return;
			var size = (sceneMax - sceneMin).Length;
			var near = MathF.Max(0.001f, OrbitingCamera.Distance - size);
			var far = OrbitingCamera.Distance + size;
			OrbitingCamera.Projection = Matrix4.CreatePerspectiveFieldOfView(1f, _aspect, near, far);
			//OrbitingCamera.Projection = Matrix4.CreatePerspectiveFieldOfView(1f, _aspect,0.1f, 10);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);



			//_shaderProgram.Bind();
			gbuffer.Bind();

            gbuffer.cache.Draw(()=>
            {
                DrawData(time);
            });

			

			var fwidth = gbuffer.cache.GetTexture(FramebufferAttachment.DepthAttachment).Width;
			var fheight = gbuffer.cache.GetTexture(FramebufferAttachment.DepthAttachment).Height;
			GL.BlitNamedFramebuffer(gbuffer.cache.Handle, 0, 0, 0, fwidth, fheight, 0, 0, fwidth, fheight, ClearBufferMask.DepthBufferBit |ClearBufferMask.StencilBufferBit, BlitFramebufferFilter.Nearest);
			//OrbitingCamera.Target = Vector3.Zero;

			emptyVA.Bind();
			lightDefShader.Bind();
			lightDefShader.AlbedoMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment2));
			lightDefShader.NormalMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment1));
			lightDefShader.PosMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment0));
			lightDefShader.MetalRoughtnessMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment3));
			lightDefShader.EnvMap?.Bind(skyboxTexture);

			if(lightDefShader.CameraPos!=null)
            {
				var pos = OrbitingCamera.CalcPosition();
				lightDefShader.CameraPos.Value = pos;
				//Console.WriteLine(pos);
			}
			

			GL.DepthMask(false);
			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			GL.DepthMask(true);

			var drawSkybox = true;
			if (drawSkybox)
			{
				GL.DepthMask(false);
				GL.DepthFunc(DepthFunction.Equal);
				box.Draw(OrbitingCamera);
				GL.DepthFunc(DepthFunction.Less);
				GL.DepthMask(true);
			}



			overlayCacheTexture.Draw(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment2));
			overlayDepthTexture.Draw(gbuffer.cache.GetTexture(FramebufferAttachment.DepthAttachment));
			overlayTextureRight.Draw(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment1));

			//TODO: 1. Implement motion blur with temporal coherence
			//Draw a frame to the cache

			/*
			cache.Draw(() => DrawNewFrame(time));

			
			var (currentBuffer, lastBuffer) = GetPingPongCache();
			_PostProcessProgram.Bind();

			GL.Disable(EnableCap.DepthTest);

			currentBuffer.Draw(() =>
			{
				_PostProcessProgram.Uniform("camera", OrbitingCamera.Projection);
				_PostProcessProgram.Uniform("effect", 1);
				OrbitingCamera.Azimuth += 30;
				OrbitingCamera.Projection = Matrix4.CreatePerspectiveFieldOfView(1f, _aspect, near, far);
				_PostProcessProgram.Uniform("otherCamera", OrbitingCamera.Projection);
				OrbitingCamera.Azimuth -= 30;
				var texture = cache.GetTexture(FramebufferAttachment.ColorAttachment0);
				shaderPostProcessingTexImage.Bind(texture);

				var depthTexture = cache.GetTexture(FramebufferAttachment.ColorAttachment0);
				shaderPostProcessingDepthImage?.Bind(depthTexture);

				var last = lastBuffer.GetTexture(FramebufferAttachment.ColorAttachment0);
				shaderPostProcessingLastTexImage?.Bind(last);

				GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			});

			var filtered = currentBuffer.GetTexture(FramebufferAttachment.ColorAttachment0);
			shaderPostProcessingTexImage.Bind(filtered);



			_PostProcessProgram.Uniform("effect", 0);
			
			GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
			

			overlayDepthTexture.Draw(cache.GetTexture(FramebufferAttachment.DepthAttachment));
			overlayCacheTexture.Draw(currentBuffer.GetTexture(FramebufferAttachment.ColorAttachment1));

			*/
		}

        private void DrawData(float time)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gbuffer.Program.Uniform("camera", OrbitingCamera.ViewProjection);
            //_shaderProgram.Uniform("cameraPos", OrbitingCamera.CalcPosition());

            //var locBaseColor = _shaderProgram.CheckedUniformLocation("baseColor");
            var locWorld = gbuffer.Program.CheckedUniformLocation("world");

            _model.UpdateAnimations(time);

            int pos = 0;
            foreach (var (globalTransform, drawable, material) in _modelRenderer.TraverseSceneGraphDrawables())
            {
                state.Add(globalTransform);
				gbuffer.Program.Uniform(locWorld, globalTransform);


                //SetMaterial(material);
                if (material.NormalTexture != null)
                {
                    var index = material.NormalTexture.Index;
                    gbuffer.Normal?.Bind(textures.TextureToTextureHandle[index]);
                }
                if (material.PbrMetallicRoughness != null)
                {
                    if (material.PbrMetallicRoughness.BaseColorTexture != null)
                    {
                        var index = material.PbrMetallicRoughness.BaseColorTexture.Index;
                        gbuffer.Albedo?.Bind(textures.TextureToTextureHandle[index]);
                    }
					if (material.PbrMetallicRoughness.MetallicRoughnessTexture != null)
					{
						var index = material.PbrMetallicRoughness.MetallicRoughnessTexture.Index;
						gbuffer.MetalRoughness?.Bind(textures.TextureToTextureHandle[index]);
					}
				}


                drawable.Draw();
            }
        }

        internal void Resize(int width, int height)
		{
			GL.Viewport(0, 0, width, height);
			_aspect = width / (float)height;
			if (width > 0 && height > 0)
			{
				cache.Dispose();
				cache1.Dispose();
				cache2.Dispose();


				cache = CreateCache(width, height);
				cache1 = CreateCache(width, height);
				cache2 = CreateCache(width, height);

				gbuffer.Resize(width, height);
			}
		}

		private static FrameBufferGL CreateCache(int width, int height)
		{
			var cache = new FrameBufferGL(true);

			cache.Attach(new Framework.Texture(width, height, Framework.Texture.DepthComponent32f), FramebufferAttachment.DepthAttachment);
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba8), FramebufferAttachment.ColorAttachment0);
			cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.Rgba8), FramebufferAttachment.ColorAttachment1);

			return cache;
		}


		protected override void DisposeResources() => DisposeAllFields(this);


		int frame = 0;
        private CubeTexture skyboxTexture;
        private Skybox box;
        private GBUffer gbuffer;
        private GLTFTextures textures;

        private void DrawNewFrame(float time)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_shaderProgram.Uniform("camera", OrbitingCamera.ViewProjection);
			_shaderProgram.Uniform("cameraPos", OrbitingCamera.CalcPosition());
			
			var locBaseColor = _shaderProgram.CheckedUniformLocation("baseColor");
			void SetMaterial(Material? material)
			{
				if (material?.PbrMetallicRoughness?.BaseColorFactor is float[] c)
				{
					_shaderProgram.Uniform(locBaseColor, c.ToColor4());
				}
				else
				{
					_shaderProgram.Uniform(locBaseColor, Color4.White);
				}
			}
			_model.UpdateAnimations(time);
			var locWorld = _shaderProgram.CheckedUniformLocation("world");

			

			int pos = 0;
			foreach (var (globalTransform, drawable, material) in _modelRenderer.TraverseSceneGraphDrawables())
			{
				state.Add(globalTransform);
				_shaderProgram.Uniform(locWorld, globalTransform);
				SetMaterial(material);



				drawable.Draw();
			}
			state.EndIteration();

			
			frame++;
		}

        internal void SetActiveState(bool v)
        {
			Active = v;
        }
    }
}
