using Framework;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Diagnostics;

namespace Example
{
	internal class Program
	{
		private static void Main()
		{
			var window = new DebugGameWindow();
			using var view = new View();
			var stopWatch = Stopwatch.StartNew();
			window.KeyDown += args =>
			{
				switch (args.Key)
				{
					case Keys.Escape: window.Close(); break;
					case Keys.Enter: stopWatch = Stopwatch.StartNew(); break;
					case Keys.Space: view.SetLightDir(); break;
				}
			};

			var mouseState = window.MouseState;
			window.MouseMove += args =>
			{

				if (mouseState.IsButtonDown(MouseButton.Left))
				{
					view.OrbitingCamera.Azimuth += 300 * args.DeltaX / window.Size.X;
					view.OrbitingCamera.Elevation += 300 * args.DeltaY / window.Size.Y;
				}
			};

			float time()
			{
				var t = (float)stopWatch.Elapsed.TotalSeconds;
				if (t > 1.6f)
				{
					t = 0f;
					stopWatch.Restart();
				}
				return t;
			}

			float lastTime = 0;
			void ShowFPS(float time)
            {
				float deltaTime = time-lastTime;
				lastTime = time;

				window.Title = $"{Math.Round( 1/deltaTime * 1000)}";
            }

			window.UpdateFrame += args => view.OrbitingCamera.Distance *= MathF.Pow(1.05f, mouseState.ScrollDelta.Y);
			window.RenderFrame += _ => view.Draw((float)stopWatch.Elapsed.TotalMilliseconds);
			window.RenderFrame += _ => window.SwapBuffers();
			window.RenderFrame += _ => ShowFPS((float)stopWatch.Elapsed.TotalMilliseconds);
			window.Resize += (window) => view.Resize(window.Width, window.Height);
			window.Minimized += _ => view.SetActiveState(false);
			window.FocusedChanged += _ => view.SetActiveState(true);
			window.Run();
		}
	}
}