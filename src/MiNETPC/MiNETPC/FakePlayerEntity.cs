using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiNET.Entities;
using MiNET.Net;
using MiNET.Utils;
using MiNET.Worlds;
using MiNET;

namespace MiNETPC
{
	public class FakePlayerEntity : Entity
	{
		public FakePlayerEntity(Level level) : base(-1, level)
		{
			Level.EntityManager.AddEntity(null, this);
		}

		public void UpdatePosition(PlayerLocation newLocation)
		{
			KnownPosition = newLocation;
		}

		public void SendMovePlayer()
		{
			var package = McpeMovePlayer.CreateObject();
			package.entityId = EntityId;
			package.x = KnownPosition.X;
			package.y = KnownPosition.Y;
			package.z = KnownPosition.Z;
			package.yaw = KnownPosition.Yaw;
			package.pitch = KnownPosition.Pitch;
			package.bodyYaw = KnownPosition.BodyYaw;
			package.teleport = 0x80;

			foreach (Player pl in Level.Players)
			{
				pl.SendPackage(package);
			}
		}
	}
}
