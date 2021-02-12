namespace Framework
{
	public interface IShaderProgramVariables
	{
		VariableType Get<VariableType>(string name) where VariableType : ShaderVariable;
		VariableType GetWithDefault<VariableType>(string name) where VariableType : ShaderVariable;
	}
}