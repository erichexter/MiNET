using System;
using MiNET.Net;
using MiNET.PluginSystem.Attributes;
using MiNET.Utils;
using MiNET.Worlds;
using MiNETPC.Classes;
using MiNETPC.Packages;
using Package = MiNET.Net.Package;
using Player = MiNET.Player;

namespace MiNETPC
{
	public class PEPacketReader
	{
		[HandlePacket(typeof(McpeMessage))]
		public void HandleChatPacket(Package packet, Player source)
		{
			McpeMessage data = (McpeMessage) packet;
			PluginGlobals.BroadcastChat("<" + source.Username + "> " + data.message.Replace("\\", "\\\\").Replace("\"", "\'\'"));
		}

		[HandleSendPacket(typeof (McpeUpdateBlock))]
		public void ChangeBlockHandler(Package packet, Player source)
		{
			McpeUpdateBlock data = (McpeUpdateBlock) packet;
			ChunkCoordinates target = new ChunkCoordinates(data.x >> 4, data.z >> 4);
			ChunkColumn targetchunk = PluginGlobals.Level._worldProvider.GenerateChunkColumn(target);
			PCChunkColumn converted = new PCChunkColumn { X = target.X, Z = target.Z };

			converted.Pe2Pc(targetchunk);

			foreach (var player in PluginGlobals.pcPlayers)
			{
				//new BlockChange(player.Wrapper, new MSGBuffer(player.Wrapper)) {BlockID = data.block, MetaData = data.meta, Location = new Vector3(data.x, data.y, data.z)}.Write();
				new ChunkData(player.Wrapper){ Chunk = converted, Quee = false}.Write();
			}
		}

		[HandlePacket(typeof (McpeMovePlayer))]
		public void HandleMovePacket(Package packet, Player source)
		{
			
			McpeMovePlayer data = (McpeMovePlayer) packet;

			foreach (var player in PluginGlobals.pcPlayers)
			{
				new EntityTeleport(player.Wrapper, new MSGBuffer(player.Wrapper)) {Coordinates = new Vector3(data.x, data.y, data.z), OnGround = false, Yaw = (byte)data.yaw, Pitch = (byte)data.pitch, UniqueServerID = PluginGlobals.PEIDOffset + data.entityId}.Write();
			}
		}

		[HandlePlayerDisconnect]
		public void HandleDisconnect(Player player)
		{
			foreach (var playerd in PluginGlobals.pePlayers)
			{
				if (playerd.Username == player.Username)
				{
					foreach (var playerd2 in PluginGlobals.pcPlayers)
					{
						new PlayerListItem(playerd2.Wrapper) {Action = 4, Gamemode = GameMode.Creative, Username = playerd.Username, UUID = playerd.UUID}.Write();
					}
					break;
				}
			}
			PluginGlobals.BroadcastChat("\\u00A7e" + player.Username + " has left the game...");
		}

		[HandlePlayerLogin]
		public void HandleLogin(Player player)
		{
			MiNETPC.Classes.Player p = new Classes.Player();
			p.Username = player.Username;
			p.UniqueServerID = PluginGlobals.PEIDOffset + player.EntityId ;
			p.Wrapper = new ClientWrapper();
			p.UUID = Guid.NewGuid().ToString();
			p.PlayerEntity = player;
			p.Coordinates = new Vector3(player.KnownPosition.X, player.KnownPosition.Y, player.KnownPosition.Z);
			p.Yaw = player.KnownPosition.Yaw;
			p.Pitch = player.KnownPosition.Pitch;

			PluginGlobals.pePlayers.Add(p);
		
				//PluginGlobals.Level.SendAddForPlayer(targetPlayer, this);
				//PluginGlobals.Level.SendAddForPlayer(newPlayer, targetPlayer);

				foreach (var pc in PluginGlobals.pcPlayers) //Send PC players to PE
				{
					player.SendPackage(new McpeAddPlayer
					{
						clientId = 0,
						username = pc.Username,
						entityId = PluginGlobals.PCIDOffset + pc.UniqueServerID,
						x = (float) pc.Coordinates.X,
						y = (float) pc.Coordinates.Y,
						z = (float) pc.Coordinates.Z,
						yaw = (byte) pc.Yaw,
						pitch = (byte) pc.Pitch,
						metadata = new byte[0]
					});

					player.SendPackage(new McpeAddEntity
					{
						entityType = -1,
						entityId = PluginGlobals.PCIDOffset + pc.UniqueServerID,
						x = (float) pc.Coordinates.X,
						y = (float)pc.Coordinates.Y,
						z = (float)pc.Coordinates.Z,
					});
				}

				foreach (var playerd in PluginGlobals.pcPlayers) //Send PE Players to PC
				{
					new PlayerListItem(playerd.Wrapper)
					{
						Action = 0,
						Username = p.Username,
						Gamemode = GameMode.Creative,
						UUID = p.UUID
					}.Write();

					new SpawnPlayer(playerd.Wrapper)
					{
						Player = p
					}.Write();
				}

			PluginGlobals.BroadcastChat("\\u00A7e" + player.Username + " joined the game!");
		}

	/*	[HandlePacket(typeof(McpePlayerEquipment))]
		public void HandleChangeItem(Package packet, Player source)
		{
			McpePlayerEquipment data = (McpePlayerEquipment) packet;
			var itemid = data.item;
			var metadata = data.meta;

			foreach (var player in PluginGlobals.pcPlayers)
			{
				new EntityEquipment(player.Wrapper) {ItemID = (short)(itemid << 4 | metadata), ItemCount = 1, EntityID = PluginGlobals.PEIDOffset + source.EntityId, Slot = 0}.Write();
			}
		}*/
	}
}
