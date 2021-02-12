using System;

namespace Framework
{
	public interface IObjectGL : IDisposable
	{
		int Handle { get; }
	}
}