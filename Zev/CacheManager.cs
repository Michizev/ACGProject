using Framework;
using System;

namespace Example.Zev
{
    class CacheManager : IResizeable, IDisposable
    {
        public ICacheMaker maker;
        public FrameBufferGL cache;

        public CacheManager(ICacheMaker maker)
        {
            this.maker = maker;
            cache = maker.CreateCache(2, 2);
        }

        public void Dispose()
        {
            ((IDisposable)cache).Dispose();
        }

        public void Resize(int width, int height)
        {
            cache.Dispose();
            cache = maker.CreateCache(width, height);
        }

    }
}
