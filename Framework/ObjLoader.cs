using ObjLoader.Loader.Loaders;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.IO;

namespace Framework
{
	public static class ObjLoader
	{
		private static Mesh ToMesh(this LoadResult loadResult)
		{
			var mesh = new Mesh();
			if(loadResult.Materials.Count > 0)
			{
				var material = loadResult.Materials[0];
				mesh.DiffuseTexture = material.DiffuseTextureMap;
				mesh.SpecularTexture = material.SpecularTextureMap;
			}
			var uniqueVertexIDs = new Dictionary<global::ObjLoader.Loader.Data.Elements.FaceVertex, uint>();

			foreach (var group in loadResult.Groups)
			{
				foreach (var face in group.Faces)
				{
					//only accept triangles
					if (3 != face.Count) continue;
					for (int i = 0; i < 3; ++i)
					{
						var vertex = face[i];
						if (uniqueVertexIDs.TryGetValue(vertex, out uint index))
						{
							mesh.ID.Add(index);
						}
						else
						{
							uint id = (uint)mesh.Position.Count;
							//add vertex data to mesh
							mesh.ID.Add(id);

							var position = loadResult.Vertices[vertex.VertexIndex - 1];
							mesh.Position.Add(new Vector3(position.X, position.Y, position.Z));
							if (0 != vertex.NormalIndex)
							{
								var normal = loadResult.Normals[vertex.NormalIndex - 1];
								mesh.Normal.Add(new Vector3(normal.X, normal.Y, normal.Z));
							}
							if (0 != vertex.TextureIndex)
							{
								var tex = loadResult.Textures[vertex.TextureIndex - 1];
								mesh.TextureCoordinate.Add(new Vector2(tex.X, tex.Y));
							}
							//new id
							uniqueVertexIDs[vertex] = id;
						}
					}
				}
			}
			return mesh;
		}

		public static Mesh Load(Stream stream, IMaterialStreamProvider materialProvider)
		{
			var objLoaderFactory = new ObjLoaderFactory();
			var objLoader = objLoaderFactory.Create(materialProvider);
			var loadResult = objLoader.Load(stream);
			return loadResult.ToMesh();
		}

		public static Mesh LoadFromResource(string resPrefix, string mesh)
		{
			var materialProvider = new MaterialProvider(resPrefix);
			using var meshStream = Resource.LoadStream(resPrefix + mesh);
			return Load(meshStream, materialProvider);
		}

		private class MaterialProvider : IMaterialStreamProvider
		{
			public MaterialProvider(string path)
			{
				Path = path;
			}

			public string Path { get; }

			public Stream Open(string materialFilePath) => Resource.LoadStream(Path + materialFilePath);
		};
	}
}
