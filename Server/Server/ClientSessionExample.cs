﻿//using ServerCore;
//using System.Net;

//namespace Server
//{
//    class ClientSession : PacketSession
//    {
//        public override void OnConnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"OnConnected : {endPoint}");

//            //Packet packet = new Packet() { size = 100, packetId = 10 };

//            // 보낸다
//            //byte[] sendBuff = Encoding.UTF8.GetBytes("Welcome to MMORPG Server!");
//            //byte[] sendBuff = new byte[4096];
//            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);
//            //byte[] buffer = BitConverter.GetBytes(packet.size);
//            //byte[] buffer2 = BitConverter.GetBytes(packet.packetId);
//            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset, buffer.Length);
//            //Array.Copy(buffer, 0, openSegment.Array, openSegment.Offset + buffer.Length, buffer2.Length);
//            //ArraySegment<byte> sendBuff = SendBufferHelper.Close(buffer.Length + buffer2.Length);

//            //Send(sendBuff);
//            Thread.Sleep(5000);
//            Disconnect();
//        }

//        public override void OnRecvPacket(ArraySegment<byte> buffer)
//        {
//            PacketManager.Instance.OnRecvPacket(this, buffer);
//        }

//        //public override int OnRecv(ArraySegment<byte> buffer)
//        //{
//        //    string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
//        //    Console.WriteLine($"[From Client] {recvData}");
//        //    return buffer.Count;
//        //}

//        public override void OnDisconnected(EndPoint endPoint)
//        {
//            Console.WriteLine($"OnDisconnected : {endPoint}");
//        }

//        public override void OnSend(int numOfBytes)
//        {
//            Console.WriteLine($"Transferred bytes: {numOfBytes}"); ;
//        }
//    }
//}
