﻿using ServerCore;
using System.Net;

namespace Server
{
    class ClientSession : PacketSession
    {
        public int SessionId { get; set; }  
        public GameRoom Room { get; set; }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            Program.Room.Enter(this);
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        //public override int OnRecv(ArraySegment<byte> buffer)
        //{
        //    string recvData = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
        //    Console.WriteLine($"[From Client] {recvData}");
        //    return buffer.Count;
        //}

        public override void OnDisconnected(EndPoint endPoint)
        {
            SessionManager.Instance.Remove(this);
            if (Room != null)
            {
                Room.Exit(this);
                Room = null;
            }
            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            Console.WriteLine($"Transferred bytes: {numOfBytes}"); ;
        }
    }
}