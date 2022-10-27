using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Zev
{
    class FpsCamera : ICamera
    {
		private float _elevation;
		private float _azimuth;

		public float Speed = 10;
		public Vector3 Position { get; internal set; }

		public FpsCamera(Vector3 position)
        {
			Position = position;
			UpdatePosition();
			UpdateMatrix();
        }

		public float Azimuth
		{
			get => _azimuth;
			set
			{
				_azimuth = value;
				UpdateMatrix();
			}
		}
		public float Elevation
		{
			get => _elevation;
			set
			{
				_elevation = value;
				UpdateMatrix();
			}
		}

		private Vector3 _movement;
		private float simpleNum = 0;
		public Vector3 Movement
		{
			set
			{
				if (value == Vector3.Zero) return;
				_movement = value;
				
				UpdatePosition();
			}

			get
			{
				return _movement;
			}

		}

		public Matrix4 RotTranspose => Matrix4.Transpose(rot);


		public Vector3 Right => RotTranspose.Row2.Xyz; //+X
		public Vector3 Forward => RotTranspose.Row0.Xyz; //-Z
		private void UpdatePosition()
		{
			Position += new Vector3(Forward * Movement.X * Speed);
			Position += new Vector3(Right * Movement.Y * Speed);
			//Position +=Movement* Speed;
			
			Translation = Matrix4.CreateTranslation(-Position);
		}

		Matrix4 rot;
        private Matrix4 _projection;

        private void UpdateMatrix()
		{
			float degAzi = (float)(Math.PI / 180) * _azimuth;
			float degEle = (float)(Math.PI / 180) * _elevation;
			rot = Matrix4.CreateRotationY(degAzi) * Matrix4.CreateRotationX(degEle);
		}

		public Matrix4 Translation
        {
			get; private set;
        }

		public Matrix4 View
		{
			get
			{
				return  Translation * rot;

				//return Matrix4.LookAt(Position, Vector3.Zero, Forward);
			}
		}

		public Matrix4 Projection
		{
			get => _projection;
			set
			{
				_projection = value;
				//_cachedMatrixViewProjection.Invalidate();
			}
		}

        public Matrix4 ViewInv => View.Inverted();

        public Matrix4 ViewProjection => View*Projection;
    }
}
