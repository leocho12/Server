using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//=========================================
//Socket programming
//=========================================
namespace ServerCore
{
    class GameSession : Session
    {
        public override void OnConected(EndPoint endPoint)
        {
            
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);//수신한 데이터를 문자열로 변환
            Console.WriteLine($"[From Client] {recvData}");// 출력
        }

        public override void OnSend(int numOfBytes)
        {
            
        }
    }
    class Program
    {
        static Listener _listener = new Listener();// Listener가 새 연결을 잡을 때마다 호출되는 콜백. 클라이언트 1명당 1번씩 실행 -> 클라가 서버에 접속할 때 마다 OnAcceptHandler함수가 한번 씩 실행됨

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                //전송

                Session session = new GameSession();//세션에 소켓을 집어 넣어 연동 완료
                session.Start(clientSocket);
                byte[] sendBuff = Encoding.UTF8.GetBytes("welcome to my server");//전송할 데이터를 바이트 배열로 변환
                session.Send(sendBuff);// 전송

                Thread.Sleep(1000);// 1초간 강제 대기

                session.Disconnect();// 연결 해제
                session.Disconnect();// 중복방지 테스트
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        static void Main(string[] args)
        {
            //DNS

            string host = Dns.GetHostName();// 컴퓨터의 호스트 이름 조회
            IPHostEntry ipHost = Dns.GetHostEntry(host);// 호스트 이름으로 IP주소 조회
            IPAddress ipAddr = ipHost.AddressList[0];// IP주소 중 첫번째 주소를 가져옴
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);// IP주소와 포트번호를 묶어 endpoint 생성

            // 문지기의 무전기 생성

            _listener.Init(endPoint, OnAcceptHandler);//Listener 가동 연결되면 OnAcceptHandler를 실행하도록 연결
            Console.WriteLine("Listening...");


            while (true)
            {

            }

            
        }
    }
}

