using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Framework
{
	public class Mesh
	{
		public List<uint> ID = new List<uint>();
		public List<Vector3> Position = new List<Vector3>();
		public List<Vector3> Normal = new List<Vector3>();
		public List<Vector2> TextureCoordinate = new List<Vector2>();
		public string DiffuseTexture = string.Empty;
		public string SpecularTexture = string.Empty;
	}
}
