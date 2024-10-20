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

		public static int recvPacketCount = 0;
		public static int completePacketCount = 0;
		public static int packetOverCount = 0;

		static void TickRoom<T>(T room, int tick = 100) where T : JobSerializer
		{
			var timer = new System.Timers.Timer();
			timer.Interval = tick;
			timer.Elapsed += ((s, e) => { room.Update(); });
			timer.AutoReset = true;
			timer.Enabled = true;

			_timers.Add(timer);
		}

		static void Profiling(int tick = 1000)
		{
			var timer = new System.Timers.Timer();
			timer.Interval = tick;
            timer.Elapsed += ((s, e) => { if (recvPacketCount - completePacketCount > 10000) packetOverCount++; Console.WriteLine($"현재까지 받은 패킷 수 : {recvPacketCount} / 처리되지 않은 패킷 수 : {recvPacketCount - completePacketCount} / 패킷 과부하 횟수 : {packetOverCount}"); });
			timer.AutoReset = true;
			timer.Enabled = true;

			_timers.Add(timer);
		}

		static void Main(string[] args)
		{
			GameRoom room = RoomManager.Instance.Add();
			TickRoom(room, 50);

			string host = Dns.GetHostName();
			//IPHostEntry ipHost = Dns.GetHostEntry(host);
			//IPAddress ipAddr = ipHost.AddressList[ipHost.AddressList.Length - 1];
            IPAddress ipAddr = IPAddress.Parse("0.0.0.0");
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			Profiling(1000);

			while (true)
			{
				_lobby.Update();
				Thread.Sleep(500);
			}
		}
	}
}
