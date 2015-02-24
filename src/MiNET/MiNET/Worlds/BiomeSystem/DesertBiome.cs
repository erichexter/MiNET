using System.Collections.Generic;
using MiNET.Blocks;
using MiNET.Worlds.Structures;

namespace MiNET.Worlds.BiomeSystem
{
	class DesertBiome : iBiome
	{
		public bool Overhang
		{
			get { return false; }
		}

		public double OverhangsMagnitude
		{
			get { return 16; }
		}

		public double BottomsMagnitude
		{
			get { return 32; }
		}

		public Dictionary<int, Block> Blocks
		{
			get
			{
				return new Dictionary<int, Block>()
				{
					{1, BlockFactory.GetBlockById(1)}
				};
			}
		}

		public Block TopBlock
		{
			get
			{
				return BlockFactory.GetBlockById(12);
			}
		}

		public Structure TreeStructure
		{
			get
			{
				return new CactusStructure();
			}
		}
	}
}
