using ServerCore;
using System.Net;

namespace TestClient
{
    class Program
	{
		static void Main(string[] args)
		{
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[ipHost.AddressList.Length - 1];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			Connector connector = new Connector();

			connector.Connect(endPoint,
				() => { return SessionManager.Instance.Generate(); },
				20);

			while (true)
			{
				try
				{
					SessionManager.Instance.SendForEach();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
				}

				Thread.Sleep(250);
			}
		}
	}
}
