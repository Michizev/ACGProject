using Example.Zev;
using Example.Zev.Extensions;
using Framework;
using glTFLoader.Schema;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Zev;

namespace Example
{
    interface IResizeable
    {
        public void Resize(int width, int height);
    }
    interface ICacheMaker
    {
        public FrameBufferGL CreateCache(int width, int height);
    }
    class CacheManager : IResizeable, IDisposable
    {
        public ICacheMaker maker;
        public FrameBufferGL cache;

        public CacheManager(ICacheMaker maker)
        {
            this.maker = maker;
            cache = maker.CreateCache(2, 2);
        }

        public void Dispose()
        {
            ((IDisposable)cache).Dispose();
        }

        public void Resize(int width, int height)
        {
            cache.Dispose();
            cache = maker.CreateCache(width, height);
        }

    }
    class DepthCache : ICacheMaker
    {
        public FrameBufferGL CreateCache(int width, int height)
        {
            var cache = new FrameBufferGL(true);

            //cache.Attach(new RenderBufferGL(RenderbufferStorage.DepthComponent32, width, height), FramebufferAttachment.DepthAttachment);
            cache.Attach(new Framework.Texture(width, height, Framework.Texture.DepthComponent32f), FramebufferAttachment.DepthAttachment);
            cache.Attach(new Framework.Texture(width, height, SizedInternalFormat.R16f), FramebufferAttachment.ColorAttachment0);
            return cache;
        }
    }
    class BaseCache : IResizeable, IDisposable
    {
        protected CacheManager cache;

        public BaseCache(ICacheMaker maker)
        {
            this.cache = new CacheManager(maker);
        }

        public void Resize(int width, int height)
        {
            cache.Resize(width, height);
        }

        public void Dispose()
        {
            ((IDisposable)cache).Dispose();
        }

        public FrameBufferGL Cache => cache.cache;
    }
    class ShadowMap : BaseCache
    {
        public ShadowMap() : base(new DepthCache())
        {

        }

    }

    class GLTFImage
    {
        public string Name { get; private set; }

    }

    public class ViewportHelper
    {
        private int[] viewport;
        
        public void GetCurrentViewPort()
        {
            viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
        }

        public void SetLastViewport()
        {
            GL.Viewport(viewport[0], viewport[1], viewport[2], viewport[3]);
        }
    }
    public class InterfaceSearcher
    {
        public static void SearchForDisposeable(Object obj)
        {
            var fields = obj.GetType().GetFields(
                                     BindingFlags.NonPublic | BindingFlags.Public |
                                     BindingFlags.Instance);

            var props = obj.GetType().GetProperties(
                         BindingFlags.NonPublic | BindingFlags.Public |
                         BindingFlags.Instance);

            Console.WriteLine("Fields");
            var t = typeof(IDisposable);
            foreach (var f in fields)
            {
                if (f.FieldType.GetInterfaces().Contains(t))
                {
                    Console.Write("DISPOSEABLE : ");
                }
                Console.WriteLine(f);
            }
            Console.WriteLine("Props");
            foreach (var f in props)
            {

                if (f.PropertyType.GetInterfaces().Contains(t))
                {
                    Console.Write("DISPOSEABLE : ");
                }
                Console.WriteLine(f);
            }
        }
    }

    internal class View : Disposable
    {
        private const string ResourceDir = nameof(Example) + ".content.";
        GLTFState state;
        private LightPassShader lightDefShader;
        private ShadowMap map;

