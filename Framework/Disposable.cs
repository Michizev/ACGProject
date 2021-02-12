using System;
using System.Linq;
using System.Reflection;

namespace Framework
{
	public abstract class Disposable : IDisposable
	{
		/// <summary>
		/// Will be called from the default Dispose method.
		/// Implementers should dispose all their resources her.
		/// </summary>
		protected abstract void DisposeResources();

		/// <summary>
		/// Dispose status of the instance.
		/// </summary>
		public bool Disposed => disposed;

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Calls <see cref="IDisposable.Dispose()"/> on all fields of type <see cref="IDisposable"/> found on the given object.
		/// </summary>
		/// <param name="obj"></param>
		public static void DisposeAllFields(object obj)
		{
			// get all fields, including backing fields for properties
			var allFields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (var field in allFields.Where(field => typeof(IDisposable).IsAssignableFrom(field.FieldType)))
			{
				((IDisposable)field.GetValue(obj))?.Dispose();
			}
		}


		// NOTE: Leave out the finalizer altogether if this class doesn't
		// own unmanaged resources, but leave the other methods
		// exactly as they are.
		~Disposable()
		{
			// Finalizer calls Dispose(false)
			Dispose(false);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing)
			{
				DisposeResources();
				disposed = true;
			}
		}

		private bool disposed = false;
	}
}
