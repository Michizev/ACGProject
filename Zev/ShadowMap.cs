using Framework;
using OpenTK.Graphics.OpenGL4;

namespace Example.Zev
{
    class ShadowMap : ResizeableBaseCache
    {
        public ShadowMap() : base(new DepthCache(TextureWrapMode.ClampToEdge))
        {
            var tex1 = Cache.GetTexture(FramebufferAttachment.ColorAttachment0);
            tex1.Function = TextureWrapMode.ClampToEdge;
           
            
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

        public void SetFilterNearest()
        {
            var t = cache.cache.GetTexture(FramebufferAttachment.ColorAttachment0);
            t.MinFilter = TextureMinFilter.Linear;
            t.MagFilter = TextureMagFilter.Linear;
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
