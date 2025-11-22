using System;
using System.IO;
using System.Globalization;

namespace MathLib
{
	static class Program
	{
		[STAThread]
		static void Main()
		{
			//wavefront accept only dot decimal
			CultureInfo.CurrentCulture = new CultureInfo("en-US");

			//open file with https://imagetostl.com/view-obj-online
			Console.WriteLine("Create wavefront obj rappresentation");
			int numverts = 0;
			using (var file = File.Create("unitvectors.obj"))
			using (var writer = new StreamWriter(file))
			{
				for (int sign = 0; sign <8;sign++)
					for (int n = 0; n <= 8127; n++)
					{
						ushort encode0 = (ushort)(n | (sign << 13));
						var v0 = SphericalPacker16.Decode(encode0);
						var encode = SphericalPacker16.Encode(v0);
						var v = SphericalPacker16.Decode(encode);
						//decode>encode>decode must generate the same result
						writer.WriteLine(string.Format("v {0:0.000000} {1:0.000000} {2:0.000000}", v.x, v.y, v.z));
						numverts++;
					}
				writer.WriteLine($"# {numverts} geometry vertices");
				writer.WriteLine("o sphere\ng sphere\np");
				for (int i = 1; i <= numverts; i++)
					writer.Write($" {i}");
				writer.WriteLine("# 1 polygons as pointlist");
			}
		}
	}
}


