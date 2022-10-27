using Framework;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using Zev;
using Zev.HDRIHelper;

namespace Example.Zev
{
    class TextureCalculator
    {
        private ShaderProgram irradianceShader;
        private ShaderProgram HDRIToCubemapShader;
        private readonly ShaderProgram specularPrefilterShader;
        private readonly ShaderProgram brdfPrefilterShader;
        public ShaderProgram CubeShadow { get; }
        private List<Matrix4> inPV;
        private UniformCubeTexture envCube;
        private MeshGL cube;

        private const string ResourceDir = nameof(Example) + ".content.";
        public Matrix4[] CaptureViews { get; }
        public Matrix4 CaptureProjection { get; }
        readonly VertexArray emptyVa = new VertexArray();
        public TextureCalculator()
        {
            irradianceShader = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "irradiance"));
            HDRIToCubemapShader = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "Shader." + "equi2Cube"));
            specularPrefilterShader = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "Shader.specular." + "preFilter"));
            brdfPrefilterShader = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "Shader.specular." + "preBrdf"));
            CubeShadow = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "Shader.pointLight." + "shadow"));
            //specularIrradianceShader = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "Shader." + "specular"));
            CaptureProjection = Matrix4.CreatePerspectiveFieldOfView(1.5708f, 1, 0.1f, 10f);

            var box = "bix.obj";
            var vars = new ShaderProgramVariables(irradianceShader);
            var pos = vars.Get<VertexAttrib>("position");

            envCube = vars.Get<UniformCubeTexture>("environmentMap");
            cube = MeshToGL.Create(Framework.ObjLoader.LoadFromResource(ResourceDir, box), pos);

            var eye = Vector3.Zero;
            CaptureViews=DirsFromEye(eye);

            var viewDirs = new List<Vector3>
            {
                new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, 0.0f, -1.0f)
            };
            int i = 0;
            inPV = new List<Matrix4>();
            foreach (var v in CaptureViews)
            {
                var m = CaptureProjection * v;
                m.Invert();
                inPV.Add(m);
            }
        }

        public Matrix4[] DirsFromEye(Vector3 eye)
        {
            var list = new List<Matrix4>
            {
                Matrix4.LookAt(eye,eye+new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                Matrix4.LookAt(eye,eye+new Vector3(-1.0f, 0.0f, 0.0f),new Vector3(0.0f, -1.0f, 0.0f)),
                Matrix4.LookAt(eye,eye+new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                Matrix4.LookAt(eye,eye+new Vector3(0.0f, -1.0f, 0.0f),new Vector3(0.0f, 0.0f, -1.0f)),
                Matrix4.LookAt(eye,eye+new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                Matrix4.LookAt(eye,eye+new Vector3(0.0f, 0.0f, -1.0f),new Vector3(0.0f, -1.0f, 0.0f))
            }.ToArray();
            return list;
        }

        void drawCube()
        {
            cube.VertexArray.Bind();
            //GL.DrawElements(PrimitiveType.Triangles, cube.IndexCount, DrawElementsType.UnsignedByte,0);
            GL.Disable(EnableCap.CullFace);
            //GL.DrawArrays(PrimitiveType.Triangles, 0, cube.IndexCount);
            GL.DrawElements(PrimitiveType.Triangles, cube.IndexCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.Enable(EnableCap.CullFace);
        }

        public Texture PreBDRF()
        {
            var width = 512;
            var height = 512;
            var internalFormat = SizedInternalFormat.Rgba16f;
            var format = PixelFormat.Rgb;

            using var cache = new FrameBufferGL(false);
            var tex = new Texture(width, height, SizedInternalFormat.Rgba16f);
            
            cache.Attach(tex, FramebufferAttachment.ColorAttachment0);

            brdfPrefilterShader.Bind();

            emptyVa.Bind();
            cache.Draw(() =>
            {
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            });

            tex.MinFilter = TextureMinFilter.Nearest;
            tex.MagFilter = TextureMagFilter.Nearest;
            //GL.GenerateTextureMipmap(tex.Handle);
            return tex;
        }


        public Texture HDRIToTexture(HDRIimage image)
        {
            var internalFormat = SizedInternalFormat.Rgba16f;
            var format = PixelFormat.Rgb;
            var texture = new Texture(image.Width, image.Height, internalFormat)
            {
                Function = TextureWrapMode.ClampToEdge,
                //Function = TextureWrapMode.MirroredRepeat,
                MagFilter = TextureMagFilter.Linear,
                MinFilter = TextureMinFilter.Linear
            };

            GL.TextureSubImage2D(texture.Handle, 0, 0, 0, image.Width, image.Height, format, PixelType.Float, image.Pixels);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return texture;
        }
        public CubeTexture TurnHDRIIntoCubemap(Texture baseTexture)
        {
            var shader = HDRIToCubemapShader;
            var port = new ViewportHelper();
            port.GetCurrentViewPort();
            shader.Bind();
            //irradianceShader.Uniform("environmentMap", 0);
            //irradianceShader.Uniform("camera", captureProjection);

            var width = 512;
            var height = 512;
            var irradianceMap = CubeTextureLoader.MakeEmptyCubeMap(width, height);

            

            GL.BindTexture(TextureTarget.Texture2D, baseTexture.Handle);
            //GL.BindTexture(TextureTarget.TextureCubeMap, baseTexture.Handle);
            // GL.GetInteger(GetIndexedPName.Viewport, 3, out var data);
            //Console.WriteLine(data);
            GL.Viewport(0, 0, width, height); // don't forget to configure the viewport to the capture dimensions.

            shader.Uniform("projection", CaptureProjection);

            var cache = new FrameBufferGL();
            cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
            cache.Attach(new Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);
            //TODO FRAMEBUFFER

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, cache.Handle);

            //GL.DepthFunc(DepthFunction.Equal);
            for (int i = 0; i < 6; ++i)
            {
                shader.Uniform("view", CaptureViews[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                       TextureTarget.TextureCubeMapPositiveX + i, irradianceMap.Handle, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                drawCube();
                /*
                //Draws an empty vertex array
                emptyVa.Bind();
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                */
            }

            
            GL.GenerateTextureMipmap(irradianceMap.Handle);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //GL.DepthFunc(DepthFunction.Less);

            port.SetLastViewport();

            return irradianceMap;
        }

        internal CubeTexture CalculateSpecularPrefilterMap(CubeTexture skyboxTexture)
        {
            var shader = specularPrefilterShader;
            var width = 128;
            var height = 128;
            var levels = 5;
            var port = new ViewportHelper();
            port.GetCurrentViewPort();
            

            

            using var cache = new FrameBufferGL();
            cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
            //cache.Attach(new Texture(width, height, (SizedInternalFormat)All.DepthComponent32, levels = 5), FramebufferAttachment.DepthAttachment);

            var map = CubeTextureLoader.MakeEmptyCubeMap(width, height, mipmap: 5);

           

            

            

            
            //texture?.Bind(hdriTexture);
            
            
            var targetTexture = skyboxTexture;
            int maxMipLevels = 5;
            var vars = new ShaderProgramVariables(shader);

            var texture = vars.Get<UniformCubeTexture>("environmentMap");
            var viewMatrixLocation = vars.Get<UniformMatrix4>("view");
            shader.Bind();
            shader.Uniform("projection", CaptureProjection);

            GL.BindTexture(TextureTarget.TextureCubeMap, skyboxTexture.Handle);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, cache.Handle);
            for (int mip = 0; mip < maxMipLevels; ++mip)
            {
                // resize framebuffer according to mip-level size.
                int mipWidth = (int)(width * Math.Pow(0.5, mip));
                int mipHeight = (int)(height * Math.Pow(0.5, mip));
                GL.Viewport(0, 0, mipWidth, mipHeight);
                //GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, cache.Handle);
                //GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent32, mipWidth, mipHeight);
                
                float roughness = (float)mip / (float)(maxMipLevels - 1);
                shader.Uniform("roughness", roughness);

                RenderToCube(viewMatrixLocation, map, mip);
            }
            port.SetLastViewport();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return map;
        }

        public CubeTexture TurnHDRIIntoCubemapDead(Texture hdriTexture)
        {
            var shader = HDRIToCubemapShader;
            var width = 512;
            var height = 512;

            var port = new ViewportHelper();
            port.GetCurrentViewPort();
            GL.Viewport(0, 0, width, height); // don't forget to configure the viewport to the capture dimensions.

            shader.Bind();

            using var cache = new FrameBufferGL();
            cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
            //cache.Attach(new Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);

            var map = CubeTextureLoader.MakeEmptyCubeMap(width, height);

            var vars = new ShaderProgramVariables(shader);
            
            var viewMatrixLocation = vars.Get<UniformMatrix4>("view");

            shader.Uniform("projection", CaptureProjection);

            var texture = vars.Get<UniformTexture>("equirectangularMap");
            //texture?.Bind(hdriTexture);
            GL.BindTexture(TextureTarget.Texture2D, hdriTexture.Handle);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, cache.Handle);
            /*
            cache.Draw(() =>
            {
                RenderToCube(viewMatrixLocation, map);
            });
            */
            RenderToCube(viewMatrixLocation, map);
            //GL.GenerateTextureMipmap(hdriTexture.Handle);

            port.SetLastViewport();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return map;
        }
        public CubeTexture CalculateIrradianceMap(CubeTexture baseTexture)
        {
            var port = new ViewportHelper();
            port.GetCurrentViewPort();
            irradianceShader.Bind();
            //irradianceShader.Uniform("environmentMap", 0);
            //irradianceShader.Uniform("camera", captureProjection);

            var width = 32;
            var height = 32;
            var irradianceMap = CubeTextureLoader.MakeEmptyCubeMap(width, height);


            GL.BindTexture(TextureTarget.TextureCubeMap, baseTexture.Handle);
            //GL.BindTexture(TextureTarget.TextureCubeMap, baseTexture.Handle);
            // GL.GetInteger(GetIndexedPName.Viewport, 3, out var data);
            //Console.WriteLine(data);
            GL.Viewport(0, 0, width, height); // don't forget to configure the viewport to the capture dimensions.

            irradianceShader.Uniform("projection", CaptureProjection);

            var cache = new FrameBufferGL();
            cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
            cache.Attach(new Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);
            //TODO FRAMEBUFFER

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, cache.Handle);
           
            //GL.DepthFunc(DepthFunction.Equal);
            for (int i = 0; i < 6; ++i)
            {
                irradianceShader.Uniform("view", CaptureViews[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                       TextureTarget.TextureCubeMapPositiveX + i, irradianceMap.Handle, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                drawCube();
                /*
                //Draws an empty vertex array
                emptyVa.Bind();
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                */
            }

            
            GL.GenerateTextureMipmap(irradianceMap.Handle);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //GL.DepthFunc(DepthFunction.Less);

            port.SetLastViewport();

            return irradianceMap;
        }
        public CubeTexture CalculateIrradianceMapDead(CubeTexture baseTexture)
        {
            var shader = irradianceShader;
            var port = new ViewportHelper();
            var width = 32;
            var height = 32;

            GL.UseProgram(0);
            var irradianceMap = CubeTextureLoader.MakeEmptyCubeMap(width, height);
            //port.GetCurrentViewPort();
            //GL.Viewport(0, 0, width, height); // don't forget to configure the viewport to the capture dimensions.
            shader.Bind();

            var vars = new ShaderProgramVariables(irradianceShader);
            var viewMatrixLocation = vars.Get<UniformMatrix4>("view");
            var cubeTexture = vars.Get<UniformCubeTexture>("environmentMap");
            var targetTexture = irradianceMap;

            shader.Uniform("projection", CaptureProjection);
            cubeTexture.Value = baseTexture.Handle;

            using var cache = new FrameBufferGL();
            cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
            cache.Attach(new Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);

            cache.Draw(() =>
            {
                RenderToCube(viewMatrixLocation, targetTexture);
            });
            //irradianceShader.Uniform("camera", captureProjection);





            //GL.BindTexture(TextureTarget.TextureCubeMap, baseTexture.Handle);
            //GL.BindTexture(TextureTarget.TextureCubeMap, baseTexture.Handle);
            // GL.GetInteger(GetIndexedPName.Viewport, 3, out var data);
            //Console.WriteLine(data);



            /*
            using var cache = new FrameBufferGL();
            cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
            cache.Attach(new Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);
            //TODO FRAMEBUFFER

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, cache.Handle);

            
            var vars = new ShaderProgramVariables(irradianceShader);
            var viewMatrixLocation = vars.Get<UniformMatrix4>("view");
            var targetTexture = irradianceMap;
            //GL.DepthFunc(DepthFunction.Equal);
            RenderToCube(viewMatrixLocation, targetTexture);

            GL.GenerateTextureMipmap(targetTexture.Handle);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            //GL.DepthFunc(DepthFunction.Less);

            port.SetLastViewport();
            */
            return irradianceMap;
        }

        private void RenderToCube(UniformMatrix4 viewMatrixLocation, CubeTexture targetTexture, int level = 0)
        {
            for (int i = 0; i < 6; ++i)
            {
                //shader.Uniform("view", captureViews[i]);
                viewMatrixLocation.Value = CaptureViews[i];
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                       TextureTarget.TextureCubeMapPositiveX + i, targetTexture.Handle, level);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                //GL.Clear(ClearBufferMask.ColorBufferBit);

                drawCube();
                /*
                //Draws an empty vertex array
                emptyVa.Bind();
                GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
                */
            }
        }
    }
}
