using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Framework
{
	public class DebugGameWindow : GameWindow
	{
#if DEBUG
		private readonly DebugOutputGL _debugOutput;
		public DebugGameWindow() : base(GameWindowSettings.Default, new NativeWindowSettings { Profile = ContextProfile.Core, Flags = ContextFlags.Debug })
#else
		public DebugGameWindow() : base(GameWindowSettings.Default, new NativeWindowSettings { Profile = ContextProfile.Core })
#endif
		{
			// set window to halve monitor size
			if (Monitors.TryGetMonitorInfo(0, out var info))
			{
				Size = new Vector2i(info.HorizontalResolution, info.VerticalResolution) / 2;
			}
#if DEBUG
			_debugOutput = new DebugOutputGL();
#endif
		}

#if DEBUG
		~DebugGameWindow() => _debugOutput.Dispose();
#endif
	}
}
