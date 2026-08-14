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

        object _lock = new object();// 큐에 데이터를 넣고 빼는 작업이 동시에 일어나면 문제가 생기므로 lock을 걸어줌
        Queue<byte[]> _sendQueue=new Queue<byte[]>();// 전송할 데이터를 담을 큐
        bool _pending = false;// 전송중인지 체크용
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();// 송신전용 소켓 생성

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

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);// sendArgs객체의 Completed델리게이트에 OnSendCompleted 함수를 등록


            RegisterRecv(recvArgs);// 수신 대기 시작
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pending == false)
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)//disconnected된 상태에서 다시 disconnnected하는것 방지
                return;


            _socket.Shutdown(SocketShutdown.Both);// 양방향통신 종료신호 보냄
            _socket.Close();// 소켓 리소스 해제
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            // 이미 락을 건 상태에서 호출 함으로 락을 걸 필요 없음
            _pending = true;
            byte[] buff= _sendQueue.Dequeue();// 큐에서 데이터를 하나 꺼냄
            _sendArgs.SetBuffer(buff,0, buff.Length);// 버퍼연결

            bool pending = _socket.SendAsync(_sendArgs);
            if (pending == false)
                OnSendCompleted(null, _sendArgs);
        }

        void OnSendCompleted(object sender, SocketAsyncEventArgs args)
        {
            // 콜백으로 호출된 경우 락이 필요
            lock (_lock)
            {
                if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)// 전송 성공
                {
                    // TODO
                    try
                    {
                        if(_sendQueue.Count>0)
                            RegisterSend();// 큐에 데이터가 남아있으면 다시 전송
                        else
                             _pending = false;


                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"OnSendCompleted Failed {e}");
                    }
                }
                else// 전송 실패
                {
                    Disconnect();// 연결 해제
                }
            }
            
        }

        void RegisterRecv(SocketAsyncEventArgs args)
        {
            bool pending = _socket.ReceiveAsync(args);// 비동기적으로 연결 요청을 받음
            // 현재 이밴트가 하나밖에 없기때문에 완료될때마다 각기 다른 스레드에서 호출될수는 있어도 동시에 호출될 일은 없음
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
                Disconnect();// 연결 해제

            }
        }
        #endregion
    }
}