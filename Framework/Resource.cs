using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Framework
{
	public static class Resource
	{
		public static string LoadString(string name)
		{
			using var stream = LoadStream(name);
			using var streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
		}

		public static Stream LoadStream(string name)
		{
			
			var assembly = Assembly.GetEntryAssembly();
			var stream = assembly.GetManifestResourceStream(name);
			if (stream is null)
			{
				var names = string.Join('\n', assembly.GetManifestResourceNames());
				throw new ArgumentException($"Could not find resource '{name}' in resources\n'{names}'");
			}
			return stream;
		}

		public static IEnumerable<string> Matches(string text)
		{
			var assembly = Assembly.GetEntryAssembly();
			var resourceNames = assembly.GetManifestResourceNames();
			return resourceNames.Where(name => name.Contains(text));
		}
	}
}
