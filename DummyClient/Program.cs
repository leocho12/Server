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
    class Packet// 패킷을 보낼땐 최대한 압축해서 보내는게 중요
    {
        public ushort size;
        public ushort packetId;
    }
    class GameSession : Session
    {
        public override void OnConected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            Packet packet = new Packet() { size = 4, packetId = 7 };

            //전송
            for (int i = 0; i < 5; i++)
            {
                ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
                byte[] buffer = BitConverter.GetBytes(packet.size);
                byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
                Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
                Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
                ArraySegment<byte> sendBuff = SendBufferHelper.Close(packet.size);

                Send(sendBuff);
            }
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