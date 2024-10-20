using Server.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.DB
{
    public class DataModel
    {
        public class AccountDb
        {
            [Key]
            public string AccountId { get; set; }
            public string AccountPw { get; set; }

            //PlayerDb와 1:1 관계 설정
            public PlayerDb Player { get; set; }

            // ScoreDb와 1:N 관계 설정 (하나의 계정이 여러 점수를 가질 수 있음)
            public List<ScoreDb> Scores { get; set; } = new List<ScoreDb>();
        }

        public class PlayerDb
        {
            [Key]
            public string PlayerId { get; set; }
            public string PlayerName {  get; set; }

            // 외래 키 추가
            [ForeignKey("Account")]
            public string AccountId { get; set; }
            public AccountDb Account { get; set; }
        }

        public class ScoreDb
        {
            [Key]
            public int ScoreId { get; set; }
            public string Position {  get; set; }   // 수행한 직무
            public int FinalScore { get; set; }  // 최종 점수
            public int FaultCount { get; set; }  // 실수 횟수
            public DateTime GameDate { get; set; } // 게임이 완료된 날짜 및 시간

            // AccountDb와 1:N 관계 설정 (외래 키)
            [ForeignKey("Account")]
            public string AccountId { get; set; }
            public AccountDb Account { get; set; }
        }
    }
}
