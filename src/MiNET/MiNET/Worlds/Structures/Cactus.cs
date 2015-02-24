using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Utils;

namespace MiNET.Worlds.Structures
{
	class CactusStructure : Structure
	{
		public override string Name
		{
			get { return "Cactus"; }
		}

		public override Block[] Blocks
		{
			get
			{
				return new Block[]
				{
					new Block(81) {Coordinates = new BlockCoordinates(0,1,0)}, 
					new Block(81) {Coordinates = new BlockCoordinates(0,2,0)}, 
					new Block(81) {Coordinates = new BlockCoordinates(0,3,0)}, 
				};
			}
		}
	}
}
