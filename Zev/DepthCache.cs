using Framework;
using OpenTK.Graphics.OpenGL4;

namespace Example.Zev
{
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
}
