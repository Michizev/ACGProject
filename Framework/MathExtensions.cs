using OpenTK.Mathematics;
using System;

namespace Framework
{
	/// <summary>
	/// Contains static/extension methods for System.Math and System.Numerics for more mathematical operations, 
	/// often overloaded for Vector types.
	/// Operations include Clamp, Round, Lerp, Floor, Mod
	/// </summary>
	public static class MathExtensions
	{
		/// <summary>
		/// Returns for each component the smallest integer bigger than or equal to the specified floating-point number.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static Vector2 Ceiling(in Vector2 value) => new Vector2(MathF.Ceiling(value.X), MathF.Ceiling(value.Y));

		/// <summary>
		/// Clamp the input value x in between min and max. 
		/// If x smaller min return min; 
		/// if x bigger max return max; 
		/// else return x unchanged
		/// </summary>
		/// <param name="x">input value that will be clamped</param>
		/// <param name="min">lower limit</param>
		/// <param name="max">upper limit</param>
		/// <returns>clamped version of x</returns>
		public static int Clamp(this int x, int min, int max) => Math.Min(max, Math.Max(min, x));

		/// <summary>
		/// Clamp the input value x in between min and max. 
		/// If x smaller min return min; 
		/// if x bigger max return max; 
		/// else return x unchanged
		/// </summary>
		/// <param name="x">input value that will be clamped</param>
		/// <param name="min">lower limit</param>
		/// <param name="max">upper limit</param>
		/// <returns>clamped version of x</returns>
		public static float Clamp(this float x, float min, float max) => MathF.Min(max, MathF.Max(min, x));

		/// <summary>
		/// Clamp the input value x in between min and max. 
		/// If x smaller min return min; 
		/// if x bigger max return max; 
		/// else return x unchanged
		/// </summary>
		/// <param name="x">input value that will be clamped</param>
		/// <param name="min">lower limit</param>
		/// <param name="max">upper limit</param>
		/// <returns>clamped version of x</returns>
		public static double Clamp(this double x, double min, double max) => Math.Min(max, Math.Max(min, x));

		/// <summary>
		/// Calculates the determinant of the two vectors.
		/// </summary>
		/// <param name="a">Vector a.</param>
		/// <param name="b">Vector b.</param>
		/// <returns>The determinant</returns>
		public static float Determinant(in Vector2 a, in Vector2 b) => a.X * b.Y - a.Y * b.X;

		/// <summary>
		/// Returns the number of mipmap levels required for mipmapped filtering of an image.
		/// </summary>
		/// <param name="width">The image width in pixels.</param>
		/// <param name="height">The image height in pixels.</param>
		/// <returns>Number of mipmap levels</returns>
		public static int MipMapLevels(int width, int height) => (int)MathF.Log(Math.Max(width, height), 2f) + 1;

		/// <summary>
		/// Clock-wise normal to input vector.
		/// </summary>
		/// <param name="v">The input vector.</param>
		/// <returns>A vector normal to the input vector</returns>
		public static Vector2 CwNormalTo(this in Vector2 v) => new Vector2(v.Y, -v.X);

		/// <summary>
		/// Counter-clock-wise normal to input vector.
		/// </summary>
		/// <param name="v">The input vector.</param>
		/// <returns>A vector normal to the input vector</returns>
		public static Vector2 CcwNormalTo(this in Vector2 v) => new Vector2(-v.Y, v.X);

		/// <summary>
		/// Convert input uint from range [0,255] into float in range [0,1]
		/// </summary>
		/// <param name="v">input in range [0,255]</param>
		/// <returns>range [0,1]</returns>
		public static float Normalize(uint v) => v / 255f;

		/// <summary>
		/// Finds a range of existing indexes inside a sorted array that encompass a given value
		/// </summary>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="sorted">a sorted array of values</param>
		/// <param name="value">a value</param>
		/// <returns></returns>
		public static (int lower, int upper) FindExistingRange<TValue>(this TValue[] sorted, TValue value)
		{
			var ipos = Array.BinarySearch(sorted, value);
			if (ipos >= 0)
			{
				// exact target found at position "ipos"
				return (ipos, ipos);
			}
			else
			{
				// Exact key not found: BinarySearch returns negative when the 
				// exact target is not found, which is the bitwise complement 
				// of the next index in the list larger than the target.
				ipos = ~ipos;
				if (0 == ipos)
				{
					return (0, 0);
				}
				if (ipos < sorted.Length)
				{
					return (ipos - 1, ipos);
				}
				else
				{
					return (sorted.Length - 1, sorted.Length - 1);
				}
			}
		}
		/*
			var times = Enumerable.Range(-4, 10).Select(v => (float)v).ToArray();
			for (var v = -4f; v < 15; v += 0.25f)
			{
				var (lower, upper) = times.FindExistingRange(v);
				Console.WriteLine($"{v}: lower={times[lower]} higher={times[upper]}");
			}
		 */

