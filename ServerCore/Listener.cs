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
        Action<Socket> _onAcceptHandler;

        public void Init(IPEndPoint endPoint, Action<Socket> _onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            this._onAcceptHandler += _onAcceptHandler;

            // 문지기의 주소 연동
            _listenSocket.Bind(endPoint);

            //영업시작
            //backlog : 대기열의 최대 길이
            _listenSocket.Listen(10);//대기열의 최대 길이 10

            SocketAsyncEventArgs args=new SocketAsyncEventArgs();
            args.Completed+=new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            RegisterAccept(args);//낚시대 던짐
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            if (pending == false)//대기 없이 즉시완료되면
                OnAcceptCompleted(null, args);
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)//물고기가 잡혀서 낚시대 끌어올림
        {
            if(args.SocketError == SocketError.Success)
            {
                _onAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
            RegisterAccept(args);//낚시대 다시 던짐
        }
    }
}
