using System;
using System.Net;
using System.Threading.Tasks;
using MiNET.Net;
using MiNET.PluginSystem.Attributes;
using MiNET.Worlds;

namespace MiNET.PluginSystem
{
	public class PluginHandlers
	{

		/// <summary>
		/// Handle the plugin OnPlayerLogin Attributes.
		/// </summary>
		/// <param name="source">The source.</param>
		public static void PlayerLoginHandler(Player source)
		{
			try
			{
				var target = source;
				foreach (var handler in PluginLoader.PlayerLoginDictionary)
				{
					var atrib = (HandlePlayerLoginAttribute) handler.Key;
					if (atrib == null) continue;
					var method = handler.Value;

					if (method == null) return;
					if (method.IsStatic)
					{
						new Task(() => method.Invoke(null, new object[] {target})).Start();
					}
					else
					{
						object obj = Activator.CreateInstance(method.DeclaringType);
						new Task(() => method.Invoke(obj, new object[] {target})).Start();
					}
				}
			}
			catch (Exception ex)
			{
				//For now we will just ignore this, not to big of a deal.
				//Will have to think a bit more about this later on.
				//Log.Warn("Plugin Error: " + ex);
			}
		}


		/// <summary>
		/// Handles the disconnecting of players.
		/// </summary>
		/// <param name="source">The source.</param>
		public static void PlayerDisconnectHandler(Player source)
		{
			try
			{
				var target = source;
				foreach (var handler in PluginLoader.PlayerDisconnectDictionary)
				{
					var atrib = (HandlePlayerDisconnectAttribute) handler.Key;
					if (atrib == null) continue;

					var method = handler.Value;
					if (method == null) return;

					if (method.IsStatic)
					{
						new Task(() => method.Invoke(null, new object[] {target})).Start();
					}
					else
					{
						object obj = Activator.CreateInstance(method.DeclaringType);
						new Task(() => method.Invoke(obj, new object[] {target})).Start();
					}
				}
			}
			catch (Exception ex)
			{
				//For now we will just ignore this, not to big of a deal.
				//Will have to think a bit more about this later on.
				//Log.Warn("Plugin Error: " + ex);
			}
		}


		public static void PluginPacketHandler(Package message, IPEndPoint senderEndPoint, Level level)
		{
			try
			{
				var target = level.GetPlayer(senderEndPoint);
				if (target == null) return;

				foreach (var handler in PluginLoader.PacketHandlerDictionary)
				{
					var atrib = (HandlePacketAttribute)handler.Key;
					if (atrib.Packet == null) continue;

					if (atrib.Packet != message.GetType()) continue;

					var method = handler.Value;
					if (method == null) return;

					if (method.IsStatic)
					{
						new Task(() => method.Invoke(null, new object[] { message, target })).Start();
					}
					else
					{
						object obj = Activator.CreateInstance(method.DeclaringType);
						new Task(() => method.Invoke(obj, new object[] { message, target })).Start();
					}
				}
			}
			catch (Exception ex)
			{
				//For now we will just ignore this, not to big of a deal.
				//Will have to think a bit more about this later on.
				//Log.Warn("Plugin Error: " + ex);
			}
		}


		public static void PluginSendPacketHandler(Package message, IPEndPoint receiveEndPoint, Level level)
		{
			try
			{
				var target = level.GetPlayer(receiveEndPoint);

				if (target == null) return;

				foreach (var handler in PluginLoader.PacketSendHandlerDictionary)
				{
					var atrib = (HandleSendPacketAttribute)handler.Key;
					if (atrib.Packet == null) continue;

					if (atrib.Packet != message.GetType()) continue;

					var method = handler.Value;
					if (method == null) return;

					if (method.IsStatic)
					{
						new Task(() => method.Invoke(null, new object[] { message, target })).Start();
					}
					else
					{
						object obj = Activator.CreateInstance(method.DeclaringType);
						new Task(() => method.Invoke(obj, new object[] { message, target })).Start();
					}
				}
			}
			catch (Exception ex)
			{
				//For now we will just ignore this, not to big of a deal.
				//Will have to think a bit more about this later on.
				//Log.Warn("Plugin Error: " + ex);
			}
		}

		public static void HandlePlayerInteractPlugin(int entityId)
		{
			try
			{
				foreach (var handler in PluginLoader.OnEntityDamageDictionary)
				{
					var atrib = (OnPlayerInteractAttribute) handler.Key;
					if (atrib == null) continue;
					var method = handler.Value;

					if (method == null) return;

					if (method.IsStatic)
					{
						new Task(() => method.Invoke(null, new object[] {entityId})).Start();
					}
					else
					{
						object obj = Activator.CreateInstance(method.DeclaringType);
						new Task(() => method.Invoke(obj, new object[] {entityId})).Start();
					}
				}
			}
			catch (Exception ex)
			{
				//For now we will just ignore this, not to big of a deal.
				//Will have to think a bit more about this later on.
				//Log.Warn("Plugin Error: " + ex);
			}
		}
	}
}