		/// <summary>
		/// Transform the input value into the range [0..1]
		/// </summary>
		/// <param name="inputValue">the input value</param>
		/// <param name="inputMin">the lower input range bound</param>
		/// <param name="inputMax">the upper input range bound</param>
		/// <returns></returns>
		public static float Normalize(this float inputValue, float inputMin, float inputMax)
		{
			var inputRange = inputMax - inputMin;
			return float.Epsilon >= inputRange ? 0f : (inputValue - inputMin) / inputRange;
		}

		/// <summary>
		/// Normalizes each input uint from range [0,255] into float in range [0,1]
		/// </summary>
		/// <param name="x">input in range [0,255]</param>
		/// <param name="y">input in range [0,255]</param>
		/// <param name="z">input in range [0,255]</param>
		/// <param name="w">input in range [0,255]</param>
		/// <returns>vector with each component in range [0,1]</returns>
		public static Vector4 Normalize(uint x, uint y, uint z, uint w) => new Vector4(x, y, z, w) / 255f;

		/// <summary>
		/// Linear interpolation of two known values a and b according to weight
		/// </summary>
		/// <param name="a">First value</param>
		/// <param name="b">Second value</param>
		/// <param name="weight">Interpolation weight</param>
		/// <returns>Linearly interpolated value</returns>
		public static float Lerp(float a, float b, float weight) => a * (1 - weight) + b * weight;

		/// <summary>
		/// Linear interpolation of two known values a and b according to weight
		/// </summary>
		/// <param name="a">First value</param>
		/// <param name="b">Second value</param>
		/// <param name="weight">Interpolation weight</param>
		/// <returns>Linearly interpolated value</returns>
		public static double Lerp(double a, double b, double weight) => a * (1 - weight) + b * weight;

		/// <summary>
		/// Returns the integer part of the specified floating-point number. 
		/// Works not for constructs like <code>1f - float.epsilon</code> because this is outside of floating point precision
		/// </summary>
		/// <param name="x">Input floating-point number</param>
		/// <returns>The integer part.</returns>
		public static int FastTruncate(this float x) => (int)x;

		/// <summary>
		/// Returns for each component the integer part of the specified floating-point number. 
		/// Works not for constructs like <code>1f - float.epsilon</code> because this is outside of floating point precision
		/// </summary>
		/// <param name="value">Input floating-point vector</param>
		/// <returns>The integer parts.</returns>
		public static Vector2 Truncate(in Vector2 value) => new Vector2(value.X.FastTruncate(), value.Y.FastTruncate());

		/// <summary>
		/// For each component returns the largest integer less than or equal to the specified floating-point number.
		/// </summary>
		/// <param name="v">Input vector</param>
		/// <returns>For each component returns the largest integer less than or equal to the specified floating-point number.</returns>
		public static Vector3 Floor(this in Vector3 v) => new Vector3(MathF.Floor(v.X), MathF.Floor(v.Y), MathF.Floor(v.Z));

		/// <summary>
		/// Returns the value of x modulo y. This is computed as x - y * floor(x/y). 
		/// </summary>
		/// <param name="x">Dividend</param>
		/// <param name="y">Divisor</param>
		/// <returns>Returns the value of x modulo y.</returns>
		public static Vector3 Mod(this in Vector3 x, float y)
		{
			var div = x / y;
			return x - y * Floor(div);
		}

		/// <summary>
		/// packs normalized floating-point values into an unsigned integer.  
		/// </summary>
		/// <param name="v">Input normalized floating-point vector. Will be clamped</param>
		/// <returns>The first component of the vector will be written to the least significant bits of the output; 
		/// the last component will be written to the most significant bits.</returns>
		public static uint PackUnorm4x8(this in Vector4 v)
		{
			var r = Round(Vector4.Clamp(v, Vector4.Zero, Vector4.One) * 255.0f);
			var x = (uint)r.X;
			var y = (uint)r.Y;
			var z = (uint)r.Z;
			var w = (uint)r.W;
			return (w << 24) + (z << 16) + (y << 8) + x;
		}

