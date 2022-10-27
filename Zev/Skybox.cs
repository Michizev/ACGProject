using Framework;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Zev
{
    class Skybox
    {
        static float[] skyboxVertices = {
            // positions          
            -1.0f,  1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

            -1.0f,  1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f,  1.0f
        };
        static float[] vertices = {
    1.0f,    1.0f,    1.0f,
    0.0f,   1.0f,    1.0f,
    1.0f,    1.0f,    0.0f,
    0.0f,   1.0f,    0.0f,
    1.0f,    0.0f,   1.0f,
    0.0f,   0.0f,   1.0f,
    0.0f,   0.0f,   0.0f,
    1.0f,    0.0f,   0.0f
};
    float[]   cube_vertices = {
    // front
    -1.0f, -1.0f,  1.0f,
     1.0f, -1.0f,  1.0f,
     1.0f,  1.0f,  1.0f,
    -1.0f,  1.0f,  1.0f,
    // back
    -1.0f, -1.0f, -1.0f,
     1.0f, -1.0f, -1.0f,
     1.0f,  1.0f, -1.0f,
    -1.0f,  1.0f, -1.0f
  };
        byte[] cube_elements = {
		// front
		0, 1, 2,
		2, 3, 0,
		// right
		1, 5, 6,
		6, 2, 1,
		// back
		7, 6, 5,
		5, 4, 7,
		// left
		4, 0, 3,
		3, 7, 4,
		// bottom
		4, 5, 1,
		1, 0, 4,
		// top
		3, 2, 6,
		6, 7, 3
	};


        // Declares the Elements Array, where the indexs to be drawn are stored
        static int[] elements = {
    3, 2, 6, 7, 4, 2, 0,
    3, 1, 6, 5, 4, 1, 0
};


        public VertexArray Va;
        public VertexArray emptyVa = new VertexArray();
        private ShaderProgram _shaderProgram;
        private const string resourceDir = nameof(Example) + ".content.";

        private VertexAttrib pos;
        private MeshGL mesh;
        CubeTexture tex;
        public Skybox(CubeTexture cubeTex)
        {
            LoadShader();
            //LoadMesh(pos.Location);
            //mesh= MeshToGL.Create(Framework.ObjLoader.LoadFromResource(resourceDir, "meshSkybox.obj"), pos);
            tex = cubeTex;
        }

        public void LoadShader()
        {
            _shaderProgram = ShaderTools.PrintExceptions(() => ShaderProgramLoader.FromResourcePrefix(resourceDir + "lessSkybox"));
            var vars = new ShaderProgramVariables(_shaderProgram);

            //pos = vars.Get<VertexAttrib>("position");
        }
        public void LoadMesh(int positionLocation)
        {
            var buf = new BufferGL();
            buf.Set(cube_vertices, BufferUsageHint.StaticDraw);
            Va = new VertexArray();

            var ind = new BufferGL();
            ind.Set(cube_elements, BufferUsageHint.StaticDraw);
            Va.BindAttribute(positionLocation, buf, 3, sizeof(float), VertexAttribType.Float);
            Va.BindIndices(ind);
        }

        public void Draw(ICamera camera)
        {
            var cam = camera.ViewProjection;

            var inCam =  camera.View* camera.Projection;
            inCam.Invert();
            
            var cam2 =cam.ClearTranslation();
            var camTest = cam;
            camTest.Column3 = new Vector4(0, 0, 0, 1);
            camTest.Row3 = new Vector4(0, 0, 0, 1);
            //if (camTest != cam2) Debugger.Break();
            _shaderProgram.Bind();
            _shaderProgram.Uniform("invPV", inCam);

            //Va.Bind();
            //mesh.VertexArray.Bind();
            emptyVa.Bind();
            GL.BindTexture(TextureTarget.TextureCubeMap, tex.Handle);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
            //GL.DrawElements(PrimitiveType.Triangles, mesh.IndexCount, DrawElementsType.UnsignedInt,0);
        }
    }
}
