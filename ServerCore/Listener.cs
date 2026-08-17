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
        Action<Socket> _onAcceptHandler;// 연결되면 Program에 알려줄 콜백함수 델리게이트임
        /*
        델리게이트는 여러 함수를 담을 수 있는 리스트 같은 구조
            _onAcceptHandler += A;   // [A]
            _onAcceptHandler += B;   // [A, B]
            _onAcceptHandler += C;   // [A, B, C]
        이 상태에서 _onAcceptHandler.Invoke(socket)를 호출하면 A, B, C가 순서대로 호출됨

        */

        public void Init(IPEndPoint endPoint, Action<Socket> _onAcceptHandler)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);// 소켓 생성
            this._onAcceptHandler += _onAcceptHandler;// 델리게이트에 Program에서 전달받은 콜백함수 등록

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
                _onAcceptHandler.Invoke(args.AcceptSocket);// 델리게이트를 실행해 담겨있는 함수들을 실행
            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
            RegisterAccept(args);// 다시 호출대기 시작
        }
    }
}
