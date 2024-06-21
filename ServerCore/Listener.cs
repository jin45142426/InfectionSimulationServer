using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
	public class Listener
	{
		Socket _listenSocket;
		Func<Session> _sessionFactory;

		public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 10, int backlog = 100)
		{
			_listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_sessionFactory += sessionFactory;

			//소켓이 통신에 사용할 IP주소와 포트 번호를 설정
			_listenSocket.Bind(endPoint);

			//연결 시도 수신, 연결 시도를 큐에 대기시키는데 큐에 대기할 수 있는 연결 시도 수를 backlog로 지정
			_listenSocket.Listen(backlog);

			for (int i = 0; i < register; i++)
			{
				SocketAsyncEventArgs args = new SocketAsyncEventArgs();
				args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
				RegisterAccept(args);
			}
		}

		void RegisterAccept(SocketAsyncEventArgs args)
		{
			args.AcceptSocket = null;

			//연결 시도를 비동기로 수락
			bool pending = _listenSocket.AcceptAsync(args);
			if (pending == false)
				OnAcceptCompleted(null, args);
		}

		void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
		{
			//연결이 성공하면, _sessionFactory에 등록된 메서드 실행(세션을 만들어서 반환), args에 등록된 수락된 소켓을 세션의 소켓에 등록
			if (args.SocketError == SocketError.Success)
			{
				Session session = _sessionFactory.Invoke();
				session.Start(args.AcceptSocket);
				session.OnConnected(args.AcceptSocket.RemoteEndPoint);
			}
			else
				Console.WriteLine(args.SocketError.ToString());

			//다시 연결 시도를 비동기로 수락하는 상태로 전환
			RegisterAccept(args);
		}
	}
}
