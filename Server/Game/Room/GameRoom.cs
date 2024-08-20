using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class GameRoom : JobSerializer
	{
		public int RoomId { get; set; }

		public int ScenarioProgress { get; set; } = 0;
		public string ScenarioName { get; set; }
		public int CompleteCount { get; set; } = 0;

		bool _doingScenario = false;

		Dictionary<int, Player> _players = new Dictionary<int, Player>();

		public void EnterGame(GameObject gameObject)
		{
			if (gameObject == null)
				return;

			GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.ObjectId);

			if (type == GameObjectType.Player)
			{
				Player player = gameObject as Player;
				_players.Add(gameObject.ObjectId, player);
				player.Room = this;


				// 본인한테 정보 전송
				{
					S_EnterGame enterPacket = new S_EnterGame();
					enterPacket.Player = new ObjectInfo();
					enterPacket.Player.ObjectId = player.Info.ObjectId;
					enterPacket.Player.MoveInfo = player.MoveInfo;
					enterPacket.Player.UserInfo = player.UserInfo;
					enterPacket.Player.PosInfo = player.PosInfo;
					player.Session.Send(enterPacket);

					S_Spawn spawnPacket = new S_Spawn();
					foreach (Player p in _players.Values)
					{
						if (player != p)
                        {
							ObjectInfo info = new ObjectInfo();
							info.ObjectId = p.ObjectId;
							info.UserInfo = p.UserInfo;
							info.MoveInfo = p.MoveInfo;
							info.PosInfo = p.PosInfo;
							spawnPacket.Objects.Add(info);
                        }
					}

					player.Session.Send(spawnPacket);
				}
			}
			
			// 타인한테 정보 전송
			{
				S_Spawn spawnPacket = new S_Spawn();
				ObjectInfo newObject = new ObjectInfo();
				newObject.ObjectId = gameObject.ObjectId;
				newObject.UserInfo = gameObject.UserInfo;
				newObject.MoveInfo = gameObject.MoveInfo;
				newObject.PosInfo = gameObject.PosInfo;
				spawnPacket.Objects.Add(newObject);
				foreach (Player p in _players.Values)
				{
					if (p.ObjectId != gameObject.ObjectId)
						p.Session.Send(spawnPacket);
				}
			}
		}

        public void LeaveGame(int objectId)
		{
			GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

			if (type == GameObjectType.Player)
			{
				Player player = null;
				if (_players.Remove(objectId, out player) == false)
					return;

				player.Room = null;

				// 본인한테 정보 전송
				{
					S_LeaveGame leavePacket = new S_LeaveGame();
					player.Session.Send(leavePacket);
				}
			}

			// 타인한테 정보 전송
			{
				S_Despawn despawnPacket = new S_Despawn();
				despawnPacket.ObjectIds.Add(objectId);
				foreach (Player p in _players.Values)
				{
					if (p.ObjectId != objectId)
						p.Session.Send(despawnPacket);
				}
			}
		}

		public void HandleEndVoice(Player player, C_EndVoice packet)
		{
			if (player == null)
				return;

			S_EndVoice voicePacket = new S_EndVoice();
			voicePacket.Id = player.ObjectId;

			Broadcast(voicePacket);
		}

		public void HandleVoice(Player player, C_Voice packet)
        {
			if (player == null)
				return;

			S_Voice voicePacket = new S_Voice();
			voicePacket.Id = player.ObjectId;
            voicePacket.VoiceClip = packet.VoiceClip;

			Broadcast(voicePacket, false, player);

            Console.WriteLine($"음성 패킷 크기 : {packet.VoiceClip.Length} bytes");
        }

        public void HandleEquip(Player player, C_Equip packet)
		{
			if (player == null)
				return;

			S_Equip equipPacket = new S_Equip();
			equipPacket.Id = player.ObjectId;
			equipPacket.ItemName = packet.ItemName;
			
			Broadcast(equipPacket);
		}

		public void HandleUnEquip(Player player, C_UnEquip packet)
        {
			if (player == null)
				return;

			S_UnEquip unEquipPacket = new S_UnEquip();
			unEquipPacket.Id = player.ObjectId;

			Broadcast(unEquipPacket);
		}

        public void HandleMove(Player player, C_Move movePacket)
		{
			if (player == null)
				return;

			S_Move newMovePacket = new S_Move();
			newMovePacket.ObjectId = player.Info.ObjectId;
			newMovePacket.MoveInfo = movePacket.MoveInfo;
			player.MoveInfo = movePacket.MoveInfo;
            Console.WriteLine($"{player.Info.ObjectId} 플레이어의 이동 동기화");

			Broadcast(newMovePacket);
		}

		public void HandleSync(Player player, C_Sync syncPacket)
        {
			if (player == null)
				return;

			S_Sync newSyncPacket = new S_Sync();
			newSyncPacket.ObjectId = player.Info.ObjectId;
			newSyncPacket.PosInfo = syncPacket.PosInfo;
			player.PosInfo = syncPacket.PosInfo;
            Console.WriteLine($"{player.Info.ObjectId} 플레이어의 위치 ({player.PosInfo.PosX}, {player.PosInfo.PosY}, {player.PosInfo.PosX}) 동기화");

			Broadcast(newSyncPacket);

        }

		public void HandleScenario(Player player, C_StartScenario packet)
        {
			if (player == null)
				return;

			if (_doingScenario == true)
				return;

			_doingScenario = true;

			this.ScenarioProgress = 0;
			this.ScenarioName = packet.ScenarioName;
			this.CompleteCount = 0;

			S_StartScenario startPacket = new S_StartScenario();
			startPacket.ScenarioName = packet.ScenarioName;

			Broadcast(startPacket);

            Console.WriteLine($"{RoomId}방에서 {ScenarioName} 훈련 시나리오 시작");
        }

		public void HandleComplete(Player player, C_Complete completePacket)
        {
			if (player == null)
				return;

			CompleteCount++;

			if(CompleteCount >= _players.Count)
            {
				S_NextProgress processPacket = new S_NextProgress();
				processPacket.Progress = ++ScenarioProgress;
				Broadcast(processPacket);
				CompleteCount = 0;
            }
        }

		public void HandleTalk(Player player, C_Talk talkPacket)
        {
			if (player == null)
				return;

			S_Talk sTalkPacket = new S_Talk();
			sTalkPacket.Id = player.ObjectId;
			sTalkPacket.Message = talkPacket.Message;
			Broadcast(sTalkPacket);
        }

		public Player FindPlayer(Func<GameObject, bool> condition)
		{
			foreach (Player player in _players.Values)
			{
				if (condition.Invoke(player))
					return player;
			}

			return null;
		}

		public void Broadcast(IMessage packet, bool includeSender = true, Player sender = null)
		{
			foreach (Player p in _players.Values)
			{
				if (includeSender == false && sender != null)
					if (p == sender)
						continue;

				p.Session.Send(packet);
			}
		}
	}
}