		/// <summary>
		/// Unpacks normalized floating-point values from an unsigned integer.
		/// </summary>
		/// <param name="i">Specifies an unsigned integer containing packed floating-point values.</param>
		/// <returns>The first component of the returned vector will be extracted from the least significant bits of the input; 
		/// the last component will be extracted from the most significant bits. </returns>
		public static Vector4 UnpackUnorm4x8(uint i)
		{
			var x = (i & 0x000000ff);
			var y = (i & 0x0000ff00) >> 8;
			var z = (i & 0x00ff0000) >> 16;
			var w = (i & 0xff000000) >> 24;
			var v = new Vector4(x, y, z, w);
			return v / 255.0f;
		}

		/// <summary>
		/// Rounds each component of a floating-point vector (using MathHelper.Round) to the nearest integral value.
		/// </summary>
		/// <param name="v">A floating-point vector to be rounded component-wise.</param>
		/// <returns>Component-wise rounded vector</returns>
		public static Vector3 Round(this in Vector3 v) => new Vector3(MathF.Round(v.X), MathF.Round(v.Y), MathF.Round(v.Z));

		/// <summary>
		/// Rounds each component of a floating-point vector (using MathHelper.Round) to the nearest integral value.
		/// </summary>
		/// <param name="v">A floating-point vector to be rounded component-wise.</param>
		/// <returns>Component-wise rounded vector</returns>
		public static Vector4 Round(this in Vector4 v) => new Vector4(MathF.Round(v.X), MathF.Round(v.Y), MathF.Round(v.Z), MathF.Round(v.W));

		/// <summary>
		/// Converts given Cartesian coordinates into a polar angle.
		/// Returns an angle [-PI, PI].
		/// </summary>
		/// <param name="cartesian">Cartesian input coordinates</param>
		/// <returns>An angle [-PI, PI].</returns>
		public static float PolarAngle(this in Vector2 cartesian) => MathF.Atan2(cartesian.Y, cartesian.X);

		/// <summary>
		/// Converts a Vector to a array of float
		/// </summary>
		/// <param name="q">The input vector.</param>
		/// <returns></returns>
		public static float[] ToArray(this in Quaternion q) => new float[] { q.X, q.Y, q.Z, q.W };

		/// <summary>
		/// Converts a Vector to a array of float
		/// </summary>
		/// <param name="vector">The input vector.</param>
		/// <returns></returns>
		public static float[] ToArray(this in Vector2 vector) => new float[] { vector.X, vector.Y };

		/// <summary>
		/// Converts a Vector to a array of float
		/// </summary>
		/// <param name="vector">The input vector.</param>
		/// <returns></returns>
		public static float[] ToArray(this in Vector3 vector) => new float[] { vector.X, vector.Y, vector.Z };

		/// <summary>
		/// Converts a Vector to a array of float
		/// </summary>
		/// <param name="vector">The input vector.</param>
		/// <returns></returns>
		public static float[] ToArray(this in Vector4 vector) => new float[] { vector.X, vector.Y, vector.Z, vector.W };

		/// <summary>
		/// Converts the given polar coordinates to Cartesian.
		/// </summary>
		/// <param name="polar">The polar coordinates. A vector with first component angle [-PI, PI] and second component radius.</param>
		/// <returns>A Cartesian coordinate vector.</returns>
		public static Vector2 ToCartesian(this in Vector2 polar)
		{
			float x = polar.Y * MathF.Cos(polar.X);
			float y = polar.Y * MathF.Sin(polar.X);
			return new Vector2(x, y);
		}

		public static Color4 ToColor4(this float[] vector) => new Color4(vector[0], vector[1], vector[2], vector[3]);

		/// <summary>
		/// Converts given Cartesian coordinates into polar coordinates.
		/// Returns a vector with first component angle [-PI, PI] and second component radius.
		/// </summary>
		/// <param name="cartesian">Cartesian input coordinates</param>
		/// <returns>A vector with first component angle [-PI, PI] and second component radius.</returns>
		public static Vector2 ToPolar(this in Vector2 cartesian)
		{
			float angle = cartesian.PolarAngle();
			float radius = cartesian.Length;
			return new Vector2(angle, radius);
		}
	}
}
