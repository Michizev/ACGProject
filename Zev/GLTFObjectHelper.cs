using glTFLoader.Schema;
using System;
using System.Collections.Generic;

namespace Zev
{
    public class GLTFObjectHelper
	{

		public static void GetNames(Gltf gltf)
		{
			//var localNodeTransforms = gltf.ExtractLocalTransforms();
			List<string> names = new List<string>();
			foreach (var scene in gltf.Scenes)
			{
				var currentChildren = new List<int>();
				var next = new List<int>();

				currentChildren.AddRange(scene.Nodes);

				int iteration = 0;
				while (currentChildren.Count != 0)
				{
					//if (iteration > 2) break;
					//iteration++;
					foreach (var n in currentChildren)
					{
						var node = gltf.Nodes[n];
						//if (node.Mesh != null) continue;
						names.Add($"{n} : {node.Name}");
						if (node.Children != null) next.AddRange(node.Children);
					}

					var tmp = currentChildren;
					currentChildren = next;
					next = tmp;

					next.Clear();
				}
				/*
				foreach(var n in names)
                {
					Console.WriteLine(n);
                }
				*/
				Console.WriteLine("DONE");
			}
		}

	}
}