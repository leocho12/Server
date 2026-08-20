using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class Packet// 패킷을 보낼땐 최대한 압축해서 보내는게 중요
    {
        public ushort size;
        public ushort packetId;
    }
    

    class GameSession : PacketSession
    {
        public override void OnConected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected: {endPoint}");

            //전송
            //Packet packet = new Packet() { size = 100, packetId = 10 };

            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
            //byte[] buffer = BitConverter.GetBytes(packet.size);
            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
            //Array.Copy(buffer2, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);


            // 100명
            // 1->이동패킷이 100명
            // 100-> 이동 패킷이 100*100=10000개
            //Send(sendBuff);
            Thread.Sleep(5000);
            Disconnect();
        }
        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            ushort size=BitConverter.ToUInt16(buffer.Array, buffer.Offset);
            ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + sizeof(ushort));
            Console.WriteLine($"RecvPacketId: {id}, Size: {size}");
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnDisconnected: {endPoint}");
        }


        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transfer Completed: {numOfBytes}");
        }
    }
    class Program
    {
        static Listener _listener = new Listener();// Listener가 새 연결을 잡을 때마다 호출되는 콜백. 클라이언트 1명당 1번씩 실행 -> 클라가 서버에 접속할 때 마다 OnAcceptHandler함수가 한번 씩 실행됨



        static void Main(string[] args)
        {
            //DNS

            string host = Dns.GetHostName();// 컴퓨터의 호스트 이름 조회
            IPHostEntry ipHost = Dns.GetHostEntry(host);// 호스트 이름으로 IP주소 조회
            IPAddress ipAddr = ipHost.AddressList[0];// IP주소 중 첫번째 주소를 가져옴
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);// IP주소와 포트번호를 묶어 endpoint 생성

            // 문지기의 무전기 생성

            _listener.Init(endPoint, () => { return new GameSession(); });
            Console.WriteLine("Listening...");


            while (true)
            {

            }


        }
    }
}