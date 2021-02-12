using OpenTK.Graphics.OpenGL4;

namespace Framework
{
	/// <summary>
	/// 
	/// </summary>
	/// <seealso cref="Disposable" />
	public class RenderBufferGL : Disposable, IObjectGL
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RenderBufferGL"/> class.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="width">The width.</param>
		/// <param name="height">The height.</param>
		public RenderBufferGL(RenderbufferStorage type, int width, int height)
		{
			GL.CreateRenderbuffers(1, out int handle);
			Handle = handle;
			GL.NamedRenderbufferStorage(handle, type, width, height);
		}

		public int Handle { get; private set; } = -1;

		/// <summary>
		/// Will be called from the default Dispose method.
		/// </summary>
		protected override void DisposeResources() => GL.DeleteRenderbuffer(Handle);
	}
}
