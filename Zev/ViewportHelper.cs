using OpenTK.Graphics.OpenGL4;

namespace Example.Zev
{
    public class ViewportHelper
    {
        private int[] viewport;

        public void GetCurrentViewPort()
        {
            viewport = new int[4];
            GL.GetInteger(GetPName.Viewport, viewport);
        }

        public void SetLastViewport()
        {
            GL.Viewport(viewport[0], viewport[1], viewport[2], viewport[3]);
        }
    }
}
