using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    internal class Listener
    {
        Socket _listenSocket;

        public void Init(IPEndPoint endPoint)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // 문지기의 주소 연동
            _listenSocket.Bind(endPoint);

            //영업시작
            //backlog : 대기열의 최대 길이
            _listenSocket.Listen(10);//대기열의 최대 길이 10
        }

        public Socket Accept()
        {
            return _listenSocket.Accept();
        }
    }
}
