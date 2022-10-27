using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Zev
{
    class ZMath
    {
        public static double ToRadians(double angle)
        {
            return (Math.PI / 180f) * angle;
        }
        public static float ToRadians(float angle)
        {
            return (MathF.PI / 180f) * angle;
        }
    }
}