        public View()
        {
            gbuffer = new GBUffer();
            state = new GLTFState();
            lightDefShader = new LightPassShader(ResourceDir);
            var ferris = "ferris_wheel_animated.gltf";
            var test = "anotherTest.gltf";
            var cube = "cube.glb";
            var wube = "wubeFolder.wube.gltf";
            wube = "otherModel.oof.gltf";

            wube = "testScene.scene.gltf";
            //wube = "otherModelLess.testSchatten.gltf";

            var target = ResourceDir + wube;
            var baseDir = ExtractBaseDir(target);

            using (var stream = Resource.LoadStream(target))
            {
                _model = new GltfModel(stream, (string externalName) => Resource.LoadStream(baseDir + externalName));
            }

            map = new ShadowMap();

            var path = wube;
            path = path.Replace('.', '\\');
            var index = path.LastIndexOf('\\');
            path = path.Substring(0, index);
            path = Path.GetDirectoryName(path);

            var root = "content";
            textures = new GLTFTextures(_model, root + "\\" + path);
            textures.Load();

            /*
        foreach (var m in _model.Gltf.Materials)
        {
            Console.WriteLine(m.PbrMetallicRoughness.BaseColorTexture.Index);
        }
            */

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
            switch (skybox)
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


            texCalc = new TextureCalculator();
            irradianceMap = texCalc.CalculateIrradianceMap(skyboxTexture);

            box = new Skybox(skyboxTexture);
            //box = new Skybox(irradianceMap);
            #region Load Sphere
            shaderLightBallsInstanced = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "lightBallsShader"));
            var lightShaderVars = new ShaderProgramVariables(shaderLightBallsInstanced);
            var lightPos = lightShaderVars.Get<VertexAttrib>("position");
            var lightNormal = lightShaderVars.Get<VertexAttrib>("normal");
            var lightTexCoord = lightShaderVars.Get<VertexAttrib>("texCoords");

            lightInstancePos = lightShaderVars.Get<VertexAttrib>("instancePosition");

            var sphere = "Sphere.obj";
            var suzanne = "suzanne.obj";
            lightSphere = MeshToGL.Create(Framework.ObjLoader.LoadFromResource(ResourceDir, sphere), lightPos, lightNormal, lightTexCoord);
            if (lightInstancePos != null)
            {
                instanceLightPositions = new Vector3[] { new Vector3(-2f, 1f, 0f), new Vector3(-4f, 1f, 0f), new Vector3(2f, 1f, 0f), new Vector3(1f, 1f, 0f) };
                /*
                for (int i = 0; i < instanceLightPositions.Length; i++)
                {
                    instanceLightPositions[i] *= 100;
                }
                */
                //instanceLightPositions = new Vector3[] { new Vector3(-2f, 0f, 0f) };
                //var lightSpherePositionBuffer = lightSphere.AddAttribute(lightInstancePos, instanceLightPositions);
                //var lightSpherePositionBuffer = lightSphere.AddAttribute(lightInstancePos, instanceLightPositions);
                
                    var attrib = lightInstancePos;
                    lightSpherePositionBuffer = lightSphere.AddAttribute(attrib.Location, attrib.Components, attrib.Type, instanceLightPositions, true);
                
                
                for (int i = 0; i < instanceLightPositions.Length; i++)
                {
                    instanceLightPositions[i].X *= 2;
                }
                lightSphere.UpdateAttribute(lightSpherePositionBuffer, attrib.Location, attrib.Components, attrib.Type, instanceLightPositions, true);
            }



            #endregion
            //-52.5

            //ele 36.66667


            directionalLight = new OrbitingCamera(OrbitingCamera.Distance, -52.5f, 36.66667f);


            InterfaceSearcher.SearchForDisposeable(this);


