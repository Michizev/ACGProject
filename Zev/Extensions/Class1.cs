using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zev;

namespace Example.Zev.Extensions
{
    public static class Class1
    {
        public static void Bind(this UniformCubeTexture tex,CubeTexture texture)
        {
	        GL.BindTextureUnit(tex.TextureSampler, texture.Handle);
        }
	}
}
