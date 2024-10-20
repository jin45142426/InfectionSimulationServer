using ServerCore;
using System.Net;

namespace TestClient
{
    class Program
	{
		static List<System.Timers.Timer> _timers = new List<System.Timers.Timer>();
		public static int count = 0;

		static void Profiling(int tick = 1000)
		{
			var timer = new System.Timers.Timer();
			timer.Interval = tick;
			timer.Elapsed += ((s, e) => { Console.WriteLine($"현재까지 보낸 패킷 수 : {count}"); });
			timer.AutoReset = true;
			timer.Enabled = true;

			_timers.Add(timer);
		}

		static void Main(string[] args)
		{
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[ipHost.AddressList.Length - 1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			Connector connector = new Connector();
			
			Thread.Sleep(1000);
			connector.Connect(endPoint,
				() => { return SessionManager.Instance.Generate(); },
				1);

			Profiling(1000);

			SessionManager.Instance.RegistClients();

			//SessionManager.Instance.LoginClients();

			//while (true)
			//{
			//	try
			//	{
			//		SessionManager.Instance.MoveClients();
			//	}
			//	catch (Exception e)
			//	{
			//		Console.WriteLine(e.ToString());
			//	}

			//	Thread.Sleep(20);
			//}
		}
	}
}
