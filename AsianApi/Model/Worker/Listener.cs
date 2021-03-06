﻿using AsianApi.Api;
using AsianApi.Api.Model;
using AsianApi.Model;
//using AsianApi.;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;


namespace AsianApi.Model.Worker
{
    public class Listener
    {
        protected AccountApi account;
        protected Thread process;
        private ApiAsian api;
        private List<LeagueModel> leaguesList; // лист лиг в которых идут игры (пока Live)
        private List<MyTable> result; // очищается полностью перед приемом новых данных

        private string Home_Team; // команда в матче, для фильтра
        private string game_minuts; // разбор времени матча

        private string path = @"Test.txt";
        
        public Listener (AccountApi accountApi)
        {
            account = accountApi;
            process = new Thread(runThread);
        }
        public void initThread()
        {
            process.Start();
        }


        public void runThread()
        {
            account.MarketTypeId = 0;
            account.sportsType = 1;
            api = new ApiAsian(account);
            leaguesList = new List<LeagueModel>();
            result = new List<MyTable>(); // приемник данных
            while (true)
            {
        //        Thread.Sleep(2000); //ждем 2 секунды
                if (!File.Exists(path)) // если файл есть , то не нужно скачивать с азии 
                {
                    Get_Ligas();
                    Get_Matches();
                }
            }
        }

        private void Get_Ligas()
        {
            try
            {
                // получить текущие лиги
                JToken leaguesJson = api.GetLeagues();
                List<JToken> leagues = ApiModel.Parse(leaguesJson, "Sports League");
                // записать их в свойство объекта LeaguesList
                foreach (JToken league in leagues)
                {
                    LeagueModel leagueModel = new LeagueModel();
                    leagueModel.LeagueId = (long)league.SelectToken("LeagueId");
                    leagueModel.LeagueName = (string)league.SelectToken("LeagueName");
                    leagueModel.MarketTypeId = (int)league.SelectToken("MarketTypeId");
                    leagueModel.Since = (ulong)league.SelectToken("Since");
                    leagueModel.ListGames = new List<Game>();

                    leaguesList.Add(leagueModel);
                }
                leaguesList.Sort((a, b) => a.LeagueName.CompareTo(b.LeagueName)); // сортировка
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " В реале пока лиг нет");
                var err = new BaseUp().ConBase();
                if (err == "")
                {
                    //   Base = new BaseUp().read_Base(Base, 3);
                    if (UCTable.Base != null)
                    {
                        err = new BaseUp().write_Base(UCTable.Base, UCTable.user_id, UCTable.credit);
                        if (err != "") MessageBox.Show(err);
                    }
                }
                else
                {
                    MessageBox.Show(err);
                }
                Environment.Exit(0);
                return;
            }
        }

