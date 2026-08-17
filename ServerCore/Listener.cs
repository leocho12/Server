using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServerCore
{
    internal class Listener// Listener는 델리게이트(Action<Socket>)로 "연결되면 뭘 할지"를 주입받음
    {
        Socket _listenSocket;// 클라의 연결요청을 받는 소켓
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);// 소켓 생성
            _sessionFactory += sessionFactory;// 델리게이트에 Program에서 전달받은 콜백함수 등록

            // 문지기의 주소 연동
            _listenSocket.Bind(endPoint);// 이 소켓은 endpoint포트로 들어오는 연결을 받도록 바인딩

            //영업시작
            //backlog : 대기열의 최대 길이
            _listenSocket.Listen(10);//대기열의 길이 설정

            SocketAsyncEventArgs args=new SocketAsyncEventArgs();// 비동기 소켓 작업에 필요한 정보를 담아두기 위한 클래스
            args.Completed+=new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);// 작업이 끝나면 args내부의 complete이밴트에 OnAcceptCompleted를 부르도록 등록
            RegisterAccept(args);// 호출 대기 시작
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;// args의 AcceptSocket을 null로 초기화

            bool pending = _listenSocket.AcceptAsync(args);// 비동기적으로 연결 요청을 받음
            if (pending == false)// 연결 즉시 잡혀서 이밴트가 불리지 않음 -> 직접 OnAcceptCompleted 호출
                OnAcceptCompleted(null, args);
        }

        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)//물고기가 잡혀서 낚시대 끌어올림
        {
            if(args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConected(args.AcceptSocket.RemoteEndPoint);// 세션에 연결된 클라의 EndPoint를 전달
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
            RegisterAccept(args);// 다시 호출대기 시작
        }
    }
}
