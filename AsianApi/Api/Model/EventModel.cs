using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi.Api.Model
{
    class EventModel
    {
        public long GameId { get; set; } // -
        public OneXTwo FullTimeOneXTwo { get; set; }
        public Ou FullTimeOu { get; set; }
        public Hdp FullTimeHdp { get; set; }
        public OneXTwo HalfTimeOneXTwo { get; set; }
        public Ou HalfTimeOu { get; set; }
        public Hdp HalfTimeHdp { get; set; }
    }
}
