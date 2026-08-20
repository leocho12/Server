using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ServerCore
{
    public class SendBufferHelper
    {
        public static ThreadLocal<SendBuffer>CurrentBuffer=new ThreadLocal<SendBuffer>(() => { return null; });// thread끼리의 경합을 없애기 위해 threadlocal로

        public static int ChunkSize { get; set; } = 4096*100;// 크게 잡고 쪼개서 씀

        public static ArraySegment<byte> Open(int reserveSize)
        {
            if(CurrentBuffer.Value == null)// 버퍼가 비어있으면 청크 사이즈 만큼 생성
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            if(CurrentBuffer.Value.FreeSize < reserveSize)// 사용 가능한 버퍼가 부족하면 기존걸 날리고 생성
                CurrentBuffer.Value = new SendBuffer(ChunkSize);

            return CurrentBuffer.Value.Open(reserveSize);
        }

        public static ArraySegment<byte> Close(int usedSize)
        {
            return CurrentBuffer.Value.Close(usedSize);
        }

    }
    public class SendBuffer
        // 다른 세션에서 사용중인 범위를 침해할 수 도 있기 때문에 커서 옮겨주지 않음
    {
        byte[] _buffer;
        int _usedSize = 0;// writebuffer의 writtepos와 같은 역할

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunkSize)
        {
            _buffer = new byte[chunkSize];
        }

        public ArraySegment<byte>Open(int reserveSize)// 사용할 최대치 설정
        {
            if(reserveSize > FreeSize)
                return null;

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);
        }
        public ArraySegment<byte> Close(int usedSize)// 실제로 사용한 사이즈를 알려주고 반환
        {
            ArraySegment<byte> segment=new ArraySegment<byte>(_buffer,_usedSize, usedSize);
            _usedSize += usedSize;
            return segment;
        }
    }
}
