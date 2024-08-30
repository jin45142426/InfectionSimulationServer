using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{
	public abstract class PacketSession : Session
	{
		public static readonly int HeaderSize = 2;

		// [size(2)][packetId(2)][ ... ][size(2)][packetId(2)][ ... ]
		public sealed override int OnRecv(ArraySegment<byte> buffer)
		{
			int processLen = 0;
			int packetCount = 0;

			//패킷을 조립할 수 있는 만큼 조립하기
			while (true)
			{
				// 최소한 헤더는 파싱할 수 있는지 확인
				if (buffer.Count < HeaderSize)
					break;

				// 패킷이 완전체로 도착했는지 확인
				ushort dataSize = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
				if (buffer.Count < dataSize)
					break;

				// 여기까지 왔으면 패킷 조립 가능
				OnRecvPacket(new ArraySegment<byte>(buffer.Array, buffer.Offset, dataSize));
				packetCount++;

				processLen += dataSize;
				buffer = new ArraySegment<byte>(buffer.Array, buffer.Offset + dataSize, buffer.Count - dataSize);
			}

			//if (packetCount > 1)
   //             Console.WriteLine($"패킷 모아보내기 : {packetCount}");

            return processLen;
		}

		public abstract void OnRecvPacket(ArraySegment<byte> buffer);
	}

	public abstract class Session
	{
		//세션에서 통신에 사용되는 소켓
		Socket _socket;
		//연결이 끊겼는지 여부
		int _disconnected = 0;

		//소켓에서 수신한 패킷이 저장되는 버퍼
		RecvBuffer _recvBuffer = new RecvBuffer(65535);

		object _lock = new object();
		//송신할 패킷을 저장해두는 큐
		Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
		//비동기 송신 과정에서 송신 대기 중인 패킷들이 대기하는 리스트
		List<ArraySegment<byte>> _pendingList = new List<ArraySegment<byte>>();

		//패킷 송신 성공 시 호출할 콜백 함수
		SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
		//패킷 수신 성공 시 호출할 콜백 함수
		SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

		public abstract void OnConnected(EndPoint endPoint);
		public abstract int  OnRecv(ArraySegment<byte> buffer);
		public abstract void OnSend(int numOfBytes);
		public abstract void OnDisconnected(EndPoint endPoint);

		void Clear()
		{
			lock (_lock)
			{
				_sendQueue.Clear();
				_pendingList.Clear();
			}
		}

		public void Start(Socket socket)
		{
			_socket = socket;

			//비동기 통신 성공 시 발생할 콜백함수 등록
			_recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
			_sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

			//패킷 수신 시작
			RegisterRecv();
		}

		public void Send(List<ArraySegment<byte>> sendBuffList)
		{
			if (sendBuffList.Count == 0)
				return;

			lock (_lock)
			{
				foreach (ArraySegment<byte> sendBuff in sendBuffList)
					_sendQueue.Enqueue(sendBuff);

				if (_pendingList.Count == 0)
					RegisterSend();
			}
		}

		//Send의 경우, 하나의 세션에 대하여 여러 채널(?)에서 동시에 Send를 수행할 수 있기 때문에 lock으로 보호
		public void Send(ArraySegment<byte> sendBuff)
		{
			//send큐에 보낼 내용 저장, 앞서 Send를 수행 중인 쓰레드가 없다면 RegisterSend()를 수행
			lock (_lock)
			{
				_sendQueue.Enqueue(sendBuff);
				if (_pendingList.Count == 0)
					RegisterSend();
			}
		}

		//세션의 소켓의 연결을 종료
		public void Disconnect()
		{
			if (Interlocked.Exchange(ref _disconnected, 1) == 1)
				return;

			OnDisconnected(_socket.RemoteEndPoint);
			_socket.Shutdown(SocketShutdown.Both);
			_socket.Close();
			Clear();
		}

		#region 네트워크 통신

		void RegisterSend()
		{
			if (_disconnected == 1)
				return;

			//_sendQueue에 등록된 패킷들을 _pendingList로 옮긴 뒤, sendArgs의 버퍼에 저장
			while (_sendQueue.Count > 0)
			{
				ArraySegment<byte> buff = _sendQueue.Dequeue();
				_pendingList.Add(buff);
			}
			_sendArgs.BufferList = _pendingList;

			try
			{
				//args에 저장된 버퍼의 비동기 전송을 시도, 전송이 완료되면 OnSendCompleted 호출
				bool pending = _socket.SendAsync(_sendArgs);
				//SendAsync는 동기적으로 수행될 경우, false가 반환되기 때문에 이 경우를 처리해줘야 함
				if (pending == false)
					OnSendCompleted(null, _sendArgs);
			}
			catch (Exception e)
			{
				Console.WriteLine($"RegisterSend Failed {e}");
			}

		}

		//패킷 송신에 성공하면 호출되는 콜백 함수
		void OnSendCompleted(object sender, SocketAsyncEventArgs args)
		{
			lock (_lock)
			{
				if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
				{
					try
					{
						//이미 전송된 찌꺼기? 청소
						_sendArgs.BufferList = null;
						_pendingList.Clear();

						OnSend(_sendArgs.BytesTransferred);

						//비동기로 연결 수행하는 동안 _sendQueue에 대기 중인 패킷이 있다면 연결 등록 수행
						//내가 또 수행하는 이유? => lock을 가진 스레드가 변경되면 CPU가 제어권이 다른 스레드로 변환되며
						//Context Switching(문맥 교환)의 발생 비용이 부담되기 때문
						if (_sendQueue.Count > 0)
							RegisterSend();
					}
					catch (Exception e)
					{
						Console.WriteLine($"OnSendCompleted Failed {e}");
					}
				}
				else
				{
					Disconnect();
				}
			}
		}

		void RegisterRecv()
		{
			if (_disconnected == 1)
				return;

			//버퍼가 고갈되지 않도록 청소
			_recvBuffer.Clean();
			//세션의 recvBuffer의 쓰기 범위로 recvArgs의 버퍼를 세팅
			ArraySegment<byte> segment = _recvBuffer.WriteSegment;
			_recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

			try
			{
				//비동기 수신 수행, 수신하면 OnRecvCompleted 호출
				bool pending = _socket.ReceiveAsync(_recvArgs);
				//ReceiveAsync는 동기적으로 수행될 경우, false가 반환되기 때문에 이 경우를 처리해줘야 함
				if (pending == false)
					OnRecvCompleted(null, _recvArgs);
			}
			catch (Exception e)
			{
				Console.WriteLine($"RegisterRecv Failed {e}");
			}
		}

		//패킷 수신 시 콜백 함수
		void OnRecvCompleted(object sender, SocketAsyncEventArgs args)
		{
			//소켓에서 통신된 바이트의 수가 0보다 크다 => 통신이 성공했다
			if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
			{
				try
				{
					// 버퍼의 Write 위치 이동
					if (_recvBuffer.OnWrite(args.BytesTransferred) == false)
					{
						Disconnect();
						return;
					}

					// 컨텐츠 쪽으로 데이터를 넘겨주고 얼마나 처리했는지 받는다
					int processLen = OnRecv(_recvBuffer.ReadSegment);
					if (processLen < 0 || _recvBuffer.DataSize < processLen)
					{
						Disconnect();
						return;
					}

					// 버퍼의 Read 위치 이동
					if (_recvBuffer.OnRead(processLen) == false)
					{
						Disconnect();
						return;
					}

					RegisterRecv();
				}
				catch (Exception e)
				{
					Console.WriteLine($"OnRecvCompleted Failed {e}");
				}
			}
			else
			{
				Disconnect();
			}
		}

		#endregion
	}
}
