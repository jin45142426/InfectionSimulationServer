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

	public static void C_EnterGameHandler(PacketSession session, IMessage packet)
	{
        C_EnterGame enterPacket = (C_EnterGame)packet;
        ClientSession clientSession = (ClientSession)session;

        GameRoom room = RoomManager.Instance.Find(enterPacket.RoomId);

        Player myPlayer = ObjectManager.Instance.Add<Player>();
        {
            clientSession.MyPlayer = myPlayer;
            myPlayer.Room = room;
            myPlayer.MoveInfo = new MoveInfo();
            myPlayer.MoveInfo.State = CreatureState.Idle;
            myPlayer.MoveInfo.DirX = 0;
            myPlayer.MoveInfo.DirZ = 0;
            myPlayer.MoveInfo.InputBit = 0;
            myPlayer.PosInfo.PosX = 0;
            myPlayer.PosInfo.PosY = 0;
            myPlayer.PosInfo.PosX = 0;
            myPlayer.Session = clientSession;
        }

        room.Push(room.EnterGame, myPlayer);
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
}
