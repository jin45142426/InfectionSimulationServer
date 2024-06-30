using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestClient;

class PacketHandler
{
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame getPacket = (S_LeaveGame)packet;
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn getPacket = (S_Spawn)packet;
    }

    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move getPacket = (S_Move)packet;
        ServerSession serverSession = (ServerSession)session;
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn getPacket = (S_Despawn)packet;
    }

    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame getPacket = (S_EnterGame)packet;
    }
}