using OpenTK.Mathematics;
using System;

namespace Framework
{
	/// <summary>
	/// Implement an orbiting camera
	/// </summary>
	public class OrbitingCamera
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Orbit"/> class.
		/// </summary>
		/// <param name="distance">The distance to the target.</param>
		/// <param name="azimuth">The azimuth or heading.</param>
		/// <param name="elevation">The elevation or tilt.</param>
		public OrbitingCamera(float distance, float azimuth = 0f, float elevation = 0f)
		{
			Matrix4 CalcViewMatrix()
			{
				var mtxDistance = Matrix4.CreateTranslation(0, 0, -Distance);
				var mtxElevation = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(Elevation));
				var mtxAzimut = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(Azimuth));
				var mtxTarget = Matrix4.CreateTranslation(-Target);
				return mtxTarget * mtxAzimut * mtxElevation * mtxDistance;
			}

			_cachedMatrixView = new DirtyFlag<Matrix4>(CalcViewMatrix);
			_cachedMatrixViewInv = new DirtyFlag<Matrix4>(() => View.Inverted());
			_cachedMatrixViewProjection = new DirtyFlag<Matrix4>(() => View * Projection);
			Distance = distance;
			Azimuth = azimuth;
			Elevation = elevation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Orbit"/> class.
		/// </summary>
		/// <param name="distance">The distance to the target.</param>
		/// <param name="azimuth">The azimuth or heading.</param>
		/// <param name="elevation">The elevation or tilt.</param>
		/// <param name="projection">The projection matrix.</param>
		public OrbitingCamera(float distance, float azimuth, float elevation, Matrix4 projection) : this(distance, azimuth, elevation)
		{
			Projection = projection;
		}
		
		/// <summary>
		/// Gets or sets the azimuth or heading.
		/// </summary>
		/// <value>
		/// The azimuth.
		/// </value>
		public float Azimuth
		{
			get => _azimuth;
			set
			{
				_azimuth = value;
				InvalidateViewMatrices();
			}
		}

		/// <summary>
		/// Gets or sets the distance from the target.
		/// </summary>
		/// <value>
		/// The distance.
		/// </value>
		public float Distance
		{
			get => _distance;
			set
			{
				_distance = MathF.Max(0.001f, value);
				InvalidateViewMatrices();
			}
		}

		/// <summary>
		/// Gets or sets the elevation or tilt.
		/// </summary>
		/// <value>
		/// The elevation.
		/// </value>
		public float Elevation
		{
			get => _elevation;
			set
			{
				_elevation = value;
				InvalidateViewMatrices();
			}
		}

		public Matrix4 Projection
		{
			get => _projection;
			set
			{
				_projection = value;
				_cachedMatrixViewProjection.Invalidate();
			}
		}

		public Vector3 Target
		{
			get => _target;
			set
			{
				_target = value;
				InvalidateViewMatrices();
			}
		}

		public Matrix4 View => _cachedMatrixView.Value;
		public Matrix4 ViewInv => _cachedMatrixViewInv.Value;
		public Matrix4 ViewProjection => _cachedMatrixViewProjection.Value;

		/// <summary>
		/// Calculates the camera position.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="ArithmeticException">Could not invert matrix</exception>
		public Vector3 CalcPosition() => ViewInv.ExtractTranslation();

		private float _azimuth;
		private float _distance;
		private float _elevation;
		private Matrix4 _projection = Matrix4.Identity;
		private Vector3 _target = Vector3.Zero;
		private readonly DirtyFlag<Matrix4> _cachedMatrixView;
		private readonly DirtyFlag<Matrix4> _cachedMatrixViewInv;
		private readonly DirtyFlag<Matrix4> _cachedMatrixViewProjection;

		private void InvalidateViewMatrices()
		{
			_cachedMatrixView.Invalidate();
			_cachedMatrixViewInv.Invalidate();
			_cachedMatrixViewProjection.Invalidate();
		}
	}
}
