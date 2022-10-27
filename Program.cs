using Dear_ImGui_Sample;
using Framework;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Input;
using System;
using System.Diagnostics;
using OpenTK.Mathematics;

namespace Example
{
	internal class Program
	{
        private static ImGuiController _controller;

        private static void Main()
		{
			var window = new DebugGameWindow();

			_controller = new ImGuiController(window.Size.X, window.Size.Y);

            using var view = new View
            {
                imguiController = _controller
            };
            var stopWatch = Stopwatch.StartNew();

			

			window.KeyDown += args =>
			{
				switch (args.Key)
				{
					case Keys.Escape: window.Close(); break;
					case Keys.Enter: stopWatch = Stopwatch.StartNew(); break;
					case Keys.Space: view.SetLightDir(); break;

					case Keys.KeyPadAdd: view.Exposure += 0.1f; break;
					case Keys.KeyPadSubtract: view.Exposure -= 0.1f; break;

					case Keys.G: view.RenderGUI = !view.RenderGUI; break;
					case Keys.P: view.RenderExtraWindows = !view.RenderExtraWindows;break;
					case Keys.B: view.Bloom = !view.Bloom; Console.WriteLine("Set bloom to " + view.Bloom); break;

					case Keys.M: view.UseSSAO = !view.UseSSAO; break;
					case Keys.C: view.Cameras.IncrementCamera(); break;
					case Keys.V: view.CubeViewDirection(1); break;
				}
			};

			var mouseState = window.MouseState;
			window.MouseMove += args =>
			{

				if (mouseState.IsButtonDown(MouseButton.Left))
				{
					view.CurrentCamera.Azimuth += 300 * args.DeltaX / window.Size.X;
					view.CurrentCamera.Elevation += 300 * args.DeltaY / window.Size.Y;
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
			var deltaTime = 0f;
			var lastTime2 = 0f;
			window.UpdateFrame += args =>
			{
				var tmpTime = stopWatch.ElapsedMilliseconds;
				deltaTime = (tmpTime - lastTime2) / 1000f;
				lastTime2 = tmpTime;

				var state = window.KeyboardState;

				var xAxis = 0;
				var yAxis = 0;

				if (state.IsKeyDown(Keys.A))
				{
					xAxis += -1;
				}
				if (state.IsKeyDown(Keys.D))
				{
					xAxis += 1;
				}
				if (state.IsKeyDown(Keys.W))
				{
					yAxis += -1;
				}
				if (state.IsKeyDown(Keys.S))
				{
					yAxis += 1;
				}

				view.FpsCamera.Movement = new Vector3(xAxis, yAxis, 0) * deltaTime;
			};


			window.RenderFrame += e =>
			{
				if (view.RenderGUI) _controller.Update(window, (float)e.Time);
			};
			window.RenderFrame += _ => view.Draw((float)stopWatch.Elapsed.TotalMilliseconds);

			window.RenderFrame += _ => window.SwapBuffers();
			window.RenderFrame += _ => ShowFPS((float)stopWatch.Elapsed.TotalMilliseconds);
			window.Resize += (window) => view.Resize(window.Width, window.Height);
			window.Minimized += _ => view.SetActiveState(false);
			window.FocusedChanged += _ => view.SetActiveState(true);

			window.TextInput += e =>
			{
				if (view.RenderGUI) _controller.PressChar((char)e.Unicode);
			};
            window.MouseWheel += e =>
            {
				if(view.RenderGUI)_controller.MouseScroll(e.Offset);
			};


			window.Run();
		}
	}
}