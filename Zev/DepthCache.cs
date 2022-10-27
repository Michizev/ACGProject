using Framework;
using OpenTK.Graphics.OpenGL4;

namespace Example.Zev
{
    class DepthCache : ICacheMaker
    {
        TextureWrapMode wrapMode;

        public DepthCache(TextureWrapMode wrapMode = TextureWrapMode.Repeat)
        {
            this.wrapMode = wrapMode;
        }

        public FrameBufferGL CreateCache(int width, int height)
        {
            var cache = new FrameBufferGL(true);

            //cache.Attach(new RenderBufferGL(RenderbufferStorage.DepthComponent32, width, height), FramebufferAttachment.DepthAttachment);
            var tex = new Texture(width, height, Texture.DepthComponent32f)
            {
                Function = wrapMode
            };
            cache.Attach(tex, FramebufferAttachment.DepthAttachment);
            tex = new Texture(width, height, SizedInternalFormat.R16f)
            {
                Function = wrapMode
            };
            cache.Attach(tex, FramebufferAttachment.ColorAttachment0);
            return cache;
        }
    }
}
