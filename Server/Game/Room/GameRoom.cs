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

		public bool DoingScenario = false;

		public Dictionary<int, Player> Players = new Dictionary<int, Player>();

        public DateTime EndTime { get; set; }
		int _scenarioSyncCounter = 0;

        public override void Update()
        {
            base.Update();

			if(_scenarioSyncCounter >= 20)
			{
                S_NextProgress processPacket = new S_NextProgress();
                processPacket.Progress = ScenarioProgress;
                Broadcast(processPacket);
				_scenarioSyncCounter = 0;
            }
			else
			{
				_scenarioSyncCounter++;
			}
        }

        public void EnterGame(GameObject gameObject)
		{
			if (gameObject == null)
				return;

			GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.ObjectId);

			if (type == GameObjectType.Player)
			{
				Player player = gameObject as Player;
				Players.Add(gameObject.ObjectId, player);
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
					foreach (Player p in Players.Values)
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
				foreach (Player p in Players.Values)
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
				if (Players.Remove(objectId, out player) == false)
					return;

				player.Room = null;
				player.Session.ServerState = PlayerServerState.ServerStateLogin;

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
				foreach (Player p in Players.Values)
				{
					if (p.ObjectId != objectId)
						p.Session.Send(despawnPacket);
				}
			}

			if(Players.Count == 0)
            {
                ScenarioProgress = 0;
                ScenarioName = null;
                CompleteCount = 0;
                DoingScenario = false;
                EndTime = DateTime.MinValue;
            }
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
			unEquipPacket.ItemName = packet.ItemName;

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

			Broadcast(newSyncPacket);

        }

		public void HandleScenario(Player player, C_StartScenario packet)
        {
			if (player == null)
				return;

			if (DoingScenario == true)
				return;

			DoingScenario = true;

			this.ScenarioProgress = 0;
			this.ScenarioName = packet.ScenarioName;
			this.CompleteCount = 0;

			List<string> lackPositions = new List<string>(DataManager.Positions);
			foreach(var p in Players.Values)
			{
                if (lackPositions.Contains(p.Position))
				{
					lackPositions.Remove(p.Position);
				}
            }

            S_StartScenario startPacket = new S_StartScenario();
            startPacket.ScenarioName = packet.ScenarioName;
			startPacket.LackPositions.AddRange(lackPositions);

            Broadcast(startPacket);

            Console.WriteLine($"{RoomId}방에서 {ScenarioName} 훈련 시나리오 시작");
        }

		public void HandleComplete(Player player, C_Complete completePacket)
        {
			if (player == null)
				return;

			CompleteCount++;

			if(CompleteCount >= Players.Count)
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
			sTalkPacket.TTSSelf = talkPacket.TTSSelf;
			Broadcast(sTalkPacket);
        }

        public void HandleEndGame(Player player, C_EndGame game)
        {
            if (player == null)
                return;

			if(EndTime == DateTime.MinValue)
			{
				EndTime = DateTime.Now;
			}

			player.Session.RegisterScore(player.Session.AccountDbId, game.Position, game.FinalScore, EndTime);
			//player에게 해당 Position의 게임기록들 전송, player의 ScoreDbId 따로 전송
			player.Session.SendScores(player.Session.AccountDbId, game.Position);

			LeaveGame(player.ObjectId);
        }

        public Player FindPlayer(Func<GameObject, bool> condition)
		{
			foreach (Player player in Players.Values)
			{
				if (condition.Invoke(player))
					return player;
			}

			return null;
		}

		public void Broadcast(IMessage packet, bool includeSender = true, Player sender = null)
		{
			foreach (Player p in Players.Values)
			{
				if (includeSender == false && sender != null)
					if (p == sender)
						continue;

				p.Session.Send(packet);
			}
		}
    }
}
