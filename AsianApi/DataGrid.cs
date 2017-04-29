using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi
{
    public class MyStr
    {
        private string liganame;
        public string LigaName
        {
            get { return liganame; }
            set { liganame = value; }
        }
        public MyStr(string _liganame)
        {
            LigaName = _liganame;
        }
    }

    public class MyTable
    {
        public MyTable(string TIME, string EVENT, string FULL_TIME_1X2, string FULL_TIME_HDP_1, string FULL_TIME_HDP_2, string FULL_TIME_OU_1, string FULL_TIME_OU_2, string Bet_FULL_TIME_1X2, string Bet_FULL_1X2, string FIRST_HALF_1X2, string FIRST_HALF_HDP_1, string FIRST_HALF_HDP_2, string FIRST_HALF_OU_1, string FIRST_HALF_OU_2,
                                                  string Bet_FIRST_HALF_1X2, string Bet_FIRST_1X2, string LeagueId, string MathcId, string GameId, 
                                                  string DataGame, string IsActiv, string WillBeRemoved)
        {

            this.TIME = TIME;
            this.EVENT = EVENT;
            this.FULL_TIME_1X2 = FULL_TIME_1X2;
            this.FULL_TIME_HDP_1 = FULL_TIME_HDP_1;
            this.FULL_TIME_HDP_2 = FULL_TIME_HDP_2;
            this.FULL_TIME_OU_1 = FULL_TIME_OU_1;
            this.FULL_TIME_OU_2 = FULL_TIME_OU_2;

            this.Bet_FULL_TIME_1X2 = Bet_FULL_TIME_1X2;
            this.Bet_FULL_1X2 = Bet_FULL_1X2;

            this.FIRST_HALF_1X2 = FIRST_HALF_1X2;
            this.FIRST_HALF_HDP_1 = FIRST_HALF_HDP_1;
            this.FIRST_HALF_HDP_2 = FIRST_HALF_HDP_2;
            this.FIRST_HALF_OU_1 = FIRST_HALF_OU_1;
            this.FIRST_HALF_OU_2 = FIRST_HALF_OU_2;

            this.Bet_FIRST_HALF_1X2 = Bet_FIRST_HALF_1X2;
            this.Bet_FIRST_1X2 = Bet_FIRST_1X2;

   /*       this.Bet_FULL_TIME_HDP_1 = Bet_FULL_TIME_HDP_1;
            this.Bet_FULL_TIME_HDP_2 = Bet_FULL_TIME_HDP_2;
            this.Bet_FULL_TIME_OU_1 = Bet_FULL_TIME_OU_1;
            this.Bet_FULL_TIME_OU_2 = Bet_FULL_TIME_OU_2;
            
            this.Bet_FIRST_HALF_HDP_1 = Bet_FIRST_HALF_HDP_1;
            this.Bet_FIRST_HALF_HDP_2 = Bet_FIRST_HALF_HDP_2;
            this.Bet_FIRST_HALF_OU_1 = Bet_FIRST_HALF_OU_1;
            this.Bet_FIRST_HALF_OU_2 = Bet_FIRST_HALF_OU_2; */

            this.LeagueId = LeagueId;
            this.MathcId = MathcId;
            this.GameId = GameId;

            this.DataGame = DataGame;
            this.IsActiv = IsActiv;
            this.WillBeRemoved = WillBeRemoved;
        }


        public string TIME { get; set; }
        public string EVENT { get; set; }
        public string FULL_TIME_1X2 { get; set; }
        public string FULL_TIME_HDP_1 { get; set; }
        public string FULL_TIME_HDP_2 { get; set; }
        public string FULL_TIME_OU_1 { get; set; }
        public string FULL_TIME_OU_2 { get; set; }

        public string Bet_FULL_TIME_1X2 { get; set; }
        public string Bet_FULL_1X2 { get; set; }

        public string FIRST_HALF_1X2 { get; set; }
        public string FIRST_HALF_HDP_1 { get; set; }
        public string FIRST_HALF_HDP_2 { get; set; }
        public string FIRST_HALF_OU_1 { get; set; }
        public string FIRST_HALF_OU_2 { get; set; }

        
   /*     public string Bet_FULL_TIME_HDP_1 { get; set; }
        public string Bet_FULL_TIME_HDP_2 { get; set; }
        public string Bet_FULL_TIME_OU_1 { get; set; }
        public string Bet_FULL_TIME_OU_2 { get; set; } */

        public string Bet_FIRST_HALF_1X2 { get; set; }
        public string Bet_FIRST_1X2 { get; set; }

  /*      public string Bet_FIRST_HALF_HDP_1 { get; set; }
        public string Bet_FIRST_HALF_HDP_2 { get; set; }
        public string Bet_FIRST_HALF_OU_1 { get; set; }
        public string Bet_FIRST_HALF_OU_2 { get; set; } */
        public string LeagueId { get; set; }
        public string MathcId { get; set; }
        public string GameId { get; set; }

        public string DataGame { get; set; }
        public string IsActiv { get; set; }
        public string WillBeRemoved { get; set; }

        public override string ToString()
        {
            return $"{TIME},{EVENT},{FULL_TIME_1X2},{FULL_TIME_HDP_1},{FULL_TIME_HDP_2},{FULL_TIME_OU_1},{FULL_TIME_OU_2},{FIRST_HALF_1X2},{FIRST_HALF_HDP_1},{FIRST_HALF_HDP_2},{FIRST_HALF_OU_1},{FIRST_HALF_OU_2},{LeagueId},{MathcId},{GameId},{DataGame}, {IsActiv}, {WillBeRemoved}"; // ,{},{},{},{},{},{},{},{},{},{},{}
        }

    }


    public class limit
    {
        public limit(string Id, string v, string bet_column, string diff, string bet, bool betted)
        {
            this.Id = Id;
            this.v = v;
            this.bet_column = bet_column;
            this.diff = diff;
            this.bet = bet;
            this.betted = betted;
        }
        public string Id { get; set; }
        public string v { get; set; }
        public string bet_column { get; set; }
        public string diff { get; set; }
        public string bet { get; set; }
        public bool betted { get; set; }
    }

}
