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
