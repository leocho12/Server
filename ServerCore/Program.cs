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
                //전송

                Session session = new Session();//세션에 소켓을 집어 넣어 연동 완료
                session.Start(clientSocket);
                byte[] sendBuff = Encoding.UTF8.GetBytes("welcome to my server");//전송할 데이터를 바이트 배열로 변환
                session.Send(sendBuff);

                Thread.Sleep(1000);

                session.Disconnect();
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

