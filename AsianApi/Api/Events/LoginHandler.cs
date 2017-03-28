using AsianApi.Api;
using AsianApi.Api.Helper;
using AsianApi.Api.Model;
using AsianApi.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi.Api.Events
{
    class LoginHandler
    {
        /**
         * Register user in system
         * 
         * @return void
         */
        public static void Register(AccountApi account)
        {
            var helper = new Data();
            IDictionary<string, string> data = new Dictionary<string, string>();
            IDictionary<string, string> head = new Dictionary<string, string>();

            data["username"] = account.Username;
            head["token"] = account.Token;
            head["accept"] = helper.getOutputType();

            JToken responseJSON = ApiAsian.sendRequest(data, head, helper.getRegisterUrl());

            if (!ApiModel.Valid(responseJSON))
            {
                throw new Exception((String)responseJSON.SelectToken("Result").SelectToken("TextMessage"));
            }

            account.AOToken = (String)responseJSON.SelectToken("Result").SelectToken("Token");
        }
    }
}
