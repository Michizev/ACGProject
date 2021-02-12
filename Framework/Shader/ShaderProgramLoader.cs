using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Framework
{
	public static class ShaderProgramLoader
	{
		public static ShaderProgram FromResources(this ShaderProgram shaderProgram, IEnumerable<(ShaderType, string)> resources)
		{
			var shaders = new List<(ShaderType, string)>();
			foreach ((ShaderType type, string resourceName) in resources)
			{
				var sourceCode = Resource.LoadString(resourceName);
				Debug.WriteLine($"Loading shader '{type}' from resource {resourceName}");
				shaders.Add((type, sourceCode));
			}
			shaderProgram.CompileLink(shaders);
			return shaderProgram;
		}

		public static ShaderProgram FromResources(IEnumerable<(ShaderType, string)> resources) => new ShaderProgram().FromResources(resources);

		public static ShaderProgram FromResourcePrefix(this ShaderProgram shaderProgram, string prefix)
		{
			var shaders = new List<(ShaderType, string)>();
			foreach (var name in Resource.Matches(prefix))
			{
				var extension = Path.GetExtension(name).ToLowerInvariant()[1..];
				if (mappingExtensionToShaderType.TryGetValue(extension, out var type))
				{
					shaders.Add((type, name));
				}
				else throw new ArgumentException($"Invalid extension '{extension}' for a shader in resource '{name}'");
			}
			if(0 == shaders.Count) throw new ArgumentException($"Prefix '{prefix}' did not match any shader in resource");
			return shaderProgram.FromResources(shaders);
		}

		public static ShaderProgram FromResourcePrefix(string prefix) => new ShaderProgram().FromResourcePrefix(prefix);

		private static readonly IReadOnlyDictionary<string, ShaderType> mappingExtensionToShaderType = new Dictionary<string, ShaderType>()
		{
			["frag"] = ShaderType.FragmentShader,
			["vert"] = ShaderType.VertexShader,
			["geom"] = ShaderType.GeometryShader,
			["tesc"] = ShaderType.TessControlShader,
			["tese"] = ShaderType.TessEvaluationShader,
			["comp"] = ShaderType.ComputeShader,
		};
	}
}
