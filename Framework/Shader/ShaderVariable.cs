namespace Framework
{
	public class ShaderVariable
	{
		public ShaderVariable(ShaderProgram program, int location)
		{
			Program = program;
			Location = location;
		}

		public ShaderProgram Program { get; }
		public int Location { get; }
	}
}