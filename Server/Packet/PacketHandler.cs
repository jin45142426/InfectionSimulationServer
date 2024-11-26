using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.Game;
using ServerCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whisper.net;

class PacketHandler
{
	public static void C_MoveHandler(PacketSession session, IMessage packet)
	{
		C_Move movePacket = (C_Move)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleMove, player, movePacket);
	}

    public static void C_SyncHandler(PacketSession session, IMessage packet)
    {
		C_Sync syncPacket = (C_Sync)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleSync, player, syncPacket);
    }

    public static void C_LoginHandler(PacketSession session, IMessage packet)
    {
		C_Login loginPacket = (C_Login)packet;

		ClientSession clientSession = (ClientSession)session;

        // 로그인 인증 되면 게임씬으로 이동시키기
        clientSession.HandleLogin(loginPacket.AccountId, loginPacket.AccountPw, loginPacket.Position);
    }

    public static void C_EquipHandler(PacketSession session, IMessage packet)
    {
		C_Equip equipPacket = (C_Equip)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;
		 
		room.Push(room.HandleEquip, player, equipPacket);
	}

    public static void C_UnEquipHandler(PacketSession session, IMessage packet)
    {
		C_UnEquip unEquipPacket = (C_UnEquip)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleUnEquip, player, unEquipPacket);
	}

    public static void C_TalkHandler(PacketSession session, IMessage packet)
    {
		C_Talk talkPacket = (C_Talk)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleTalk, player, talkPacket);
	}

	public static void C_RegistAccountHandler(PacketSession session, IMessage packet)
	{
		C_RegistAccount registPacket = (C_RegistAccount)packet;

		ClientSession clientSession = (ClientSession)session;

		clientSession.HandleRegistAccount(registPacket.AccountId, registPacket.AccountPw, registPacket.PlayerName);
	}

    public static void C_CompleteHandler(PacketSession session, IMessage packet)
    {
        C_Complete completePacket = (C_Complete)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleComplete, player, completePacket);
	}

    public static void C_StartScenarioHandler(PacketSession session, IMessage packet)
    {
        C_StartScenario scenarioPacket = (C_StartScenario)packet;
		ClientSession clientSession = (ClientSession)session;

		Player player = clientSession.MyPlayer;
		if (player == null)
			return;

		GameRoom room = player.Room;
		if (room == null)
			return;

		room.Push(room.HandleScenario, clientSession.MyPlayer, scenarioPacket);
    }

	public static void C_EndGameHandler(PacketSession session, IMessage packet)
	{
        C_EndGame scenarioPacket = (C_EndGame)packet;

        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleEndGame, clientSession.MyPlayer, scenarioPacket);
    }

    public static void C_LeaveLobbyHandler(PacketSession session, IMessage packet)
    {
        C_LeaveLobby leavePacket = (C_LeaveLobby)packet;

        ClientSession clientSession = (ClientSession)session;

		Program.Lobby.Push(Program.Lobby.LeaveLobby, clientSession, leavePacket);
    }

    public static void C_MakeRoomHandler(PacketSession session, IMessage packet)
    {
        C_MakeRoom makePacket = (C_MakeRoom)packet;

        ClientSession clientSession = (ClientSession)session;

        Program.Lobby.Push(Program.Lobby.MakeRoom, clientSession, makePacket);
    }

    public static void C_LeaveRoomHandler(PacketSession session, IMessage packet)
    {
        C_LeaveRoom makePacket = (C_LeaveRoom)packet;

        ClientSession clientSession = (ClientSession)session;

        Program.Lobby.Push(Program.Lobby.LeaveRoom, clientSession, makePacket);
    }

    public static void C_RequestRoomListHandler(PacketSession session, IMessage packet)
    {
        C_RequestRoomList roomPacket = (C_RequestRoomList)packet;

        ClientSession clientSession = (ClientSession)session;

        Program.Lobby.Push(Program.Lobby.SendRoomList, clientSession);
    }

    public static void C_RequestRoomInfoHandler(PacketSession session, IMessage packet)
    {
        C_RequestRoomInfo makePacket = (C_RequestRoomInfo)packet;

        ClientSession clientSession = (ClientSession)session;

        Program.Lobby.Push(Program.Lobby.SendRoomInfo, clientSession);
    }

    public static void C_ChangePositionHandler(PacketSession session, IMessage packet)
    {
        C_ChangePosition positionPacket = (C_ChangePosition)packet;

        ClientSession clientSession = (ClientSession)session;

        Program.Lobby.Push(Program.Lobby.RoomChangePosition, clientSession, positionPacket);
    }

    public static void C_EnterRoomHandler(PacketSession session, IMessage packet)
    {
        C_EnterRoom roomPacket = (C_EnterRoom)packet;

        ClientSession clientSession = (ClientSession)session;

        Program.Lobby.Push(Program.Lobby.EnterRoom, clientSession, roomPacket);
    }

    public static void C_StartSimulationHandler(PacketSession session, IMessage packet)
    {
        C_StartSimulation roomPacket = (C_StartSimulation)packet;

        ClientSession clientSession = (ClientSession)session;

        Program.Lobby.Push(Program.Lobby.StartSimulation, clientSession, roomPacket);
    }

    public static void C_ExitGameHandler(PacketSession session, IMessage packet)
    {
        C_ExitGame scenarioPacket = (C_ExitGame)packet;

        ClientSession clientSession = (ClientSession)session;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleExitGame, clientSession.MyPlayer, scenarioPacket);
    }
}
