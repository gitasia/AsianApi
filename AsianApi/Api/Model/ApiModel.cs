using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi.Api.Model
{
    class ApiModel
    {
        public static bool Valid(JToken response)
        {
            if ((int)response.SelectToken("Code") == -1)
            {
                return false;
            }
            return true;
        }

        // распарсить сложный json в нужный нам объект
        /**
         * JToken  resultJson  json объект которы пришел с сервера
         * 
         * String nodes строка с наименованиями узлов, разделенных пробелом по которым идет выборка
         */
        public static List<JToken> Parse(JToken resultJson, String nodes)
        {
            List<JToken> list = new List<JToken>();
            String[] nodesArray = nodes.Split(' ');
            var sportsObject = Api.Helper.Data.AllChildren(resultJson)
                .First(c => c.Type == JTokenType.Array && c.Path.Contains(nodesArray[0]))
                .Children<JObject>();
            foreach (JObject result in sportsObject)
            {
                foreach (JProperty property in result.Properties())
                {
                    if (property.Name == nodesArray[1])
                    {
                        var gamesJson = Api.Helper.Data.AllChildren(JObject.Parse(result.ToString()))
                          .First(c => c.Type == JTokenType.Array && c.Path.Contains(nodesArray[1]))
                          .Children<JObject>();

                        foreach (JObject games in gamesJson)
                        {
                            list.Add(games);
                        }
                    }
                }
            }
            return list;
        }

        public static void setTeam (Team team, JToken json,String node)
        {
            team.Name = (string)json.SelectToken(node).SelectToken("Name");
            team.Score = (int)json.SelectToken(node).SelectToken("Score");
            team.RedCards = (int)json.SelectToken(node).SelectToken("RedCards");
        }

        // заполняем события для игры
        public static void setEventModel(EventModel events, JToken json)
        {
            events.FullTimeOneXTwo = (OneXTwo)createEvent(json, "FullTimeOneXTwo");
            events.FullTimeHdp = (Hdp)createEvent(json, "FullTimeHdp");
            events.FullTimeOu = (Ou)createEvent(json, "FullTimeOu");
            events.HalfTimeHdp = (Hdp)createEvent(json, "HalfTimeHdp");
            events.HalfTimeOneXTwo = (OneXTwo)createEvent(json, "HalfTimeOneXTwo");
            events.HalfTimeOu = (Ou)createEvent(json, "HalfTimeOu");

        }

        // создаем события в зависиости от узла
        protected static Subevent createEvent(JToken json, String node)
        {
            Subevent result = null;
            
            if (node == "FullTimeHdp" || node == "HalfTimeHdp")
            {
                 result = new Hdp();
                 result.BookieOdds = (string)json.SelectToken(node).SelectToken("BookieOdds");
                 result.Handicap = (string)json.SelectToken(node).SelectToken("Handicap");
            }

            if (node == "FullTimeOu" || node == "HalfTimeOu")
            {
                 result = new Ou();
                 result.BookieOdds = (string)json.SelectToken(node).SelectToken("BookieOdds");
                 result.Goal = (string)json.SelectToken(node).SelectToken("Goal");
            }

            if (node == "FullTimeOneXTwo" || node == "HalfTimeOneXTwo")
            {
                result = new OneXTwo();
                result.BookieOdds = (string)json.SelectToken(node).SelectToken("BookieOdds");
            }

            return result;
        }
    }
}
