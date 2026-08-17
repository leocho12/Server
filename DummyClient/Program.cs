using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;

namespace DummyClient
{
    class GameSession : Session
    {
        public override void OnConected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            //전송
            for (int i = 0; i < 5; i++)
            {
                byte[] sendBuff = Encoding.UTF8.GetBytes($"Hello World {i}");
                Send(sendBuff);
            }
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override void OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);//수신한 데이터를 문자열로 변환
            Console.WriteLine($"[From Server] {recvData}");// 출력
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfer Completed: {numOfBytes}");
        }
    }
    class Program
    {
        static void Main(string[] args) 
        {
            //DNS

            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            Connector connector = new Connector();

            connector.Conect(endPoint, () => { return new GameSession(); });

            while (true)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString);
                }
                Thread.Sleep(100);
            }

           

            
        }
    }
}