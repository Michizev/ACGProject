using ImageMagick;
using System.IO;
using System.Linq;


namespace Zev.HDRIHelper
{
    public class HDRIimage
    {
        public HDRIimage(int width, int height, float[] pixels)
        {
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public int Width { get; }
        public int Height { get; }
        public float[] Pixels { get; }
    }
    public static class HDRILoader
    {
        public static HDRIimage LoadImageAsFloatArray(Stream stream)
        {
            using var image = new MagickImage(stream)
            {
                Format = MagickFormat.Hdr
            };


            image.Flip();
            var bytes = image.GetPixelsUnsafe().ToArray();

            return new HDRIimage(image.Width, image.Height, bytes);
        }
        public static HDRIimage LoadImageAsFloatArray(string path)
        {
            using var image = new MagickImage(path)
            {
                
            };


            image.Flip();
            var bytes = image.GetPixelsUnsafe().ToArray();
            for (int i = 0; i < bytes.Length; i++) bytes[i] *= 0.0001f*0.1f;

            return new HDRIimage(image.Width, image.Height, bytes);
         
        }
    }
}
