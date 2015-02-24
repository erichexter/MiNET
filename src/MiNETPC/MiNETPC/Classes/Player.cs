using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiNET.Entities;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;
using MiNETPC.Packages;

namespace MiNETPC.Classes
{
	public class Player
	{
		public string Username { get; set; }
		public string UUID { get; set; }
		public ClientWrapper Wrapper { get; set; }
		public int UniqueServerID { get; set; }
		public GameMode Gamemode { get; set; }
		public bool IsSpawned { get; set; }
		public bool Digging { get; set; }

		//Location stuff
		public byte Dimension { get; set; }
		public Vector3 Coordinates { get; set; }
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public bool OnGround { get; set; }

		//Client settings
		public string Locale { get; set; }
		public byte ViewDistance { get; set; }
		public byte ChatFlags { get; set; }
		public bool ChatColours { get; set; }
		public byte SkinParts { get; set; }

		//Map stuff
		Vector2 CurrentChunkPosition = new Vector2(0, 0);
		public bool ForceChunkReload { get; set; }
		private Dictionary<Tuple<int, int>, ChunkColumn> _chunksUsed;

		//Inventory stuff
		public byte CurrentSlot = 0;

		public MiNET.Player PlayerEntity;

		public FakePlayerEntity FpEntity;
		public Player()
		{
			_chunksUsed = new Dictionary<Tuple<int, int>, ChunkColumn>();
		}

		public void AddToList()
		{
			PluginGlobals.pcPlayers.Add(this);
		}

		public static Player GetPlayer(ClientWrapper wrapper)
		{
			foreach (Player i in PluginGlobals.GetPlayers())
			{
				if (i.Wrapper == wrapper)
				{
					return i;
				}
			}
			throw new ArgumentOutOfRangeException("The specified player could not be found ;(");
		}

		public void SendChunksFromPosition()
		{
			if (Coordinates == null)
			{
				Coordinates = PluginGlobals.Level.SpawnPoint;
				ViewDistance = 9;
			}
			SendChunksForKnownPosition(false);
		}

		public void SendChunksForKnownPosition(bool force = false)
		{
			int centerX = (int)Coordinates.X >> 4;
			int centerZ = (int)Coordinates.Z >> 4;

			if (!force && IsSpawned && CurrentChunkPosition == new Vector2(centerX, centerZ)) return;

			CurrentChunkPosition.X = centerX;
			CurrentChunkPosition.Z = centerZ;

			new Thread(() =>
			{
				int Counted = 0;

				foreach (
					var chunk in
						PluginGlobals.Level.GenerateChunks(new ChunkCoordinates((int) Coordinates.X, (int) Coordinates.Z),
							force ? new Dictionary<Tuple<int, int>, ChunkColumn>() : _chunksUsed))
				{
					PCChunkColumn pcchunk = new PCChunkColumn {X = chunk.x, Z = chunk.z};

					for (int y = 0; y < 128; y++)
					{
						for (int x = 0; x < 16; x++)
						{
							for (int z = 0; z < 16; z++)
							{
								pcchunk.SetBlock(x, y, z, chunk.GetBlock(x, y, z), chunk.GetMetadata(x, y, z));
							}
						}
					}

					new ChunkData(Wrapper, new MSGBuffer(Wrapper)) {Chunk = pcchunk}.Write();
					//new ChunkData().Write(Wrapper, new MSGBuffer(Wrapper), new object[]{ chunk.GetBytes() });

					Thread.Yield();

					if (Counted >= 5 && !IsSpawned)
					{

						new PlayerPositionAndLook(Wrapper).Write();

						IsSpawned = true;
						PluginGlobals.pcPlayers.Add(this);

						/*foreach (var player in PluginGlobals.GetPlayers())
						{
							new PlayerListItem(Wrapper)
							{
								Action = 0,
								Username = player.Username,
								Gamemode = player.Gamemode,
								UUID = UUID
							}.Write();
						}*/

						//PluginGlobals.Level.AddPlayer(new MiNET.Player(null, null, PluginGlobals.Level, 0, Username) { KnownPosition = new PlayerLocation((float)Coordinates.X, (float)Coordinates.Y, (float)Coordinates.Z) { Pitch = Pitch, Yaw = Yaw} });
						
						foreach (var targetPlayer in PluginGlobals.Level.Players)
						{
							//PluginGlobals.Level.SendAddForPlayer(targetPlayer, this);
							//PluginGlobals.Level.SendAddForPlayer(newPlayer, targetPlayer);
							targetPlayer.SendPackage( new McpeAddPlayer
								{
									clientId = 0,
									username = Username,
									entityId = PluginGlobals.PCIDOffset + UniqueServerID,
									x = (float)Coordinates.X,
									y = (float)Coordinates.Y,
									z = (float)Coordinates.Z,
									yaw = (byte)Yaw,
									pitch = (byte)Pitch,
									metadata = new byte[0]
								});

							PluginGlobals.Level.RelayBroadcast(new McpeAddEntity
							{
								entityType = -1,
								entityId = PluginGlobals.PCIDOffset + UniqueServerID,
								x = (float)Coordinates.X,
								y = (float)Coordinates.Y,
								z = (float)Coordinates.Z,
							});
						}

							foreach (var player2 in PluginGlobals.GetPlayers())
							{
								new PlayerListItem(Wrapper)
								{
									Action = 0,
									Username = player2.Username,
									Gamemode = player2.Gamemode,
									UUID = UUID
								}.Write();

								if (player2 != this)
								{
									new SpawnPlayer(Wrapper)
									{
										Player = player2
									}.Write();
								}
							}

						SendMovePlayer();

						PluginGlobals.Level.BroadcastTextMessage(Username  + " joined the game!");
					}
					Counted++;
				}
			}).Start();
		}

		public void SendMovePlayer()
		{
			var package = McpeMovePlayer.CreateObject();
			package.entityId = PluginGlobals.PCIDOffset + UniqueServerID;
			package.x = (float)Coordinates.X;
			package.y = (float)Coordinates.Y + 1.62f;
			package.z = (float)Coordinates.Z;
			package.yaw = Yaw;
			package.pitch = Pitch;
			package.bodyYaw = Yaw;
			package.teleport = 0x80;

			foreach (MiNET.Player pl in PluginGlobals.Level.GetSpawnedPlayers())
			{
				pl.SendPackage(package);
			}
		}
	}
}
