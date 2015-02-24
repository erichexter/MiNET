using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace MiNET.Worlds.BiomeSystem
{
	class BiomeFinder
	{
		private SimplexOctaveGenerator _s1;
		public BiomeFinder(int seed)
		{
			_s1 = new SimplexOctaveGenerator(seed, 2);

			_s1.SetScale(1 / 64.0);
		}

		private int _getBiome(int x, int z)
		{
			var v1 = _s1.Noise(x, z, 0.5, 0.5);

			var total = v1 * (_biomes.Length - 1);

			var result = Convert.ToInt32(Math.Round(total));
			if (result > (_biomes.Length - 1)) result = _biomes.Length;
			if (result < 0) result = 0;

			return result;
		}

		public iBiome GetBiome(int x, int z)
		{
			try
			{
				int g = _getBiome(x, z);
				return _biomes[g];
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);	
			}
			return new ExtremeBiome();
		}

		private readonly iBiome[] _biomes = { new ExtremeBiome(), new DesertBiome(), new PlanesBiome(),    };
	}
}
