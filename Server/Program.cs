using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Server.Game;
using ServerCore;
using Whisper.net;

namespace Server
{
	class Program
	{
		static Listener _listener = new Listener();
		static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();
		static Server.Lobby.Lobby _lobby = new Server.Lobby.Lobby();

		static void TickRoom<T>(T room, int tick = 100) where T : JobSerializer
		{
			var timer = new System.Timers.Timer();
			timer.Interval = tick;
			timer.Elapsed += ((s, e) => { room.Update(); });
			timer.AutoReset = true;
			timer.Enabled = true;

			_timers.Add(timer);
		}

		static void Main(string[] args)
		{
			GameRoom room = RoomManager.Instance.Add(1);
			TickRoom(room, 50);

			string host = Dns.GetHostName();
			//IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPHostEntry ipHost = Dns.GetHostEntry("CGlabHospital.iptime.org");
			IPAddress ipAddr = ipHost.AddressList[ipHost.AddressList.Length - 1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			while (true)
			{
				_lobby.Update();
				Thread.Sleep(500);
			}
		}
	}
}
