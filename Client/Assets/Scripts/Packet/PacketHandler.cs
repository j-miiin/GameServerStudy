﻿using DummyClient;
using ServerCore;
using UnityEngine;

internal class PacketHandler
{
    public static void S_ChatHandler(PacketSession session, IPacket packet)
    {
        S_Chat chatPacket = packet as S_Chat;
        ServerSession serverSession = session as ServerSession;

        if (chatPacket.playerId == 1) Debug.Log(chatPacket.chat);
        //if (chatPacket.playerId == 1)
            //Console.WriteLine(chatPacket.chat);
    }
}
