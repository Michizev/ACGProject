using Framework;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zev;

namespace Example.Zev
{
    class TextureCalculator
    {
        private ShaderProgram irradianceShader;
        private List<Matrix4> inPV;
        private UniformCubeTexture envCube;
        private MeshGL cube;
        private readonly Matrix4 captureProjection;
        private const string ResourceDir = nameof(Example) + ".content.";
        private readonly Matrix4[] captureViews;
        readonly VertexArray emptyVa = new VertexArray();
        public TextureCalculator()
        {
            irradianceShader = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(ResourceDir + "irradiance"));
            captureProjection = Matrix4.CreatePerspectiveFieldOfView(1.5708f, 1, 0.1f, 10f);

            var box = "bix.obj";
            var vars = new ShaderProgramVariables(irradianceShader);
            var pos = vars.Get<VertexAttrib>("position");
            envCube = vars.Get<UniformCubeTexture>("environmentMap");
            cube = MeshToGL.Create(Framework.ObjLoader.LoadFromResource(ResourceDir, box),pos);

            var eye = Vector3.Zero;
            captureViews = new List<Matrix4>
            {
                Matrix4.LookAt(eye,new Vector3(1.0f, 0.0f, 0.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                Matrix4.LookAt(eye,new Vector3(-1.0f, 0.0f, 0.0f),new Vector3(0.0f, -1.0f, 0.0f)),
                Matrix4.LookAt(eye,new Vector3(0.0f, 1.0f, 0.0f), new Vector3(0.0f, 0.0f, 1.0f)),
                Matrix4.LookAt(eye,new Vector3(0.0f, -1.0f, 0.0f),new Vector3(0.0f, 0.0f, -1.0f)),
                Matrix4.LookAt(eye,new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, -1.0f, 0.0f)),
                Matrix4.LookAt(eye,new Vector3(0.0f, 0.0f, -1.0f),new Vector3(0.0f, -1.0f, 0.0f))
            }.ToArray();

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
            foreach (var v in captureViews)
            {
                var m = captureProjection * v;
                m.Invert();
                inPV.Add(m);
            }
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

            irradianceShader.Uniform("projection", captureProjection);

            var cache = new FrameBufferGL();
            cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
            cache.Attach(new Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);
            //TODO FRAMEBUFFER

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, cache.Handle);
            
            //GL.DepthFunc(DepthFunction.Equal);
            for (int i = 0; i < 6; ++i)
            {
                irradianceShader.Uniform("view", captureViews[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0,
                                       TextureTarget.TextureCubeMapPositiveX + i, irradianceMap.Handle, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit| ClearBufferMask.DepthBufferBit);

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
    }
}
