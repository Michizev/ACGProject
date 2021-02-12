using System;
using System.Reflection;

namespace Framework
{
	public class Uniform<DataType> : ShaderVariable
	{
		public Uniform(ShaderProgram shaderProgram, PropertyInfo property, Action<int, int, DataType> setter) : base(shaderProgram, GetLocation(shaderProgram, property))
		{
			_setter = setter ?? throw new ArgumentNullException(nameof(setter));
		}

		public Uniform(ShaderProgram program, int location, Action<int, int, DataType> setter) : base(program, location)
		{
			_setter = setter ?? throw new ArgumentNullException(nameof(setter));
		}

		public DataType Value { get => _value; set => Set(value); }

		private void Set(DataType value)
		{
			_value = value;
			_setter(Program.Handle, Location, value);
		}

		private DataType _value;
		private readonly Action<int, int, DataType> _setter;

		private static int GetLocation(ShaderProgram shaderProgram, PropertyInfo property)
		{
			var name = shaderProgram.ConvertName(property.Name);
			return shaderProgram.CheckedUniformLocation(name);
		}
	}

}