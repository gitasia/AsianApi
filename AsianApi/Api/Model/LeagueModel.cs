using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi.Api.Model
{
    class LeagueModel
    {
        public long LeagueId { get; set; }
        public string LeagueName { get; set; }
        public int MarketTypeId { get; set; }
        public ulong Since { get; set; }
        // лист игр в лиге
        public List<Game> ListGames { get; set; }
    }
}
