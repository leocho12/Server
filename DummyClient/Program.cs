using ServerCore;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ServerCore;
using System.Security.Cryptography;

// TCP: 전송순서 보장됨    속도느림    신뢰성 높음  택배배송 서비스로 정해진 물류라인이 있음  연결되었다는 약속을 맺음   캐치볼을 함
// UDP: 전송순서 보장 안됨 속도빠름    신뢰성 낮음  퀵서비스로 정해진 물류라인이 없음                                  바구니에 공을 던짐

namespace DummyClient
{
   
    class GameSession : Session
    {
        public override void OnConected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            //전송
            Knight knight = new Knight() { hp = 100, attack = 10 };

            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            byte[] buffer=BitConverter.GetBytes(knight.hp);
            byte[] buffer2=BitConverter.GetBytes(knight.attack);
            Array.Copy(buffer,0, openSegment.Array, openSegment.Offset, buffer.Length);
            Array.Copy(buffer2,0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            ArraySegment<byte> sendBuff= SendBufferHelper.Close(buffer.Length + buffer2.Length);


            // 100명
            // 1->이동패킷이 100명
            // 100-> 이동 패킷이 100*100=10000개
            Send(sendBuff);
            Thread.Sleep(1000);
            Disconnect();
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }

        public override int OnRecv(ArraySegment<byte> buffer)
        {
            string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);//수신한 데이터를 문자열로 변환
            Console.WriteLine($"[From Server] {recvData}");// 출력
            return buffer.Count;
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