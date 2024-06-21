using Google.Protobuf.Protocol;

namespace TestClient
{
	class SessionManager
	{
		static SessionManager _session = new SessionManager();
		public static SessionManager Instance { get { return _session; } }

		List<ServerSession> _sessions = new List<ServerSession>();
		object _lock = new object();

		public void SendForEach()
		{
			lock (_lock)
			{
				foreach (ServerSession session in _sessions)
				{
					C_Move chatPacket = new C_Move();
					chatPacket.PosInfo = new PositionInfo();
					chatPacket.PosInfo.State = CreatureState.Moving;
					chatPacket.PosInfo.PosX = new Random().Next(-10, 10);
					chatPacket.PosInfo.PosY = 0;
					chatPacket.PosInfo.PosZ = new Random().Next(-10, 10);

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
