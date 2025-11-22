using System;

namespace MathLib
{
    /// <summary>
    /// my implementation of https://www.sciencedirect.com/science/article/abs/pii/S0097849312000568
    /// </summary>
    public static class SphericalPacker16
    {
        const ushort SIGN = 0xE000; //3 bit of sign
        const ushort MASK = 0x1FFF; //13 bit of index (8191)
        static double sqrt2 = Math.Sqrt(2);

        const float PI = (float)Math.PI;
        const float PI_2 = PI / 2;
        const int N = 126; //maximum n for a 13 bit points
        public const ushort MAX = 8127; //N 126 generates 8128 points, can be obtains using formula (N+3)N/2
        const float d_phi = PI_2 / N; //fixed delta for phi

#if DEBUG
		static byte[] i_tab = new byte[MAX + 1]; //for debug, 8Kb array 

        static SphericalPacker16()
        {
            //mantain the precomputed table for debugging
            int n = 0;
            for (byte i = 0; i <= N; i++)
                for (int j = 0; j <= i; j++)
                    i_tab[n++] = i;
        }
#endif

        /// <summary>
        /// Separate sign and index
        /// </summary>
        static void masksign(ref ushort n, out byte sign)
        {
            sign = (byte)(n >> 13);
            n &= MASK;
        }
		
        /// <summary>
        /// convert to cartesian vector
        /// </summary>
        static Vector3f cartesian(int i, int j, byte sign)
        {
            float phi = i * d_phi;
            float theta = i > 0 ? j * PI_2 / i : 0;

			var normal = new Vector3f(
				Math.Cos(theta) * Math.Sin(phi),
				Math.Cos(phi),
				Math.Sin(theta) * Math.Sin(phi));

            if ((sign & 4) != 0) normal.x *= -1;
            if ((sign & 2) != 0) normal.y *= -1;
            if ((sign & 1) != 0) normal.z *= -1;

            return normal;
        }
		
		/// <summary>
        /// convert to cartesian vector using trigonometrix approximations
        /// </summary>
        static Vector3f cartesian_approx(int i, int j, byte sign)
        {
            float phi = i * d_phi;
            float theta = i > 0 ? j * PI_2 / i : 0;

            var normal = new Vector3f(
				Cos(theta) * Sin(phi),
				Cos(phi),
				Sin(theta) * Sin(phi));

            if ((sign & 4) != 0) normal.x *= -1;
            if ((sign & 2) != 0) normal.y *= -1;
            if ((sign & 1) != 0) normal.z *= -1;

            return normal;
        }	
		static (float theta, float phi) spherical(Vector3f cartesian)
		{
			cartesian.Normalize(); //how x, y, z are divided by r
            float theta = (float)Math.Atan2(cartesian.z, cartesian.x); //work for vector -z
            //float theta = (float)Math.Atan(cartesian.z /cartesian.x);
            float phi = (float)Math.Acos(cartesian.y);
            return (theta, phi);
		}
		static (float theta, float phi) spherical_approx(Vector3f cartesian)
		{
			cartesian.Normalize();
            return ((float)Math.Atan2(cartesian.z, cartesian.x), AcosSL(cartesian.y));
		}
        /// <summary>
        /// Summation of sequence for j=0, or Gauss formula, or triangular number, or...
        /// </summary>
        static int sum(int i) => (i + 1) * i / 2;

        /// <summary>
        /// inverse of summation.
        /// </summary>
        static void inverse(int n, out int i, out int j)
        {
            //check for n[0,8127]
            i = (int)(Math.Sqrt(1 + 8 * n) - 1) / 2;
            j = n - sum(i);
        }
        /// <summary>
        /// approximation of <see cref="inverse"/> (with sqrt)
        /// </summary>
        static void inverse_aprox(int n, out int i, out int j)
        {
            //check for n[0,8127]
            i = (int)(Math.Sqrt(n) * sqrt2);
            j = n - sum(i);
            if (j < 0) { j += i; i--; }
        }
        /// <summary>
        /// approximation of <see cref="inverse"/> (without sqrt)
        /// </summary>
        static void inverse_aprox2(int n, out int i, out int j)
        {
            //check for n[0,8127]
            i = (int)(Math.Exp(0.5 * Math.Log(n)) * sqrt2);
            j = n - sum(i);
            if (j < 0) { j += i; i--; }
        }
		/// <summary>
		/// Bhāskara I's sin
		/// </summary>
		static float Sin(float r)
		{
		    var n = (PI - r)*r * 4;
            return (float)(n * 4 / (5 * PI * PI - n));
		}
		/// <summary>
		/// Bhāskara I's cos
		/// </summary>
		static float Cos(float r)
		{
		    return (PI_2 - 4 * r * r) / (PI_2 + r * r);
		}
		/// <summary>
		/// Trey Reynolds' ArcCos
		/// </summary>
		static float AcosTR(float x)
		{
		    return (float)(8 / 3.0 * Math.Sqrt(2 - Math.Sqrt(2 + 2 * x)) - 1 / 3.0 * Math.Sqrt(2 - 2 * x));
		}
		/// <summary>
		/// Sebastien Lagarde's ArcCos
		/// </summary>
		static float AcosSL(float x)
		{
		    var z = Math.Abs(x);
		    z = (float)((-0.168577f * z + 1.56723f) * Math.Sqrt(1 - z));
		    return x < 0 ? z + PI : -z;
		}

		/// <summary>
        /// Encode slow
        /// </summary>
        public static ushort Encode(Vector3f normal)
        {
            ushort value = 0;
            if (normal.x < 0) { value |= 4; normal.x *= -1; }
            if (normal.y < 0) { value |= 2; normal.y *= -1; }
            if (normal.z < 0) { value |= 1; normal.z *= -1; }

            (float theta, float phi) = spherical(normal);

            int i = (int)Math.Round(phi / d_phi);
            int j = (int)Math.Round(theta * i / PI_2);
            int n = sum(i) + j;

            return (ushort)((value << 13) | n);
        }
		
		/// <summary>
        /// Encode fast but less precise for some points
        /// </summary>
		public static ushort EncodeApprox(Vector3f normal)
        {
            ushort value = 0;
            if (normal.x < 0) { value |= 4; normal.x *= -1; }
            if (normal.y < 0) { value |= 2; normal.y *= -1; }
            if (normal.z < 0) { value |= 1; normal.z *= -1; }

            (float theta, float phi) = spherical_approx(normal);
			
            int i = (int)Math.Round(phi / d_phi);
            int j = (int)Math.Round(theta * i / PI_2);
            int n = sum(i) + j;

            return (ushort)((value << 13) | n);
        }

        /// <summary>
        /// Decode fast with trigonometrics approximations
        /// </summary>
        public static Vector3f DecodeApprox(ushort value)
        {
            masksign(ref value, out byte sign);
            inverse(value, out int i, out int j);
            return cartesian_approx(i, j, sign);
        }
        /// <summary>
        /// Decode with trigonometrics
        /// </summary>
        public static Vector3f Decode(ushort value)
        {
            masksign(ref value, out byte sign);
            inverse(value, out int i, out int j);
            return cartesian(i, j, sign);
        }
    }
}

