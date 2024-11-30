using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Server.DB;
using Server.Game;
using ServerCore;
using System;
using System.Linq;
using System.Security.Principal;
using static Server.DB.DataModel;

namespace Server
{
    public partial class ClientSession : PacketSession
    {
        public string AccountDbId { get; set; }
        public string AccountDbName { get; set; }

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

                    Console.WriteLine($"신규 사용자 등록 : {accountId}");
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

                    // 이미 로그인 중인 Id일 경우
                    if (SessionManager.Instance.CheckUsingId(account.Id))
                    {
                        loginPacket.Result = LoginState.AlreadyLogin;
                        Send(loginPacket);
                        return;
                    }

                    // 로그인 성공
                    AccountDbId = account.Id; // 세션에 계정 DB ID 저장
                    AccountDbName = account.Name; // 세션에 계정 DB Name 저장
                    ServerState = PlayerServerState.ServerStateLobby; // 로비 상태로 변경

                    loginPacket.Result = LoginState.LoginComplete;
                    Send(loginPacket);

                    Program.Lobby.EnterLobby(this);

                    Console.WriteLine($"사용자 로비 입장 : {AccountDbId}");
                    return;
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
