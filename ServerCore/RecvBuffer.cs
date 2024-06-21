using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
	public class RecvBuffer
	{
		ArraySegment<byte> _buffer;
		//버퍼에서 읽어야 되는 위치
		int _readPos;
		//버퍼에서 쓰여지는 위치
		int _writePos;

		public RecvBuffer(int bufferSize)
		{
			_buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
		}

		//버퍼에 들어있는 데이터 크기
		public int DataSize { get { return _writePos - _readPos; } }
		//버퍼에 남아있는 공간
		public int FreeSize { get { return _buffer.Count - _writePos; } }

		//데이터를 읽는 범위
		public ArraySegment<byte> ReadSegment
		{
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
		}

		//데이터를 쓰는 범위
		public ArraySegment<byte> WriteSegment
		{
			get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
		}

		//버퍼를 계속 사용하면 버퍼 공간이 고갈되기 때문에 비워주는 작업이 필요함
		public void Clean()
		{
			int dataSize = DataSize;
			if (dataSize == 0)
			{
				// 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
				_readPos = _writePos = 0;
			}
			else
			{
				// 남은 데이터가 있으면, 읽던 위치와 쓰던 위치를 그대로 배열 앞으로 옮김
				Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
				_readPos = 0;
				_writePos = dataSize;
			}
		}

		public bool OnRead(int numOfBytes)
		{
			//버퍼에 있는 데이터보다 읽는 크기가 크면 문제가 발생할 수 있기 때문에 확인
			if (numOfBytes > DataSize)
				return false;

			//읽은 크기만큼 읽기 위치 이동
			_readPos += numOfBytes;
			return true;
		}

		public bool OnWrite(int numOfBytes)
		{
			//버퍼에 남은 공간보다 쓰기 공간이 더 크면 문제가 있는 것이기 때문에 확인
			if (numOfBytes > FreeSize)
				return false;

			//쓴 위치만큼 쓰기 위치 이동
			_writePos += numOfBytes;
			return true;
		}
	}
}
