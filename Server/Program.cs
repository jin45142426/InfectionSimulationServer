using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Timers;
using Server.Game;
using ServerCore;
using Whisper.net;

namespace Server
{
    class Program
    {
        static Listener _listener = new Listener();
        static Dictionary<JobSerializer, System.Timers.Timer> _roomTimers = new Dictionary<JobSerializer, System.Timers.Timer>();
        public static Server.Lobby.Lobby Lobby = new Server.Lobby.Lobby();

        public static int recvPacketCount = 0;
        public static int completePacketCount = 0;
        public static int packetOverCount = 0;

        public static void TickRoom<T>(T room, int tick = 100) where T : JobSerializer
        {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += ((s, e) => { room.Update(); });
            timer.AutoReset = true;
            timer.Enabled = true;

            _roomTimers.Add(room, timer);
        }

        public static void StopTickRoom<T>(T room) where T : JobSerializer
        {
            if (!_roomTimers.TryGetValue(room, out System.Timers.Timer timer))
                return;

            timer.Stop();
            timer.Dispose();
            _roomTimers.Remove(room);
        }

        static void Profiling(int tick = 1000)
        {
            var timer = new System.Timers.Timer();
            timer.Interval = tick;
            timer.Elapsed += ((s, e) => Console.WriteLine("Working..."));
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        static void Main(string[] args)
        {
            string host = Dns.GetHostName();
            IPAddress ipAddr = IPAddress.Parse("220.69.209.153");
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            Profiling(3000);

            while (true)
            {
                Lobby.Update();
                Thread.Sleep(100);
            }
        }
    }
}
