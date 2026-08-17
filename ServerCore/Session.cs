using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerCore
{

    //동기: 호출한 스레드가 전송 완료 될때 까지 정지
    //비동기: 호출한 스레드가 전송 완료 될때 까지 정지하지 않고 바로 리턴

    public abstract class Session// 같은 프로젝트 안에서만 이 클래스 사용가능
    {
        Socket _socket;
        int _disconnected = 0;// 연결 상태 체크용 0=연결중, 1=끊김

        object _lock = new object();// 큐에 데이터를 넣고 빼는 작업이 동시에 일어나면 문제가 생기므로 lock을 걸어줌
        Queue<byte[]> _sendQueue=new Queue<byte[]>();// 전송할 데이터를 담을 큐
        List<ArraySegment<byte>> _pendinglist = new List<ArraySegment<byte>>();// 미리 리스트를 만들고  재사용하기 위해 클래스 안에 생성
        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();// 송신전용 소켓 생성  재사용하기 위해 클래스 안에 생성
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();// 수신전용 소켓 생성  재사용하기 위해 클래스 안에 생성

        public abstract void OnConected(EndPoint endPoint);// 연결되었을 때 호출될 함수
        public abstract void OnRecv(ArraySegment<byte> buffer);// 데이터가 도착했을 때 호출될 함수
        public abstract void OnSend(int numOfBytes);// 데이터가 전송될 때 호출될 함수
        public abstract void OnDisconnected(EndPoint endPoint);// 연결이 끊겼을 때 호출될 함수

        public void Start(Socket socket)
        {
            _socket = socket;// 전달 받은 소켓 저장

            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRevCompleted);// recvArgs객체의 Completed델리게이트에 OnRevCompleted 함수를 등록
            /*
             OnRevCompleted (Session.cs에 정의된 우리 함수)
                ↓ 시그니처가 일치하므로 등록 가능
            EventHandler<SocketAsyncEventArgs> (델리게이트 타입/규격)
                ↓ 이 타입으로 선언된 필드
            SocketAsyncEventArgs.Completed (실제로 함수를 담는 상자)
             */
            _recvArgs.SetBuffer(new byte[1024], 0, 1024);// 데이터를 담을 버퍼 지정

            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);// sendArgs객체의 Completed델리게이트에 OnSendCompleted 함수를 등록


            RegisterRecv();// 수신 대기 시작
        }

        public void Send(byte[] sendBuff)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuff);
                if (_pendinglist.Count == 0)// 리스트가 비어있으면 전송중이 아니므로 바로 전송 시작
                    RegisterSend();
            }
        }

        public void Disconnect()
        {
            if (Interlocked.Exchange(ref _disconnected, 1) == 1)//disconnected된 상태에서 다시 disconnnected하는것 방지
                return;

            OnDisconnected(_socket.RemoteEndPoint);
            _socket.Shutdown(SocketShutdown.Both);// 양방향통신 종료신호 보냄
            _socket.Close();// 소켓 리소스 해제
        }

        #region 네트워크 통신

        void RegisterSend()
        {
            // 이미 락을 건 상태에서 호출 함으로 락을 걸 필요 없음

            _pendinglist.Clear();// 리스트 초기화
            while (_sendQueue.Count > 0)// sendqueue가 빌 때 까지
            {
                byte[] buff = _sendQueue.Dequeue();// 큐에서 데이터를 하나 꺼냄
                _pendinglist.Add(new ArraySegment<byte>(buff, 0, buff.Length));// 버퍼연결
            }
            _sendArgs.BufferList = _pendinglist;// 리스트에 추가

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
                        _sendArgs.BufferList = null;// 버퍼리스트 초기화
                        _pendinglist.Clear();// 리스트 초기화
                        OnSend(_sendArgs.BytesTransferred);
                        

                        if (_sendQueue.Count>0)
                            RegisterSend();// 큐에 데이터가 남아있으면 다시 전송
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

        void RegisterRecv()
        {
            bool pending = _socket.ReceiveAsync(_recvArgs);// 비동기적으로 연결 요청을 받음
            // 현재 이밴트가 하나밖에 없기때문에 완료될때마다 각기 다른 스레드에서 호출될수는 있어도 동시에 호출될 일은 없음
            if (pending == false)// 연결 즉시 잡혀서 이밴트가 불리지 않음 -> 직접 OnRevCompleted 호출
                OnRevCompleted(null, _recvArgs);
        }

        void OnRevCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)// 실제로 데이터가 도착 했으면
            {
                // TODO
                try
                {
                    OnRecv(new ArraySegment<byte>(args.Buffer, args.Offset, args.BytesTransferred));

                    RegisterRecv();// 수신 대기 시작
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