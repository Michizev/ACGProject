using Framework;
using ImageMagick;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Zev
{
    public class CubeTexture
    {
        public CubeTexture(SizedInternalFormat format)
        {
            Format = format;

            GL.CreateTextures(TextureTarget.TextureCubeMap, 1, out int handle);
            //var texture = new CubeTexture(image.Width, image.Height, internalFormat);

            GL.BindTexture(TextureTarget.TextureCubeMap, handle);

            //GL.TextureSubImage2D(texture.Handle, 0, 0, 0, image.Width, image.Height, format, PixelType.UnsignedByte, bytes);
            //GL.GenerateTextureMipmap(texture.Handle);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureParameterName.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureParameterName.ClampToEdge);


            Handle = handle;
        }

        public int Handle { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public SizedInternalFormat Format { get; }

        internal void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }

    
    
    public class CubeTextureLoaderData
    {
        public enum CubeMapSide { RIGHT, LEFT, TOP, BOTTOM, FRONT, BACK };
        public Dictionary<CubeMapSide, string> textureNames;
        public Dictionary<CubeMapSide, string> sideNames;
        List<CubeMapSide> openglCubeOrder = new List<CubeMapSide>() { CubeMapSide.LEFT, CubeMapSide.RIGHT, CubeMapSide.TOP, CubeMapSide.BOTTOM, CubeMapSide.FRONT, CubeMapSide.BACK };
        public enum CubeMapPresets { FULLNAMECAP,SPACESCAPE,NAME };
        public void LoadPreset(CubeMapPresets preset)
        {
            switch (preset)
            {
                case CubeMapPresets.FULLNAMECAP:
                    sideNames.Add(CubeMapSide.BACK, "Back");
                    sideNames.Add(CubeMapSide.FRONT, "Front");
                    sideNames.Add(CubeMapSide.RIGHT, "Right");
                    sideNames.Add(CubeMapSide.LEFT, "Left");
                    sideNames.Add(CubeMapSide.TOP, "Top");
                    sideNames.Add(CubeMapSide.BOTTOM, "Bottom");
                    break;
                case CubeMapPresets.SPACESCAPE:
                    sideNames.Add(CubeMapSide.BACK, "BK");
                    sideNames.Add(CubeMapSide.FRONT, "FT");
                    sideNames.Add(CubeMapSide.RIGHT, "RT");
                    sideNames.Add(CubeMapSide.LEFT, "LF");
                    sideNames.Add(CubeMapSide.TOP, "UP");
                    sideNames.Add(CubeMapSide.BOTTOM, "DN");
                    break;
                case CubeMapPresets.NAME:
                    sideNames.Add(CubeMapSide.BACK, "back");
                    sideNames.Add(CubeMapSide.FRONT, "front");
                    sideNames.Add(CubeMapSide.RIGHT, "right");
                    sideNames.Add(CubeMapSide.LEFT, "left");
                    sideNames.Add(CubeMapSide.TOP, "top");
                    sideNames.Add(CubeMapSide.BOTTOM, "bottom");
                    break;

            }

        }
        public string ResourceDir { get; set; }

        public CubeTextureLoaderData()
        {
            textureNames = new Dictionary<CubeMapSide, string>();
            sideNames = new Dictionary<CubeMapSide, string>();
            ResourceDir = "";
        }

        public IEnumerable<Stream> GetTextureStreams()
        {
            foreach (var n in openglCubeOrder)
            {
                var tex = textureNames[n];
                using var stream = Resource.LoadStream(tex);
                Console.WriteLine($"LOADING {tex}");
                yield return stream;
            }
        }
        public void GenerateNames(string filename, string fileending)
        {
            textureNames = new Dictionary<CubeMapSide, string>();
            foreach (var t in sideNames)
            {
                textureNames.Add(t.Key, $"{ResourceDir}{filename}{t.Value}.{fileending}");
            }
        }
    }
    public static class CubeTextureLoader
    {
        public static CubeTexture MakeEmptyCubeMap(int width, int height)
        {
            var texture = new CubeTexture(SizedInternalFormat.Rgba16f);
            GL.BindTexture(TextureTarget.TextureCubeMap, texture.Handle);

            var pixelFormat = PixelInternalFormat.Rgba16f;
            var pixelType = PixelFormat.Rgb;
            for (int i = 0; i < 6; ++i)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + (int)i, 0, pixelFormat, width,height, 0, pixelType, PixelType.Float, IntPtr.Zero);
            }
            texture.SetSize(width, height);
            return texture;
        }
        public static CubeTexture Load(CubeTextureLoaderData data)
        {
            var texture = new CubeTexture(SizedInternalFormat.Rgba8);

            int width = 0;
            int height = 0;

            GL.BindTexture(TextureTarget.TextureCubeMap, texture.Handle);
            int i = 0;
            foreach (var stream in data.GetTextureStreams())
            {
                var img = LoadImage(stream);
                img.Flop();
                var bytes = img.GetPixelsUnsafe().ToArray();
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + (int)i++, 0, PixelInternalFormat.Rgb, img.Width, img.Height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, bytes);
            }
            GL.GenerateTextureMipmap(texture.Handle);

            texture.SetSize(width, height);
            return texture;
        }

        private static MagickImage LoadImage(Stream stream)
        {
            var image = new MagickImage(stream);
            var width = image.Width;
            var height = image.Height;

            var format = PixelFormat.Rgb;
            var internalFormat = (SizedInternalFormat)All.Rgb8; //till OpenTK issue resolved on github https://github.com/opentk/opentk/issues/752
            switch (image.ChannelCount)
            {
                case 2: format = PixelFormat.LuminanceAlpha; internalFormat = SizedInternalFormat.Rg8; break;
                case 3: break;
                case 4: format = PixelFormat.Rgba; internalFormat = SizedInternalFormat.Rgba8; break;
                default: throw new ArgumentOutOfRangeException("Unexpected image format");
            }

            
            
            return image;
        }
    }
}
