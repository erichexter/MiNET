using System;
using System.Threading;
using MiNET.API;
using MiNET.Net;
using MiNET.PluginSystem.Attributes;
using MiNET;
using MiNET.Entities;
using MiNET.Worlds;
using MiNETPC.Packages;
using Package = MiNET.Net.Package;

namespace MiNETPC
{
	[Plugin("MiNETPC", "A proof of concept plugin", "pre 1.0", "kennyvv")]
    public class MiNetpc : MiNETPlugin
    {
		public override void OnEnable(Level level)
		{
			ConsoleFunctions.WriteInfoLine("[MiNET PC] Loading...");
			PluginGlobals.Initialize();
			PluginGlobals.Level = level;
			ConsoleFunctions.WriteInfoLine("[MiNET PC] Initiating server...");
			var ClientListener = new Thread(() => new Networking.BasicListener().ListenForClients());
			ClientListener.Start();
		}

		public override void OnDisable()
		{
			Disconnect.Broadcast("Server shutting down...");
		}
    }
}
