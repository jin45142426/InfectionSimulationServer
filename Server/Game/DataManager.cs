using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Game
{
    public class DataManager
    {
        public static Dictionary<string, List<ScenarioInfo>> ScenarioDict { get; set; } = new Dictionary<string, List<ScenarioInfo>>();

        public static readonly List<string> Positions = new List<string>()
        {
            "응급센터 간호사1",
            "응급센터 간호사2",
            "응급의학과 의사",
            "감염관리팀 간호사",
            "영상의학팀 방사선사",
            "감염병대응센터 주무관"
        };

        public static void LoadData()
        {
            ScenarioDict = LoadJson<ScenarioInfo>("엠폭스");
            foreach(var scenario in ScenarioDict.Values)
            {
                foreach(var info in scenario)
                {
                    Console.WriteLine($"{info.Situation} / {info.Actor} / {info.Action} / {info.Script} / {info.Keywords}");
                }
            }
        }

        static Dictionary<string, List<T>> LoadJson<T>(string path)
        {
            string text = File.ReadAllText($"../../../{path}.json");
            Dictionary<string, List<T>> dict = JsonConvert.DeserializeObject<Dictionary<string, List<T>>>(text);
            return dict;
        }
    }
}