        public void Get_Matches()
        {
            try
            {
                JToken feedsJson = api.GetFeeds();
                List<JToken> games = ApiModel.Parse(feedsJson, "Sports MatchGames"); // сбоит когда нет лиг
                Home_Team = "";
                result.Clear();
                foreach (JToken game in games)
                {
                    if (!(bool)game.SelectToken("IsActive"))   // Live ? 
                    {
       //                 continue;
                    }
                    if ((bool)game.SelectToken("WillBeRemoved")) // ?
                    {
      //                  continue;
                    }
                    var league = leaguesList.Find(x => x.LeagueId == (long)game.SelectToken("LeagueId"));
                    Game gameLine = null;

                    if (league != null && league.ListGames.Count > 0)
                    {
                        gameLine = league.ListGames.Find(x => x.MatchId == (long)game.SelectToken("MatchId"));
                    }
                    EventModel events = new EventModel();

                    if (gameLine == null)
                    {
                        gameLine = new Game();
                        Team awayTeam = new Team();
                        Team homeTeam = new Team();
                        gameLine.EventsList = new List<EventModel>();

                        ApiModel.setTeam(awayTeam, game, "AwayTeam");
                        ApiModel.setTeam(homeTeam, game, "HomeTeam");

                        gameLine.AwayTeam = awayTeam;
                        gameLine.HomeTeam = homeTeam;
                        gameLine.ExpectedLength = (int)game.SelectToken("ExpectedLength");      // пока как продолжительность игры
                        gameLine.Favoured = (int)game.SelectToken("Favoured");                  // где применяется? это указывается  фаворит типа. Красным красят что ли в таблице.
                        gameLine.InGameinutes = (int)game.SelectToken("InGameMinutes");         //  -  ?
                        gameLine.isActive = (bool)game.SelectToken("IsActive");                 //   проверяется в момент ставки
                        gameLine.isLive = (int)game.SelectToken("IsLive");
                        gameLine.LeagueId = (long)game.SelectToken("LeagueId");                 // -
                        gameLine.LeagueName = (string)game.SelectToken("LeagueName");
                        gameLine.MarketType = (string)game.SelectToken("MarketType");           // -
                        gameLine.MarketTypeId = (int)game.SelectToken("MarketTypeId");          // -
                        gameLine.MatchId = (long)game.SelectToken("MatchId");                   // -
                        gameLine.StartTime = (ulong)game.SelectToken("StartTime");              // -
                        gameLine.StartsOn = (string)game.SelectToken("StartsOn");               // -
                        gameLine.ToBeRemovedOn = (long)game.SelectToken("ToBeRemovedOn");       // -
                        gameLine.UpdatedDateTime = (ulong)game.SelectToken("UpdatedDateTime");  // -
                        gameLine.WillBeRemoved = (bool)game.SelectToken("WillBeRemoved");       // -

                    }
                    events.GameId = (long)game.SelectToken("GameId");
                    ApiModel.setEventModel(events, game);
                    gameLine.EventsList.Add(events);
                    if (league != null)
                        league.ListGames.Add(gameLine);
                }
                Home_Team = "";
                for (int i = 0; i < leaguesList.Count; i++)
                {
                    result.Add(new MyTable("", leaguesList[i].LeagueName, "", "", "", "", "", "", "", "", "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), "", "", "", "", ""));
                    int j = 0;
                    int k = 0;
                    leaguesList[i].ListGames.Sort((a, b) => a.HomeTeam.Name.CompareTo(b.HomeTeam.Name));
                    while (j < leaguesList[i].ListGames.Count)
                    {
                       DateTime date = new DateTime(1970, 1, 1).AddSeconds(leaguesList[i].ListGames[j].StartTime/1000);
                       DateTime date_t = DateTime.UtcNow;
                       var delta = date_t.Subtract(date);
                       game_minuts = delta.Hours.ToString() + ":" + delta.Minutes.ToString(); // on begin match 

                       DateTime date1 = new DateTime(1970, 1, 1).AddSeconds(leaguesList[i].ListGames[j].UpdatedDateTime/1000);
                       DateTime date_t1 = DateTime.UtcNow;
                       var delta1 = date_t.Subtract(date1);
                       string game_minuts1 = delta1.Hours.ToString() + ":" + delta1.Minutes.ToString(); // on begin update info of match
                                              
                       string game_minuts2 = " "; // кодировка времени игры
                       if (leaguesList[i].ListGames[j].InGameinutes > 60 && leaguesList[i].ListGames[j].InGameinutes < 105) game_minuts2 = "1H "+ (leaguesList[i].ListGames[j].InGameinutes-59).ToString()+"'";
                       if (leaguesList[i].ListGames[j].InGameinutes >= 1 && leaguesList[i].ListGames[j].InGameinutes < 15) game_minuts2 = "HT "; 
                       if (leaguesList[i].ListGames[j].InGameinutes == 60) game_minuts2 = "Live "; 
                       if (leaguesList[i].ListGames[j].InGameinutes >= 120 && leaguesList[i].ListGames[j].InGameinutes < 210) game_minuts2 = "2H " + (leaguesList[i].ListGames[j].InGameinutes - 119).ToString() + "'";

                       if (leaguesList[i].ListGames[j].HomeTeam.Name != Home_Team)
                        {
                            k = 0;
                            result.Add(new MyTable(leaguesList[i].ListGames[j].HomeTeam.Score.ToString() + ":" + leaguesList[i].ListGames[j].AwayTeam.Score.ToString(), leaguesList[i].ListGames[j].HomeTeam.Name, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.BookieOdds, 1, 0), "","", Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.BookieOdds, 1, 0), "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString(),date.ToString(), leaguesList[i].ListGames[j].isActive.ToString(), leaguesList[i].ListGames[j].WillBeRemoved.ToString())); //первая строка
                            result.Add(new MyTable(game_minuts2, leaguesList[i].ListGames[j].AwayTeam.Name, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.BookieOdds, 2, 0), "", "", Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.BookieOdds, 2, 0), "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString(),date.ToString(), leaguesList[i].ListGames[j].isActive.ToString(), leaguesList[i].ListGames[j].WillBeRemoved.ToString()));
                            result.Add(new MyTable("*", "Draw", WinX(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds), "", "", "", "", "", "", WinX(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds), "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString(),date.ToString(), leaguesList[i].ListGames[j].isActive.ToString(), leaguesList[i].ListGames[j].WillBeRemoved.ToString()));
                        }

                        else
                        {
                            result.Add(new MyTable(" ", " ", Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.BookieOdds, 1, 0), "", "", Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.BookieOdds, 1, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.BookieOdds, 1, 0), "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString(),date.ToString(), leaguesList[i].ListGames[j].isActive.ToString(), leaguesList[i].ListGames[j].WillBeRemoved.ToString())); //первая строка
                            result.Add(new MyTable(" ", " ", Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeHdp.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].FullTimeOu.BookieOdds, 2, 0), "", "", Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.Handicap, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeHdp.BookieOdds, 2, 0), leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.Goal, Win(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOu.BookieOdds, 2, 0), "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString(),date.ToString(), leaguesList[i].ListGames[j].isActive.ToString(), leaguesList[i].ListGames[j].WillBeRemoved.ToString()));
                            result.Add(new MyTable(" ", " ", WinX(leaguesList[i].ListGames[j].EventsList[k].FullTimeOneXTwo.BookieOdds), "", "", "", "", "", "", WinX(leaguesList[i].ListGames[j].EventsList[k].HalfTimeOneXTwo.BookieOdds), "", "", "", "", "", "", leaguesList[i].LeagueId.ToString(), leaguesList[i].ListGames[j].MatchId.ToString(), leaguesList[i].ListGames[j].EventsList[k].GameId.ToString(),date.ToString(), leaguesList[i].ListGames[j].isActive.ToString(), leaguesList[i].ListGames[j].WillBeRemoved.ToString()));
                        }
                        k++;
                        Home_Team = leaguesList[i].ListGames[j].HomeTeam.Name;
                        j++;
                    }
                }
                result.Add(new MyTable("", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "","","")); // пустая строка - как конец 
                leaguesList.Clear();
                string data = "";
                //          if (!File.Exists(path))   это проверяется ранее..но нужно для отладки
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        for (int fi = 0; fi < result.Count; fi++)
                        {
                            data = string.Join(",", result[fi]);
                            sw.WriteLine(data);
                        }
                    }
                }
            } catch (Exception e)
            {
              MessageBox.Show( e.Message + " В реале пока игр нет. ");
                var err = new BaseUp().ConBase();
                if (err == "")
                {
                    //   Base = new BaseUp().read_Base(Base, 3);
                    if (UCTable.Base != null)
                    {
                        err = new BaseUp().write_Base(UCTable.Base, UCTable.user_id, UCTable.credit);
                        if (err != "") MessageBox.Show(err);
                    }
                }
                else
                {
                    MessageBox.Show(err);
                }
                Environment.Exit(0);
                      return;
               // loginForm.loginForm_FormClosing(this, null);
            }
        }

        // разбор BookieOdds       
        private string Win(string str, int index1, int index2)
        {
            string retstr;
            if (str != "")
                try { retstr = str.Split(' ')[index1].Split(',')[index2]; }
                catch { retstr = ""; }
            else
                retstr = "";
            return retstr;
        }
        private string WinX(string str)
        {
            string retstr;
            if (str != "")
                try { retstr = str.Split(' ')[3]; }
                catch { retstr = ""; }
            else
                retstr = "";
            return retstr;
        }

        public void betting()
        {

        }

        private void Dashboard_Closing(object sender, CancelEventArgs e)
        {
            
            Environment.Exit(0);
            return;
        }
    }
}
