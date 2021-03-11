using Framework;

namespace Example.Zev
{
    interface ICacheMaker
    {
        public FrameBufferGL CreateCache(int width, int height);
    }
}
