using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;

class PacketManager
{
	#region Singleton
	static PacketManager _instance = new PacketManager();
	public static PacketManager Instance { get { return _instance; } }
	#endregion

	PacketManager()
	{
		Register();
	}

	Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>> _onRecv = new Dictionary<ushort, Action<PacketSession, ArraySegment<byte>, ushort>>();
	Dictionary<ushort, Action<PacketSession, IMessage>> _handler = new Dictionary<ushort, Action<PacketSession, IMessage>>();
		
	public Action<PacketSession, IMessage, ushort> CustomHandler { get; set; }

	public void Register()
	{		
		_onRecv.Add((ushort)MsgId.CMove, MakePacket<C_Move>);
		_handler.Add((ushort)MsgId.CMove, PacketHandler.C_MoveHandler);		
		_onRecv.Add((ushort)MsgId.CSync, MakePacket<C_Sync>);
		_handler.Add((ushort)MsgId.CSync, PacketHandler.C_SyncHandler);		
		_onRecv.Add((ushort)MsgId.CLogin, MakePacket<C_Login>);
		_handler.Add((ushort)MsgId.CLogin, PacketHandler.C_LoginHandler);		
		_onRecv.Add((ushort)MsgId.CStartScenario, MakePacket<C_StartScenario>);
		_handler.Add((ushort)MsgId.CStartScenario, PacketHandler.C_StartScenarioHandler);		
		_onRecv.Add((ushort)MsgId.CComplete, MakePacket<C_Complete>);
		_handler.Add((ushort)MsgId.CComplete, PacketHandler.C_CompleteHandler);		
		_onRecv.Add((ushort)MsgId.CTalk, MakePacket<C_Talk>);
		_handler.Add((ushort)MsgId.CTalk, PacketHandler.C_TalkHandler);		
		_onRecv.Add((ushort)MsgId.CEquip, MakePacket<C_Equip>);
		_handler.Add((ushort)MsgId.CEquip, PacketHandler.C_EquipHandler);		
		_onRecv.Add((ushort)MsgId.CUnEquip, MakePacket<C_UnEquip>);
		_handler.Add((ushort)MsgId.CUnEquip, PacketHandler.C_UnEquipHandler);		
		_onRecv.Add((ushort)MsgId.CRegistAccount, MakePacket<C_RegistAccount>);
		_handler.Add((ushort)MsgId.CRegistAccount, PacketHandler.C_RegistAccountHandler);		
		_onRecv.Add((ushort)MsgId.CEndGame, MakePacket<C_EndGame>);
		_handler.Add((ushort)MsgId.CEndGame, PacketHandler.C_EndGameHandler);		
		_onRecv.Add((ushort)MsgId.CRequestRoomList, MakePacket<C_RequestRoomList>);
		_handler.Add((ushort)MsgId.CRequestRoomList, PacketHandler.C_RequestRoomListHandler);		
		_onRecv.Add((ushort)MsgId.CLeaveLobby, MakePacket<C_LeaveLobby>);
		_handler.Add((ushort)MsgId.CLeaveLobby, PacketHandler.C_LeaveLobbyHandler);		
		_onRecv.Add((ushort)MsgId.CMakeRoom, MakePacket<C_MakeRoom>);
		_handler.Add((ushort)MsgId.CMakeRoom, PacketHandler.C_MakeRoomHandler);		
		_onRecv.Add((ushort)MsgId.CLeaveRoom, MakePacket<C_LeaveRoom>);
		_handler.Add((ushort)MsgId.CLeaveRoom, PacketHandler.C_LeaveRoomHandler);		
		_onRecv.Add((ushort)MsgId.CRequestRoomInfo, MakePacket<C_RequestRoomInfo>);
		_handler.Add((ushort)MsgId.CRequestRoomInfo, PacketHandler.C_RequestRoomInfoHandler);		
		_onRecv.Add((ushort)MsgId.CChangePosition, MakePacket<C_ChangePosition>);
		_handler.Add((ushort)MsgId.CChangePosition, PacketHandler.C_ChangePositionHandler);		
		_onRecv.Add((ushort)MsgId.CEnterRoom, MakePacket<C_EnterRoom>);
		_handler.Add((ushort)MsgId.CEnterRoom, PacketHandler.C_EnterRoomHandler);		
		_onRecv.Add((ushort)MsgId.CStartSimulation, MakePacket<C_StartSimulation>);
		_handler.Add((ushort)MsgId.CStartSimulation, PacketHandler.C_StartSimulationHandler);		
		_onRecv.Add((ushort)MsgId.CExitGame, MakePacket<C_ExitGame>);
		_handler.Add((ushort)MsgId.CExitGame, PacketHandler.C_ExitGameHandler);
	}

	public void OnRecvPacket(PacketSession session, ArraySegment<byte> buffer)
	{
		ushort count = 0;

		ushort size = BitConverter.ToUInt16(buffer.Array, buffer.Offset);
		count += 2;
		ushort id = BitConverter.ToUInt16(buffer.Array, buffer.Offset + count);
		count += 2;

		Action<PacketSession, ArraySegment<byte>, ushort> action = null;
		if (_onRecv.TryGetValue(id, out action))
			action.Invoke(session, buffer, id);
	}

	void MakePacket<T>(PacketSession session, ArraySegment<byte> buffer, ushort id) where T : IMessage, new()
	{
		T pkt = new T();
		pkt.MergeFrom(buffer.Array, buffer.Offset + 4, buffer.Count - 4);

		if (CustomHandler != null)
		{
			CustomHandler.Invoke(session, pkt, id);
		}
		else
		{
			Action<PacketSession, IMessage> action = null;
			if (_handler.TryGetValue(id, out action))
				action.Invoke(session, pkt);
		}
	}

	public Action<PacketSession, IMessage> GetPacketHandler(ushort id)
	{
		Action<PacketSession, IMessage> action = null;
		if (_handler.TryGetValue(id, out action))
			return action;
		return null;
	}
}