using System;
using System.Runtime.InteropServices;

namespace Framework
{
	public static class ArrayConverter
	{
		public static T[] FromByteArray<T>(this byte[] source, int byteOffset, int destinationCount) where T : struct
		{
			T[] destination = new T[destinationCount];
			GCHandle handle = GCHandle.Alloc(destination, GCHandleType.Pinned);
			try
			{
				IntPtr pointer = handle.AddrOfPinnedObject();
				Marshal.Copy(source, byteOffset, pointer, destinationCount * Marshal.SizeOf<T>()); // need to use Marshalling because of data type conversion
				return destination;
			}
			finally
			{
				if (handle.IsAllocated)
					handle.Free();
			}

		}

		public static float[] ToFloatArray<SourceType>(this SourceType[] source, int byteOffset = 0) where SourceType : struct
		{
			var destination = new float[source.Length * Marshal.SizeOf<SourceType>() / 4];
			GCHandle handle = GCHandle.Alloc(source, GCHandleType.Pinned);
			try
			{
				IntPtr pointer = handle.AddrOfPinnedObject();
				Marshal.Copy(pointer + byteOffset, destination, 0, destination.Length); // need to use Marshalling because of data type conversion
				return destination;
			}
			finally
			{
				if (handle.IsAllocated)
					handle.Free();
			}
		}
	}
}
