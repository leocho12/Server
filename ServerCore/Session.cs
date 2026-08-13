using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{

    //동기: 호출한 스레드가 전송 완료 될때 까지 정지
    //비동기: 호출한 스레드가 전송 완료 될때 까지 정지하지 않고 바로 리턴

    internal class Session// 같은 프로젝트 안에서만 이 클래스 사용가능
    {
        Socket _socket;
        int _disconnected = 0;// 연결 상태 체크용 0=연결중, 1=끊김

        public void Start(Socket socket)
        {
            _socket = socket;// 전달 받은 소켓 저장
            SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();// 수신전용 소켓 생성
            recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRevCompleted);// recvArgs객체의 Completed델리게이트에 OnRevCompleted 함수를 등록
            /*
             OnRevCompleted (Session.cs에 정의된 우리 함수)
                ↓ 시그니처가 일치하므로 등록 가능
            EventHandler<SocketAsyncEventArgs> (델리게이트 타입/규격)
                ↓ 이 타입으로 선언된 필드
            SocketAsyncEventArgs.Completed (실제로 함수를 담는 상자)
             */
            recvArgs.SetBuffer(new byte[1024], 0, 1024);// 데이터를 담을 버퍼 지정

            RegisterRecv(recvArgs);// 수신 대기 시작
        }

        public void Send(byte[] sendBuff)
        {
            _socket.Send(sendBuff);// 동기전송 호출한 스레드가 전송 완료 될때 까지 정지
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)//disconnected된 상태에서 다시 disconnnected하는것 방지
                return;


            _socket.Shutdown(SocketShutdown.Both);// 양방향통신 종료신호 보냄
            _socket.Close();// 소켓 리소스 해제
        }

        #region 네트워크 통신
        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);// 비동기적으로 연결 요청을 받음
            if (pending == false)// 연결 즉시 잡혀서 이밴트가 불리지 않음 -> 직접 OnRevCompleted 호출
                OnRevCompleted(null, args);
        }

        void OnRevCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)// 실제로 데이터가 도착 했으면
            {
                // TODO
                try
                {
                    string recvData = Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);//수신한 데이터를 문자열로 변환
                    Console.WriteLine($"[From Client] {recvData}");// 출력
                    RegisterRecv(args);// 수신 대기 시작
                }
                catch (Exception e)
                {
                    Console.WriteLine($"OnRecevCompleted Failed {e}");
                }
            }
            else
            {
                // TODO : disconnect

            }
        }
        #endregion
    }
}