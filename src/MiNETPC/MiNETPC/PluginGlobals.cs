using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MiNET.Blocks;
using MiNET.Utils;
using MiNET.Worlds;
using MiNETPC.Classes;
using MiNETPC.Packages;

namespace MiNETPC
{
	class PluginGlobals
	{
		public static TcpListener ServerListener = new TcpListener(IPAddress.Any, 25565);
		public static Level Level;
		public static List<Player> pcPlayers = new List<Player>();
		public static List<Player> pePlayers = new List<Player>(); 
		public static Dictionary<string, string> Users = new Dictionary<string, string>();

		public static int LastUniqueID = 0;
		public static string ProtocolName = "MiNET PC 1.8";
		public static int ProtocolVersion = 47;
		public static int MaxPlayers = 10;
		public static string MOTD = "MiNET TEST";
		public static int PEIDOffset = 10000;
		public static int PCIDOffset = 50000;
		private static List<int> _gaps;
		private static List<int> _ignore;

		public static byte GetBlockID(ushort blockId)
		{
			//if (_gaps.Contains(itemid)) return 0;
		//	else return (byte) itemid;
			if (blockId == 125) blockId = 5;
			else if (blockId == 126) blockId = 158;
			else if (blockId == 75) blockId = 50;
			else if (blockId == 76) blockId = 50;
			else if (blockId == 123) blockId = 89;
			else if (blockId == 124) blockId = 89;
			else if (_ignore.BinarySearch(blockId) >= 0) blockId = 0;
			else if (_gaps.BinarySearch(blockId) >= 0)
			{
				blockId = 133;
			}
			if (blockId > 255) blockId = 41;
			return (byte) blockId;
		}

		public static void SendChunk(ChunkCoordinates position)
		{
			ChunkColumn targetchunk = Level._worldProvider.GenerateChunkColumn(position);
			PCChunkColumn converted = new PCChunkColumn {X = position.X, Z = position.Z};
			converted.Pe2Pc(targetchunk);

			foreach (var player in pcPlayers)
			{
				//new BlockChange(player.Wrapper, new MSGBuffer(player.Wrapper)) {BlockID = data.block, MetaData = data.meta, Location = new Vector3(data.x, data.y, data.z)}.Write();
				new ChunkData(player.Wrapper) { Chunk = converted }.Write();
			}
		}

		public static void Initialize()
		{
			_ignore = new List<int>();
			_ignore.Add(23);
			_ignore.Add(25);
			_ignore.Add(28);
			_ignore.Add(29);
			_ignore.Add(33);
			_ignore.Add(34);
			_ignore.Add(36);
			_ignore.Add(55);
			_ignore.Add(69);
			_ignore.Add(70);
			_ignore.Add(71);
			_ignore.Add(72);
			//			_ignore.Add(75);
			//			_ignore.Add(76);
			_ignore.Add(77);
			_ignore.Add(84);
			_ignore.Add(87);
			_ignore.Add(88);
			_ignore.Add(93);
			_ignore.Add(94);
			_ignore.Add(97);
			_ignore.Add(113);
			_ignore.Add(115);
			_ignore.Add(117);
			_ignore.Add(118);
			//			_ignore.Add(123);
			_ignore.Add(131);
			_ignore.Add(132);
			_ignore.Add(138);
			_ignore.Add(140);
			_ignore.Add(143);
			_ignore.Add(144);
			_ignore.Add(145);
			_ignore.Sort();

			_gaps = new List<int>();
			_gaps.Add(23);
			_gaps.Add(25);
			//			_gaps.Add(27);
			_gaps.Add(28);
			_gaps.Add(29);
			_gaps.Add(33);
			_gaps.Add(34);
			_gaps.Add(36);
			_gaps.Add(55);
			//			_gaps.Add(66);
			_gaps.Add(69);
			_gaps.Add(70);
			_gaps.Add(72);
			_gaps.Add(75);
			_gaps.Add(76);
			_gaps.Add(77);
			_gaps.Add(84);
			//			_gaps.Add(87);
			_gaps.Add(88);
			_gaps.Add(90);
			_gaps.Add(93);
			_gaps.Add(94);
			_gaps.Add(95);
			_gaps.Add(97);
			//			_gaps.Add(99);
			//			_gaps.Add(100);
			//			_gaps.Add(106);
			//			_gaps.Add(111);
			_gaps.Add(115);
			_gaps.Add(116);
			_gaps.Add(117);
			_gaps.Add(118);
			_gaps.Add(119);
			//			_gaps.Add(120);
			//			_gaps.Add(121);
			_gaps.Add(122);
			_gaps.Add(123);
			_gaps.Add(124);
			_gaps.Add(125);
			_gaps.Add(126);
			//			_gaps.Add(127);
			_gaps.Add(130);
			_gaps.Add(131);
			_gaps.Add(132);
			_gaps.Add(137);
			_gaps.Add(138);
			_gaps.Add(140);
			_gaps.Add(143);
			_gaps.Add(144);
			_gaps.Add(145);
			_gaps.Add(146);
			_gaps.Add(147);
			_gaps.Add(148);
			_gaps.Add(149);
			_gaps.Add(150);
			_gaps.Add(151);
			_gaps.Add(152);
			_gaps.Add(153);
			_gaps.Add(154);
			_gaps.Add(160);
			_gaps.Add(165);
			_gaps.Add(166);
			_gaps.Add(167);
			_gaps.Add(168);
			_gaps.Add(169);
			_gaps.Sort();
		}

		public static List<Player> GetPlayers()
		{
			List<Player> templist = new List<Player>();
				/*foreach (var player in pePlayers)
				{
					Player p = new Player();
					p.Username = player.Username;
					p.UniqueServerID = PEIDOffset + player.EntityId;
					p.Wrapper = new ClientWrapper(player.EndPoint);
					templist.Add(p);
				}*/
			templist.AddRange(pePlayers);
			templist.AddRange(pcPlayers);
			return templist;
		}

		public static void BroadcastChat(string message)
		{
			foreach (Player player in pcPlayers)
			{
				new ChatMessage(player.Wrapper) {Message = message}.Write();
			}
		}
	}
}
