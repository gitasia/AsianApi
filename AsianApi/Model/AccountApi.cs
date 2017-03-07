using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi.Model
{
   public class AccountApi
    {
        private static AccountApi accountApi = null;

        public static AccountApi Instance()
        {
            if (accountApi == null)
            {
                accountApi = new AccountApi();
            }

            return accountApi;
        }

        private AccountApi()
        {

        }

        public int Id { get; set; }
        public string Username { get; set; }
        public string AOToken { get; set; }
        public string Token { get; set; }
        public int MarketTypeId { get; set; }
        public int sportsType { get; set; }

    }
}
