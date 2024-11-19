using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Linq;
using static Server.DB.DataModel;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public string AccountDbId { get; private set; }

        // 계정 생성 및 플레이어 등록 처리
        public void HandleRegistAccount(string accountId, string accountPw, string userName)
        {
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            S_RegistAccount registPacket = new S_RegistAccount();

            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    // 이미 동일한 Id가 있는지 확인
                    var existingAccount = db.Accounts
                        .FirstOrDefault(a => a.Id == accountId);

                    // 이미 등록된 사용자일 경우
                    if (existingAccount != null)
                    {
                        // 이미 등록된 사용자라고 패킷 보내기
                        registPacket.Result = RegistAccountState.ExistAccount;
                        Send(registPacket);
                        return;
                    }

                    // 새로운 계정과 플레이어 생성
                    var newAccount = new AccountDb()
                    {
                        Id = accountId,
                        Pw = accountPw,
                        Name = userName
                    };

                    db.Accounts.Add(newAccount);
                    registPacket.Result = RegistAccountState.RegistComplete;
                    db.SaveChangesEx();
                    // 계정이 생성되었다고 패킷 보내기
                    Send(registPacket);
                }
            }
            catch (Exception ex)
            {
                // 오류가 발생했다고 패킷 보내기
                registPacket.Result = RegistAccountState.RegistError;
                Send(registPacket);

                Console.WriteLine($"계정 등록 실패 : {ex.Message}");
            }
        }

        //로그인 처리
        public void HandleLogin(string accountId, string accountPw, string position)
        {
            if (ServerState != PlayerServerState.ServerStateLogin)
                return;

            S_Login loginPacket = new S_Login();

            try
            {
                using (AppDbContext db = new AppDbContext())
                {
                    // AccountId에 해당하는 계정을 찾기
                    var account = db.Accounts
                        .FirstOrDefault(a => a.Id == accountId);

                    // 계정이 존재하지 않는 경우
                    if (account == null)
                    {
                        loginPacket.Result = LoginState.NoAccount;
                        Send(loginPacket);
                        return;
                    }

                    // 비밀번호가 틀린 경우
                    if (account.Pw != accountPw)
                    {
                        loginPacket.Result = LoginState.WrongPassword;
                        Send(loginPacket);
                        return;
                    }

                    // 로그인 성공
                    AccountDbId = account.Id; // 세션에 계정 DB ID 저장
                    ServerState = PlayerServerState.ServerStateGame; // 상태를 게임 플레이 상태로 전환

                    GameRoom room = RoomManager.Instance.Find(1);

                    if (room.DoingScenario)
                    {
                        loginPacket.Result = LoginState.AlreadyStart;
                        Send(loginPacket);
                        return;
                    }

                    Player myPlayer = ObjectManager.Instance.Add<Player>();
                    {
                        MyPlayer = myPlayer;
                        myPlayer.Room = room;
                        myPlayer.MoveInfo = new MoveInfo();
                        myPlayer.MoveInfo.State = CreatureState.Idle;
                        myPlayer.MoveInfo.DirX = 0;
                        myPlayer.MoveInfo.DirZ = 0;
                        myPlayer.MoveInfo.InputBit = 0;
                        myPlayer.PosInfo.PosX = 0;
                        myPlayer.PosInfo.PosY = 0;
                        myPlayer.PosInfo.PosX = 0;
                        myPlayer.UserInfo.Position = position;
                        myPlayer.UserInfo.AccountId = accountId;
                        myPlayer.Session = this;
                    }

                    room.Push(room.EnterGame, myPlayer);
                }
            }
            catch(Exception ex)
            {
                loginPacket.Result = LoginState.LoginError;
                Send(loginPacket);

                Console.WriteLine($"로그인 실패 : {ex.Message}");
            }
        }
    }
}
