namespace Framework
{
	public static class MeshToGL
	{
		public static MeshGL Create(Mesh mesh, VertexAttrib position, VertexAttrib normal = null, VertexAttrib texCoord = null)
		{
			var meshGL = new MeshGL();
			meshGL.AddIndices(mesh.ID.ToArray());
			meshGL.AddAttribute(position, mesh.Position.ToArray());
			if (!(normal is null) && mesh.Normal.Count > 0 && -1 != normal.Location)
			{
				meshGL.AddAttribute(normal, mesh.Normal.ToArray());
			}
			if (!(texCoord is null) && mesh.TextureCoordinate.Count > 0 && -1 != texCoord.Location)
			{
				meshGL.AddAttribute(texCoord, mesh.TextureCoordinate.ToArray());
			}

			return meshGL;
		}

		public static MeshGL Create(Mesh mesh, IShaderProgramVariables shaderProgramVars, string positionName = "position", string normalName = "normal", string texCoordName = "texCoord")
		{
			var meshGL = new MeshGL();
			meshGL.AddIndices(mesh.ID.ToArray());
			var attribPosition = shaderProgramVars.Get<VertexAttrib>(positionName);
			meshGL.AddAttribute(attribPosition, mesh.Position.ToArray());

			var attribNormal = shaderProgramVars.Get<VertexAttrib>(normalName);
			if (attribNormal != null)
			{
				meshGL.AddAttribute(attribNormal, mesh.Normal.ToArray());
			}
			var attribTexCoord = shaderProgramVars.Get<VertexAttrib>(texCoordName);
			if (attribTexCoord != null)
			{
				meshGL.AddAttribute(attribTexCoord, mesh.TextureCoordinate.ToArray());
			}
			return meshGL;
		}
	}
}
