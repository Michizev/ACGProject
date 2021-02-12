using OpenTK.Mathematics;
using System.Collections.Generic;

namespace Zev
{
    class GLTFState
	{
		List<Matrix4> prevTransform = new List<Matrix4>();
		List<Matrix4> currTransform = new List<Matrix4>();

		public void Add(Matrix4 transformation)
		{
			currTransform.Add(transformation);
		}
		public void EndIteration()
		{
			var tmp = currTransform;
			currTransform = prevTransform;
			prevTransform = tmp;
			currTransform.Clear();
		}
	}
}