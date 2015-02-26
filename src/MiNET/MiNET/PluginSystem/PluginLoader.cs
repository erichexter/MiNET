using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using MiNET.API;
using MiNET.PluginSystem.Attributes;
using MiNET.Worlds;

namespace MiNET.PluginSystem
{
	internal class PluginLoader
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(MiNetServer));
		private List<IMiNETPlugin> Plugins = new List<IMiNETPlugin>();
		public static Dictionary<Attribute, MethodInfo> PacketHandlerDictionary = new Dictionary<Attribute, MethodInfo>();
		public static Dictionary<Attribute, MethodInfo> PacketSendHandlerDictionary = new Dictionary<Attribute, MethodInfo>();
		public static Dictionary<Attribute, MethodInfo> PlayerLoginDictionary = new Dictionary<Attribute, MethodInfo>();
		public static Dictionary<Attribute, MethodInfo> PlayerDisconnectDictionary = new Dictionary<Attribute, MethodInfo>();
		public static Dictionary<Attribute, MethodInfo> OnEntityDamageDictionary = new Dictionary<Attribute, MethodInfo>(); 

		public void LoadPlugins()
		{
				if (!Directory.Exists("Plugins"))
					Directory.CreateDirectory("Plugins");

				string pluginsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Plugins");
				foreach (string pluginPath in Directory.GetFiles(pluginsFolder, "*.dll", SearchOption.TopDirectoryOnly))
				{
					Assembly newAssembly = Assembly.LoadFile(pluginPath);
					Type[] types = newAssembly.GetExportedTypes();
					foreach (Type type in types)
					{
						try
						{
							new Task(() => GetCommandHandlers(type)).Start();
							new Task(() => GetPacketEvents(type)).Start();
							new Task(() => GetPlayerLoginHandlers(type)).Start();
							new Task(() => GetPlayerDisconnectHandlers(type)).Start();
							new Task(() => GetOnEntityDamageHandlers(type)).Start();

							if (!type.IsDefined(typeof (PluginAttribute), true)) continue;
							var ctor = type.GetConstructor(new Type[] {});
							if (ctor != null)
							{
								var plugin = ctor.Invoke(new object[] {}) as IMiNETPlugin;
								Plugins.Add(plugin);
							}
						}
						catch (Exception ex)
						{
							Log.Warn("Plugin Error: " + ex);
						}
					}
				}
		}

		private void GetOnEntityDamageHandlers(Type type)
		{
			var methods = type.GetMethods();
			foreach (MethodInfo method in methods)
			{
				var cmd = Attribute.GetCustomAttribute(method,
					typeof(OnPlayerInteractAttribute), false) as OnPlayerInteractAttribute;
				if (cmd == null)
					continue;
				OnEntityDamageDictionary.Add(cmd, method);
			}
		}


		private void GetPlayerLoginHandlers(Type type)
		{
			var methods = type.GetMethods();
			foreach (MethodInfo method in methods)
			{
				var cmd = Attribute.GetCustomAttribute(method,
					typeof(HandlePlayerLoginAttribute), false) as HandlePlayerLoginAttribute;
				if (cmd == null)
					continue;
				PlayerLoginDictionary.Add(cmd, method);
			}
		}

		private void GetPlayerDisconnectHandlers(Type type)
		{
			var methods = type.GetMethods();
			foreach (MethodInfo method in methods)
			{
				var cmd = Attribute.GetCustomAttribute(method,
					typeof(HandlePlayerDisconnectAttribute), false) as HandlePlayerDisconnectAttribute;
				if (cmd == null)
					continue;
				PlayerDisconnectDictionary.Add(cmd, method);
			}
		}

		private void GetCommandHandlers(Type type)
		{
			var methods = type.GetMethods();
			foreach (MethodInfo method in methods)
			{
				var cmd = Attribute.GetCustomAttribute(method,
					typeof (CommandAttribute), false) as CommandAttribute;
				if (cmd == null)
					continue;
				CommandHandler.CommandHandler.PluginCommands.Add(cmd, method);
			}
		}

		private void GetPacketEvents(Type type)
		{
			var methods = type.GetMethods();
			foreach (MethodInfo method in methods)
			{
				var packetevent = Attribute.GetCustomAttribute(method,
					typeof(HandlePacketAttribute), false) as HandlePacketAttribute;
				if (packetevent == null)
					continue;
				PacketHandlerDictionary.Add(packetevent, method);
			}

			foreach (MethodInfo method in methods)
			{
				var packetevent = Attribute.GetCustomAttribute(method,
					typeof(HandleSendPacketAttribute), false) as HandleSendPacketAttribute;
				if (packetevent == null)
					continue;
				PacketSendHandlerDictionary.Add(packetevent, method);
			}
		}

		public void EnablePlugins(Level level)
		{
			foreach (IMiNETPlugin miNetPlugin in Plugins)
			{
				try
				{
					miNetPlugin.OnEnable(level);
				}
				catch (Exception ex)
				{
					Log.Warn("Plugin Error: " + ex);
				}
			}
		}

		public void DisablePlugins()
		{
			foreach (IMiNETPlugin miNetPlugin in Plugins)
			{
				try
				{
					miNetPlugin.OnDisable();
				}
				catch (Exception ex)
				{
					Log.Warn("Plugin Error: " + ex);
				}
			}
		}
	}
}