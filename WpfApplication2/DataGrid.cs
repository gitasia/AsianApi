using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication2
{
    public class MyTable
    {
        public MyTable(string TIME, string EVENT, string FULL_TIME_1X2, string FULL_TIME_HDP_1, string FULL_TIME_HDP_2, string FULL_TIME_OU_1, string FULL_TIME_OU_2, string FIRST_HALF_1X2, string FIRST_HALF_HDP_1, string FIRST_HALF_HDP_2, string FIRST_HALF_OU_1, string FIRST_HALF_OU_2)
        {
            
            this.TIME = TIME;
            this.EVENT = EVENT;
            this.FULL_TIME_1X2 = FULL_TIME_1X2;
            this.FULL_TIME_HDP_1 = FULL_TIME_HDP_1;
            this.FULL_TIME_HDP_2 = FULL_TIME_HDP_2;
            this.FULL_TIME_OU_1 = FULL_TIME_OU_1;
            this.FULL_TIME_OU_2 = FULL_TIME_OU_2;
            this.FIRST_HALF_1X2 = FIRST_HALF_1X2;
            this.FIRST_HALF_HDP_1 = FIRST_HALF_HDP_1;
            this.FIRST_HALF_HDP_2 = FIRST_HALF_HDP_2;
            this.FIRST_HALF_OU_1 = FIRST_HALF_OU_1;
            this.FIRST_HALF_OU_2 = FIRST_HALF_OU_2;
        }
        public string TIME { get; set; }
        public string EVENT { get; set; }
        public string FULL_TIME_1X2 { get; set; }
        public string FULL_TIME_HDP_1 { get; set; }
        public string FULL_TIME_HDP_2 { get; set; }
        public string FULL_TIME_OU_1 { get; set; }
        public string FULL_TIME_OU_2 { get; set; }
        public string FIRST_HALF_1X2 { get; set; }
        public string FIRST_HALF_HDP_1 { get; set; }
        public string FIRST_HALF_HDP_2 { get; set; }
        public string FIRST_HALF_OU_1 { get; set; }
        public string FIRST_HALF_OU_2 { get; set; }
    }
}
