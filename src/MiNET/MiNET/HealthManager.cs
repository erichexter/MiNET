﻿using System;
using System.ComponentModel;
using System.Reflection;
using MiNET.Entities;
using MiNET.Utils;

namespace MiNET
{
	public enum DamageCause
	{
		[Description("{0} went MIA")] Unknown,
		[Description("{0} was pricked to death")] Contact,
		[Description("{0} was killed by {1}")] EntityAttack,
		[Description("{0}  was shot by {1}")] Projectile,
		[Description("{0} suffocated in a wall")] Suffocation,
		[Description("{0} hit the ground too hard")] Fall,
		[Description("{0} went up in flames")] Fire,
		[Description("{0} burned to death")] FireTick,
		[Description("{0} tried to swim in lava")] Lava,
		[Description("{0} drowned")] Drowning,
		[Description("{0} blew up")] BlockExplosion,
		[Description("{0} blew up")] EntityExplosion,
		[Description("{0} fell out of the world")] Void,
		[Description("{0} died")] Suicide,
		[Description("{0} died magically")] Magic,
		[Description("{0} died a customized death")] Custom
	}

	public class HealthManager
	{
		private int _hearts;
		public Entity Entity { get; set; }
		public int Health { get; set; }
		public short Air { get; set; }
		public bool IsDead { get; set; }
		public int FireTick { get; set; }
		public int CooldownTick { get; set; }
		public bool IsOnFire { get; set; }
		public bool IsInvulnerable { get; set; }
		public DamageCause LastDamageCause { get; set; }
		public Entity LastDamageSource { get; set; }

		public HealthManager(Entity entity)
		{
			Entity = entity;
			ResetHealth();
		}

		public int Hearts
		{
			get { return (int) Math.Ceiling(Health/10d); }
		}

		public void TakeHit(Entity source, int damage = 1, DamageCause cause = DamageCause.Unknown)
		{
			if (CooldownTick > 0) return;

			if (LastDamageCause == DamageCause.Unknown) LastDamageCause = cause;
			LastDamageSource = source;

			Health -= damage*10;

			var player = Entity as Player;
			if (player != null)
			{
				player.SendSetHealth();
				player.BroadcastEntityEvent();
			}

			CooldownTick = 10;

			OnPlayerTakeHit(new HealthEventArgs(this, source, Entity));
		}

		public event EventHandler<HealthEventArgs> PlayerTakeHit;

		protected virtual void OnPlayerTakeHit(HealthEventArgs e)
		{
			EventHandler<HealthEventArgs> handler = PlayerTakeHit;
			if (handler != null) handler(this, e);
		}

		public void Ignite(int ticks = 300)
		{
			Ignite(Entity, ticks);
		}

		public void Ignite(Entity entity, int ticks = 300)
		{
			if (IsDead) return;

			FireTick = ticks;
			IsOnFire = true;
			entity.BroadcastSetEntityData();
		}

		public void Kill()
		{
			if (IsDead) return;

			OnPlayerKilled(new HealthEventArgs(this, LastDamageSource, Entity));

			IsDead = true;
			Health = 0;
			var player = Entity as Player;
			if (player != null)
			{
				player.SendSetHealth();
				player.BroadcastEntityEvent();
			}
			Entity.BroadcastSetEntityData();

			Entity.DespawnEntity();
		}

		public event EventHandler<HealthEventArgs> PlayerKilled;

		protected virtual void OnPlayerKilled(HealthEventArgs e)
		{
			EventHandler<HealthEventArgs> handler = PlayerKilled;
			if (handler != null) handler(this, e);
		}

		public void ResetHealth()
		{
			IsInvulnerable = false;
			Health = 200;
			Air = 300;
			IsOnFire = false;
			FireTick = 0;
			IsDead = false;
			CooldownTick = 0;
			LastDamageCause = DamageCause.Unknown;
		}

		public void OnTick()
		{
			if (CooldownTick > 0) CooldownTick--;

			if (IsDead) return;

			if (IsInvulnerable) Health = 200;

			if (Health <= 0)
			{
				Kill();
				return;
			}

			if (Entity.KnownPosition.Y < 0 && !IsDead)
			{
				TakeHit(null, 100);
				LastDamageCause = DamageCause.Void;
				return;
			}

			if (IsInWater(Entity.KnownPosition))
			{
				Air--;
				if (Air <= 0)
				{
					if (Math.Abs(Air)%10 == 0)
					{
						Health -= 10;
						var player = Entity as Player;
						if (player != null)
						{
							player.SendSetHealth();
							player.BroadcastEntityEvent();
						}
						Entity.BroadcastSetEntityData();
						LastDamageCause = DamageCause.Drowning;
					}
				}

				if (IsOnFire)
				{
					IsOnFire = false;
					FireTick = 0;
					Entity.BroadcastSetEntityData();
				}
			}
			else
			{
				Air = 300;
			}

			if (!IsOnFire && IsInLava(Entity.KnownPosition))
			{
				FireTick = 300;
				IsOnFire = true;
				Entity.BroadcastSetEntityData();
			}

			if (IsOnFire)
			{
				FireTick--;
				if (FireTick <= 0)
				{
					IsOnFire = false;
				}

				if (Math.Abs(FireTick)%20 == 0)
				{
					Health -= 10;
					var player = Entity as Player;
					if (player != null)
					{
						player.SendSetHealth();
						player.BroadcastEntityEvent();
					}
					Entity.BroadcastSetEntityData();
					LastDamageCause = DamageCause.FireTick;
				}
			}
		}

		private bool IsInWater(PlayerLocation playerPosition)
		{
			float y = playerPosition.Y + 1.62f;

			BlockCoordinates waterPos = new BlockCoordinates
			{
				X = (int) Math.Floor(playerPosition.X),
				Y = (int) Math.Floor(y),
				Z = (int) Math.Floor(playerPosition.Z)
			};

			var block = Entity.Level.GetBlock(waterPos);

			if (block == null || (block.Id != 8 && block.Id != 9)) return false;

			return y < Math.Floor(y) + 1 - ((1/9) - 0.1111111);
		}

		private bool IsInLava(PlayerLocation playerPosition)
		{
			var block = Entity.Level.GetBlock(playerPosition);

			if (block == null || (block.Id != 10 && block.Id != 11)) return false;

			return playerPosition.Y < Math.Floor(playerPosition.Y) + 1 - ((1/9) - 0.1111111);
		}

		public static string GetDescription(Enum value)
		{
			FieldInfo fi = value.GetType().GetField(value.ToString());
			DescriptionAttribute[] attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);

			if (attributes.Length > 0)
				return attributes[0].Description;

			return value.ToString();
		}
	}

	public class HealthEventArgs : EventArgs
	{
		public Entity SourceEntity { get; set; }
		public Entity TargetEntity { get; set; }
		public HealthManager HealthManager { get; set; }

		public HealthEventArgs(HealthManager healthManager, Entity sourceEntity, Entity targetEntity)
		{
			SourceEntity = sourceEntity;
			TargetEntity = targetEntity;
			HealthManager = healthManager;
		}
	}
}