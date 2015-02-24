using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Worlds.Structures;

namespace MiNET.Worlds.BiomeSystem
{
	interface iBiome
	{
		bool Overhang { get; }
		double OverhangsMagnitude { get; }
		double BottomsMagnitude { get; }
		Dictionary<int, Block> Blocks { get; }
		Block TopBlock { get; }
		Structure TreeStructure { get; }
	}
}
