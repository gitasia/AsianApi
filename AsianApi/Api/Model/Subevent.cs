using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi.Api.Model
{
    abstract class Subevent
    {
        public virtual string BookieOdds { get; set; }
        public virtual string Handicap { get; set; }
        public virtual string Goal { get; set; }
    }
}