            depthShader = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "depthLight"));
            var depthShaderVars = new ShaderProgramVariables(depthShader);




            GL.Enable(EnableCap.CullFace); //TODO: switch on/off; with differences in shadow quality can you see?
            GL.Enable(EnableCap.DepthTest);
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

        private OrbitingCamera directionalLight;

        private VertexArray emptyVA = new VertexArray();
        (FrameBufferGL, FrameBufferGL) GetPingPongCache()
        {
            if (cacheToUse == 1)
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
        double deltaTime;
        double lastTime = 0;

        double angle = 0;
        internal void Draw(float time)
        {

            if (!Active) return;

            deltaTime = time - lastTime;
            lastTime = time;
            
            var attrib = lightInstancePos;

            angle += deltaTime*0.001;
            var fullRadi = 2*Math.PI;
            if (angle > fullRadi) angle -= (float)fullRadi;

            var rot = Matrix4.CreateRotationY((float)angle);
            var poss = Matrix4.CreateTranslation(6.5f, 1.5f, 0);

            var combined = poss*rot;
            var newPos = combined.ExtractTranslation();
            //Console.WriteLine(angle);
            instanceLightPositions[0] = newPos;

            lightSphere.UpdateAttribute(lightSpherePositionBuffer, attrib.Location, attrib.Components, attrib.Type, instanceLightPositions, true);

            var size = (sceneMax - sceneMin).Length;
            var near = MathF.Max(0.001f, OrbitingCamera.Distance - size);
            var far = OrbitingCamera.Distance + size;

            //far = OrbitingCamera.Distance + 10;
            //near = 1;
            //near = 1;
            //far = 100;
            //far = 100;
            //near = OrbitingCamera.Distance/2;
            near = 0.1f;
            far = 50f;
            OrbitingCamera.Projection = Matrix4.CreatePerspectiveFieldOfView(1f, _aspect, near, far);
            var lightSize = 20;
            directionalLight.Projection = Matrix4.CreateOrthographic(lightSize * _aspect, lightSize, 0.1f , 50);
            //OrbitingCamera.Projection = Matrix4.CreatePerspectiveFieldOfView(1f, _aspect,0.1f, 10);
            //directionalLight.Projection = Matrix4.CreatePerspectiveFieldOfView(1f, _aspect, 0.1f, 100);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            #region Draw G Buffer
            //_shaderProgram.Bind();
            gbuffer.Bind();

            gbuffer.cache.Draw(() =>
            {
                DrawData(time);
            });


            //Copy Depth Buffer

            var fwidth = gbuffer.cache.GetTexture(FramebufferAttachment.DepthAttachment).Width;
            var fheight = gbuffer.cache.GetTexture(FramebufferAttachment.DepthAttachment).Height;
            GL.BlitNamedFramebuffer(gbuffer.cache.Handle, 0, 0, 0, fwidth, fheight, 0, 0, fwidth, fheight, ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit, BlitFramebufferFilter.Nearest);

            #endregion

            //OrbitingCamera.Target = Vector3.Zero;

            //Calculate Shadowmap
            //TODO Shadowmap
            //http://www.codinglabs.net/tutorial_opengl_deferred_rendering_shadow_mapping.aspx

            #region Draw Directional Light Depth
            var cullface = false;

            if(cullface)GL.CullFace(CullFaceMode.Front);
            depthShader.Bind();
            map.Cache.Draw(() =>
            {
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
                var viewPro = directionalLight.ViewProjection;
                depthShader.Uniform("lightSpaceMatrix", viewPro);

                foreach (var (globalTransform, drawable, material) in _modelRenderer.TraverseSceneGraphDrawables())
                {
                    depthShader.Uniform("model", globalTransform);
                    drawable.Draw();
                }
            });
            if (cullface)GL.CullFace(CullFaceMode.Back);


            emptyVA.Bind();
            lightDefShader.Bind();

            #endregion

            #region Draw PBR
            var lightPos = instanceLightPositions;
            var lightColors = new List<Vector3> { (1, 0.7f, 1), (1, 1, 1), (1, 1, 1), (1, 1, 1), };
            for(int i=0;i<lightColors.Count;i++)
            {
                lightColors[i] *= 10;
            }
            var lightPosLocation = lightDefShader.Shader.CheckedUniformLocation("lightPositions[0]");
            var lightColorsLocation = lightDefShader.Shader.CheckedUniformLocation("lightColors[0]");

            if (lightPosLocation != -1)
                lightDefShader.Shader.Uniform(lightPosLocation, lightPos);
            if (lightColorsLocation != -1)
                lightDefShader.Shader.Uniform(lightColorsLocation, lightColors.ToArray());

            

            lightDefShader.AlbedoMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment2));
            lightDefShader.NormalMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment1));
            lightDefShader.PosMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment0));
            lightDefShader.MetalRoughtnessMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment3));
            lightDefShader.EnvMap?.Bind(skyboxTexture);
            lightDefShader.DepthMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.DepthAttachment));
            lightDefShader.PositionRawMap?.Bind(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment4));

            lightDefShader.IrradianceMap?.Bind(irradianceMap);



            #region Directional Light Variables
            var dirLightPos = directionalLight.CalcPosition();
            if(lightDefShader.DirectionalLightDirection != null)
            {
                var dir = dirLightPos - directionalLight.Target;
                lightDefShader.DirectionalLightDirection.Value = dir.Normalized(); //dir.Normalized();
            }
            //lightDefShader.DirectionalLightMatrix.Value = directionalLight.ViewProjection;
            if (lightDefShader.DirectionalLightMatrix != null)
            {
                lightDefShader.DirectionalLightMatrix.Value = directionalLight.ViewProjection;
            }

            lightDefShader.DirectionalLightShadowMap?.Bind(map.Cache.GetTexture(FramebufferAttachment.ColorAttachment0));

            if (lightDefShader.CameraPos != null)
            {
                var pos = OrbitingCamera.CalcPosition();
                lightDefShader.CameraPos.Value = pos;
                //Console.WriteLine(pos);
            }
            #endregion

            GL.DepthMask(false);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            GL.DepthMask(true);

            #endregion


            #region Draw Instanced Lights
            var drawLights = true;
            if (drawLights)
            {
                shaderLightBallsInstanced.Bind();

                shaderLightBallsInstanced.Uniform("world", Matrix4.Identity);
                shaderLightBallsInstanced.Uniform("camera", OrbitingCamera.ViewProjection);
                shaderLightBallsInstanced.Uniform("cameraPos", OrbitingCamera.CalcPosition());

                lightSphere.VertexArray.Bind();
                //GL.DrawArraysInstanced(PrimitiveType.Triangles, 0, lightSphere.IndexCount, instanceLightPositions.Length);
                GL.DrawElementsInstanced(PrimitiveType.Triangles, lightSphere.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, instanceLightPositions.Length);
            }

            #endregion
            #region Draw Skybox
            var drawSkybox = true;
            if (drawSkybox)
            {
                GL.DepthMask(false);
                GL.DepthFunc(DepthFunction.Equal);
                box.Draw(OrbitingCamera);
                GL.DepthFunc(DepthFunction.Less);
                GL.DepthMask(true);
            }

            #endregion



            overlayCacheTexture.Draw(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment0));
            //overlayDepthTexture.Draw(gbuffer.cache.GetTexture(FramebufferAttachment.DepthAttachment));
            overlayDepthTexture.Draw(map.Cache.GetTexture(FramebufferAttachment.DepthAttachment));
            overlayTextureRight.Draw(gbuffer.cache.GetTexture(FramebufferAttachment.ColorAttachment1));

            //texCalc.CalculateIrradianceMap(skyboxTexture);
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
            gbuffer.Program.Uniform("cameraPos", OrbitingCamera.CalcPosition());
            //_shaderProgram.Uniform("cameraPos", OrbitingCamera.CalcPosition());

            //var locBaseColor = _shaderProgram.CheckedUniformLocation("baseColor");
            var locWorld = gbuffer.Program.CheckedUniformLocation("world");

            /*
             * uniform vec3[4] lightPositions;
uniform vec3[4] lightColors;
             * 
             */
            _model.UpdateAnimations(time);



            int pos = 0;
            foreach (var (globalTransform, drawable, material) in _modelRenderer.TraverseSceneGraphDrawables())
            {
                state.Add(globalTransform);
                gbuffer.Program.Uniform(locWorld, globalTransform);

                if (material != null)
                {
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

                map.Resize(width, height);
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
        private ShaderProgram shaderLightBallsInstanced;
        private MeshGL lightSphere;
        private GBUffer gbuffer;
        private GLTFTextures textures;
        private Vector3[] instanceLightPositions;
        private TextureCalculator texCalc;
        private CubeTexture irradianceMap;
        private BufferGL lightSpherePositionBuffer;
        private VertexAttrib lightInstancePos;
        private ShaderProgram depthShader;

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

        internal void SetLightDir()
        {
            directionalLight.Azimuth = OrbitingCamera.Azimuth;
            directionalLight.Elevation = OrbitingCamera.Elevation;
            //directionalLight.Projection = OrbitingCamera.Projection;
            directionalLight.Target = OrbitingCamera.Target;
            directionalLight.Distance = OrbitingCamera.Distance;
        }
    }
}
