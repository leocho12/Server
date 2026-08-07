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
    class Program
    {
        static Listener _listener = new Listener();

        static void OnAcceptHandler(Socket clientSocket)
        {
            try
            {
                //수신
                byte[] recvBuff = new byte[1024];//수신버퍼
                int recvBytes = clientSocket.Receive(recvBuff);//수신한 바이트 수
                string recvData = Encoding.UTF8.GetString(recvBuff, 0, recvBytes);//수신한 데이터를 문자열로 변환
                Console.WriteLine($"[From Client] {recvData}");
                //전송
                byte[] sendBuff = Encoding.UTF8.GetBytes("welcome to my server");//전송할 데이터를 바이트 배열로 변환
                clientSocket.Send(sendBuff);//전송
                                            //쫒아내기
                clientSocket.Shutdown(SocketShutdown.Both);//양방향 통신 종료
                clientSocket.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
        }

        static void Main(string[] args)
        {
            //DNS

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // 문지기의 무전기 생성

            _listener.Init(endPoint, OnAcceptHandler);//문지기 초기화
            Console.WriteLine("Listening...");


            while (true)
            {

            }

            
        }
    }
}

