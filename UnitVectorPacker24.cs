using System;
using System.Diagnostics;

namespace MathLib
{
    /// <summary>
    /// Generalization of <see cref="UnitSphericalPacker16"/>
    /// </summary>
    public static class UnitSphericalPacker24
    {
        const int MASK = 0x1F_FFFF; //21 bit of index 2.097.151
        const int N = 2046;//N 2046 generates 2.096.128 points
        const float PI = (float)Math.PI;
        const float PI_2 = PI / 2;
        const float d_phi = PI_2 / N;
        /// <summary>
        /// 2096127, maximum value of encoded value n
        /// </summary>
        public const int MAX = (N + 3) * N / 2;

#if DEBUG
        static ushort[] i_tab;
        public static void GeneratePrecomputedTab()
        {
            //mantain the precomputed table for debugging
            i_tab = new ushort[MAX + 1];
            int n = 0;
            for (ushort i = 0; i <= N; i++)
                for (ushort j = 0; j <= i; j++)
                    i_tab[n++] = i;
        }
#endif
        /// <summary>
        /// Separate sign and index
        /// </summary>
        static void masksign(ref uint n, out byte sign)
        {
            sign = (byte)(n >> 21);
            n &= MASK;
        }
        /// <summary>
        /// convert to cartesian vector
        /// </summary>
        static Vector3f cartesian(uint i, uint j, byte sign)
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
        /// Summation of sequence for j=0, or Gauss formula, or triangular number, or...
        /// </summary>
        static uint sum(uint i) => (i + 1) * i / 2;

        /// <summary>
        /// inverse of summation.
        /// </summary>
        static void inverse(uint n, out uint i, out uint j)
        {
            //check for n[0,MAX]
            i = (uint)(Math.Sqrt(1 + 8 * n) - 1) / 2;
            j = n - sum(i);
        }

        public static uint Encode(Vector3f normal)
        {
            uint value = 0;
            if (normal.x < 0) { value |= 4; normal.x *= -1; }
            if (normal.y < 0) { value |= 2; normal.y *= -1; }
            if (normal.z < 0) { value |= 1; normal.z *= -1; }

            normal.Normalize();
            float theta = (float)Math.Atan2(normal.z, normal.x);
            float phi = (float)Math.Acos(normal.y);

            uint i = (uint)Math.Round(phi / d_phi);
            uint j = (uint)Math.Round(theta * i / PI_2);
            uint n = sum(i) + j;

            return (value << 21) | n;
        }

        public static Vector3f Decode(uint value)
        {
            masksign(ref value, out byte sign);
            inverse(value, out var i, out var j);
            return cartesian(i, j, sign);
        }
    }
}

