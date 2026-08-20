using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;// 커서역할을 하는 변수 배열을 가리키는 포인터랑 같은역할
        int _writePos;
        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        public int FreeSize { get { return _buffer.Count - _writePos; } }

        public ArraySegment<byte> ReadSegment// 어디부터 데이터를 읽으면 되나 요청
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }
        public ArraySegment<byte> WriteSegment// 어디부터 데이터를 써야하나 요청
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }

        public void Clean()
        {
            int dataSize = DataSize;
            if (dataSize == 0)
            {
                // 남은데이터가 없으면 복사하지 않고 커서를 처음으로 옮김
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은데이터가 있으면 앞으로 당김
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }

        public bool OnRead(int numOfBytes)// 읽은만큼 커서를 옮김
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }

        public bool OnWrite(int numOfBytes)// 쓴만큼 커서를 옮김
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}
