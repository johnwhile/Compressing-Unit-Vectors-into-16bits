using System;
using System.Text;

namespace MathLib
{
    public struct Vector3f
    {
        public float x, y, z;

        public Vector3f(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
		
        public Vector3f(double x, double y, double z):
			this((float)x, (float)y, (float)z) {}
			
        public float Normalize()
        {
            var length = (float)Math.Sqrt(x * x + y * y + z * z);
            var l = length > float.Epsilon ? 1f / length : 0;
			x *= l;
			y *= l;
			z *= l;
            return length;
        }
        public override string ToString()
        {
            return $"{x} {y} {z}";
        }
    }
}

