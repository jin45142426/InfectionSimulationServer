using Google.Protobuf;
using Google.Protobuf.Protocol;

namespace TestClient
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; } }

		List<ServerSession> _sessions = new List<ServerSession>();
		object _lock = new object();

		public void LoginClients()
        {
			int count = 0;

			lock (_lock)
			{
				foreach (ServerSession session in _sessions)
				{
					C_Login loginPacket = new C_Login();
					loginPacket.UserInfo = new UserInfo();
					loginPacket.UserInfo.Name = count.ToString();
					loginPacket.UserInfo.Id = count.ToString();
					loginPacket.UserInfo.Position = "응급센터 간호사1";
					count++;
					session.Send(loginPacket);
				}
			}
		}

		public void MoveClients()
		{
			lock (_lock)
			{
				foreach (ServerSession session in _sessions)
				{
					C_Move chatPacket = new C_Move();
					session.Send(chatPacket);
				}
			}
		}

		public ServerSession Generate()
		{
			lock (_lock)
			{
				ServerSession session = new ServerSession();
				_sessions.Add(session);
				return session;
			}
		}
	}
}
