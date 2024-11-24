using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Lobby
{
    public class Lobby : JobSerializer
    {
        List<ClientSession> Sessions = new List<ClientSession>();
        public Dictionary<string, Room> Rooms = new Dictionary<string, Room>();
        public Dictionary<ClientSession, Room> SessionsInRoom = new Dictionary<ClientSession, Room>();

        public override void Update()
        {
            base.Update();
        }

        public void EnterLobby(ClientSession session)
        {
            Sessions.Add(session);
            session.ServerState = PlayerServerState.ServerStateLobby;
        }

        public void LeaveLobby(ClientSession session, C_LeaveLobby packet)
        {
            if (session.ServerState != PlayerServerState.ServerStateLobby)
                return;

            if (Sessions.Contains(session))
            {
                switch (packet.TargetScene)
                {
                    case Scene.LoginScene:
                        session.ServerState = PlayerServerState.ServerStateLogin;
                        break;
                    case Scene.RoomScene:
                        session.ServerState = PlayerServerState.ServerStateRoom;
                        break;
                    default:
                        return;
                }

                Sessions.Remove(session);
            }
        }

        public void Disconnect(ClientSession session)
        {
            if (session == null)
                return;

            if (SessionsInRoom.TryGetValue(session, out var room))
            {
                room.LeaveRoom(session);
            }

            if (Sessions.Contains(session))
                Sessions.Remove(session);
        }

        public void SendRoomList(ClientSession session)
        {
            if (session == null)
                return;

            if (session.ServerState != PlayerServerState.ServerStateLobby)
                return;

            S_RoomList roomPacket = new S_RoomList();
            foreach (var room in Rooms.Values)
            {
                roomPacket.Rooms.Add(room.RoomInfo);
            }

            session.Send(roomPacket);
        }

        void Broadcast(IMessage packet, ClientSession session = null)
        {
            if (session != null)
            {
                if (session.ServerState != PlayerServerState.ServerStateLobby)
                    return;

                session.Send(packet);
                return;
            }

            if (Sessions.Count <= 0)
                return;

            foreach (var s in Sessions)
            {
                s.Send(packet);
            }
        }

        #region ----------------Room 관련----------------

        public void EnterRoom(ClientSession session, C_EnterRoom packet)
        {
            if (session == null)
                return;

            if (!Rooms.TryGetValue(packet.Title, out var room))
            {
                S_EnterRoom enterPacket = new S_EnterRoom();
                enterPacket.Result = EnterRoomState.NoRoom;
                session.Send(enterPacket);
                return;
            }
            
            room.EnterRoom(session, packet);
        }

        /// <summary>
        /// 방 만들 때 호출
        /// </summary>
        /// <param name="clientSession"></param>
        /// <param name="packet"></param>
        public void MakeRoom(ClientSession clientSession, C_MakeRoom packet)
        {
            if (clientSession == null)
                return;

            S_MakeRoom serverPacket = new S_MakeRoom();

            try
            {
                if (Rooms.ContainsKey(packet.Title))
                {
                    serverPacket.Result = MakeRoomState.ExistTitle;
                    clientSession.Send(serverPacket);
                    return;
                }

                RoomInfo roomInfo = new RoomInfo();
                roomInfo.Title = packet.Title;
                roomInfo.Maker = clientSession.AccountDbId;
                roomInfo.Disease = packet.Disease;
                roomInfo.CurMembers = 0;
                roomInfo.Password = packet.Password;

                Room room = new Room(roomInfo);
                Rooms.Add(roomInfo.Title, room);
                SessionsInRoom.Add(clientSession, room);

                room.sessions[0] = clientSession;
                room.Maker = clientSession;
                room.Lobby = this;

                serverPacket.Result = MakeRoomState.MakeRoomComplete;
                clientSession.Send(serverPacket);
                clientSession.ServerState = PlayerServerState.ServerStateRoom;
                return;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                serverPacket.Result = MakeRoomState.MakeRoomError;
                clientSession.Send(serverPacket);
                return;
            }
        }

        public void SendRoomInfo(ClientSession session)
        {
            if (session == null)
                return;

            if (!SessionsInRoom.TryGetValue(session, out Room room))
                return;

            room.UpdateInfo();
        }

        public void LeaveRoom(ClientSession session, C_LeaveRoom packet)
        {
            if (session == null)
                return;

            if (packet.TargetScene != Scene.LobbyScene)
                return;

            if (!SessionsInRoom.TryGetValue(session, out Room room))
                return;

            room.LeaveRoom(session);
        }

        public void RoomChangePosition(ClientSession session, C_ChangePosition packet)
        {
            if (session == null)
                return;

            if (!SessionsInRoom.TryGetValue(session, out var room))
                return;

            room.ChangePosition(session, packet);
        }

        public void StartSimulation(ClientSession session)
        {
            if (session == null)
                return;

            if (!SessionsInRoom.TryGetValue(session, out var room))
                return;

            room.StartSimulation(session);
        }

        #endregion
    }

    public class Room
    {
        public RoomInfo RoomInfo { get; set; }
        public ClientSession[] sessions = new ClientSession[6];
        public ClientSession Maker { get; set; }
        public Lobby Lobby { get; set; }

        public Room(RoomInfo info)
        {
            RoomInfo = info;

            for (int i = 0; i < sessions.Length; i++)
            {
                sessions[i] = null;
            }
        }

        public void EnterRoom(ClientSession session, C_EnterRoom packet)
        {
            if (session == null)
                return;

            S_EnterRoom enterPacket = new S_EnterRoom();

            try
            {
                if (!string.IsNullOrEmpty(RoomInfo.Password))
                {
                    if (RoomInfo.Password != packet.Password)
                    {
                        enterPacket.Result = EnterRoomState.IncorrectPassword;
                        session.Send(enterPacket);
                        return;
                    }
                }

                if (RoomInfo.CurMembers >= 6)
                {
                    enterPacket.Result = EnterRoomState.FullMembers;
                    session.Send(enterPacket);
                    return;
                }

                for(int i = 0; i < sessions.Length; i++)
                {
                    if (sessions[i] == null)
                    {
                        sessions[i] = session;
                        break;
                    }    
                }

                session.ServerState = PlayerServerState.ServerStateRoom;
                Lobby.SessionsInRoom.Add(session, this);
                enterPacket.Result = EnterRoomState.EnterRoomComplete;
                session.Send(enterPacket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                enterPacket.Result = EnterRoomState.EnterError;
                session.Send(enterPacket);
                return;
            }

            UpdateInfo();
        }

        public void LeaveRoom(ClientSession session)
        {
            if (session == null)
                return;

            if (session.ServerState != PlayerServerState.ServerStateRoom)
                return;

            if (session == Maker)
            {
                S_MakerExit exitPacket = new S_MakerExit();
                Broadcast(exitPacket, session);
                
                if(sessions.Length > 0)
                {
                    foreach (var s in sessions)
                    {
                        if(s != null)
                        {
                            if(Lobby.SessionsInRoom.ContainsKey(s))
                                Lobby.SessionsInRoom.Remove(s);
                            s.ServerState = PlayerServerState.ServerStateLobby;
                        }
                    }
                }
                
                if(Lobby.Rooms.ContainsKey(RoomInfo.Title))
                    Lobby.Rooms.Remove(RoomInfo.Title);

                return;
            }

            for (int i = 0; i < sessions.Length; i++)
            {
                if (sessions[i] == session)
                {
                    sessions[i] = null;
                    break;
                }
            }

            if (Lobby.SessionsInRoom.ContainsKey(session))
                Lobby.SessionsInRoom.Remove(session);

            session.ServerState = PlayerServerState.ServerStateLobby;

            UpdateInfo();
        }

        public void ChangePosition(ClientSession session, C_ChangePosition packet)
        {
            if (session == null)
                return;

            if (session.ServerState != PlayerServerState.ServerStateRoom)
                return;

            int changeNumber = packet.Position;

            if (sessions[changeNumber] != null)
                return;

            for (int i = 0; i < sessions.Length; i++)
            {
                if (sessions[i] == session)
                {
                    sessions[i] = null;
                    break;
                }
            }

            sessions[changeNumber] = session;

            UpdateInfo();
        }

        public void UpdateInfo()
        {
            S_UpdateRoomInfo updatePacket = new S_UpdateRoomInfo();
            List<ClientSession> targetSessions = new List<ClientSession>();

            for (int i = 0; i < sessions.Length; i++)
            {
                if (sessions[i] != null)
                {
                    updatePacket.Players.Add(new PositionMatching() { Index = i, PlayerId = sessions[i].AccountDbId });
                    targetSessions.Add(sessions[i]);
                }
            }

            Broadcast(updatePacket);
        }

        void Broadcast(IMessage packet, ClientSession except = null)
        {
            if (sessions.Length <= 0)
                return;

            foreach (var session in sessions)
            {
                if(session != null)
                {
                    if (except != null && session == except)
                        continue;

                    session.Send(packet);
                }
            }
        }

        public void StartSimulation(ClientSession session)
        {
            GameRoom room = RoomManager.Instance.Add();
            room.ScenarioName = RoomInfo.Disease;

            for(int i = 0; i < sessions.Length; i++)
            {
                if (sessions[i] != null)
                {
                    Player myPlayer = ObjectManager.Instance.Add<Player>();
                    {
                        sessions[i].MyPlayer = myPlayer;
                        myPlayer.Room = room;
                        myPlayer.MoveInfo = new MoveInfo();
                        myPlayer.MoveInfo.State = CreatureState.Idle;
                        myPlayer.MoveInfo.DirX = 0;
                        myPlayer.MoveInfo.DirZ = 0;
                        myPlayer.MoveInfo.InputBit = 0;
                        myPlayer.PosInfo.PosX = 0;
                        myPlayer.PosInfo.PosY = 0;
                        myPlayer.PosInfo.PosX = 0;
                        myPlayer.Session = sessions[i];
                        myPlayer.Position = DataManager.Positions[i];
                    }

                    room.Push(room.EnterGame, myPlayer);
                }
            }

            if (sessions.Length > 0)
            {
                foreach (var s in sessions)
                {
                    if (s != null)
                    {
                        if (Lobby.SessionsInRoom.ContainsKey(s))
                            Lobby.SessionsInRoom.Remove(s);

                        s.ServerState = PlayerServerState.ServerStateGame;
                    }
                }
            }

            if (Lobby.Rooms.ContainsKey(RoomInfo.Title))
                Lobby.Rooms.Remove(RoomInfo.Title);
        }
    }
}