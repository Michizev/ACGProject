using Framework;
using OpenTK.Graphics.OpenGL4;

namespace Example.Zev
{
    class ShadowMap : ResizeableBaseCache
    {
        public ShadowMap() : base(new DepthCache())
        {

        }

    }

    class SimpleHDRCache : ICacheMaker
    {
        public FrameBufferGL CreateCache(int width, int height)
        {
            var cache = new FrameBufferGL(true);
            var blurTexture = new Texture(width, height, SizedInternalFormat.Rgba16f)
            {
                Function = TextureWrapMode.ClampToBorder
            };
            cache.Attach(blurTexture, FramebufferAttachment.ColorAttachment0);
            return cache;
        }
    }

    class SingleHDRBuffer : ResizeableBaseCache
    {
        public SingleHDRBuffer() : base(new SimpleHDRCache())
        {

        }
    }

    class HDRCache : ICacheMaker
    {
        public FrameBufferGL CreateCache(int width, int height)
        {
            var cache = new FrameBufferGL(true);
            cache.Attach(new Texture(width, height, SizedInternalFormat.Rgba16f), FramebufferAttachment.ColorAttachment0);
            var blurTexture = new Texture(width, height, SizedInternalFormat.Rgba16f);
            blurTexture.Function = TextureWrapMode.ClampToBorder;
            cache.Attach(blurTexture, FramebufferAttachment.ColorAttachment1);
            cache.Attach(new Texture(width, height, (SizedInternalFormat)All.DepthComponent32), FramebufferAttachment.DepthAttachment);
            return cache;
        }
    }

    class HDRBuffer : ResizeableBaseCache
    {
        public HDRBuffer() : base(new HDRCache())
        {

        }

    }
}
