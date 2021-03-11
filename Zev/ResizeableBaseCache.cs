using Framework;
using System;

namespace Example.Zev
{
    class ResizeableBaseCache : IResizeable, IDisposable
    {
        protected CacheManager cache;

        public ResizeableBaseCache(ICacheMaker maker)
        {
            this.cache = new CacheManager(maker);
        }

        public void Resize(int width, int height)
        {
            cache.Resize(width, height);
        }

        public void Dispose()
        {
            ((IDisposable)cache).Dispose();
        }

        public FrameBufferGL Cache => cache.cache;
    }
}
