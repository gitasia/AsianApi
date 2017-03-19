using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AsianApi.Api.Helper
{
    class Data
    {
        //  protected string siteUrl = "http://hospital.local/app_dev.php/api/";
           protected string siteUrl = "http://asianapi.serpentines.ru/app.php/api/";
     //   protected string siteUrl = "http://192.168.1.16/app.php/api/";
        public String getLoginUrl()
        {
            return  siteUrl + "login/";
            //return "https://webapi.asianodds88.com/AsianOddsService/Login";
        }

        public String getRegisterUrl()
        {
            return siteUrl + "loginAPI/";
           // return "https://webapi.asianodds88.com/AsianOddsService/Register";
        }

        public String getFeedsUrl()
        {
            return siteUrl + "getFeeds/";
        }

        public String getLeaguesUrl()
        {
            return siteUrl + "getLeagues/";
        }

        public String getMD5Hash(string text)
        {
            byte[] hash = Encoding.ASCII.GetBytes(text);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] hashenc = md5.ComputeHash(hash);
            string result = "";
            foreach (var b in hashenc)
            {
                result += b.ToString("x2");
            }
            return result;
        }

        public string getToken(string username, string password, string timestamp)
        {
            var hash = (new SHA1Managed()).ComputeHash(
                Encoding.UTF8.GetBytes(timestamp + username + timestamp + password));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public String getOutputType()
        {
            return "application/json";
        }

        public static IEnumerable<JToken> AllChildren(JToken json)
        {
            foreach (var c in json.Children())
            {
                yield return c;
                foreach (var cc in AllChildren(c))
                {
                    yield return cc;
                }
            }
        }
    }
}
