using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Script.Services;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Web.Security;

namespace kingdee_wangshi
{
    public class utils
    {
        public static string randomStr = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";

        public static string load_data_from_url(string url, bool post, string param)
        {
            string jsonStr = null;
            WebClient client = new WebClient();

            if (post)
            {
                byte[] postData = Encoding.UTF8.GetBytes(param);
                client.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                client.Headers.Add("ContentLength", param.Length.ToString());

                try
                {
                    byte[] responseData = client.UploadData(url, "POST", postData);
                    jsonStr = Encoding.UTF8.GetString(responseData);
                }
                catch (WebException)
                {
                    goto leave;
                }
            }
            else
            {
                client.Encoding = System.Text.Encoding.GetEncoding("GB2312");

                try
                {
                    byte[] pageData = client.DownloadData(url);
                    jsonStr = Encoding.UTF8.GetString(pageData);
                }
                catch (Exception)
                {
                    goto leave;
                }
            }
        leave:
            return jsonStr;
        }

        public static string get_random_string()
        {
            char[] random = new char[16];
            char[] chars = randomStr.ToCharArray();

            int length = randomStr.Length;

            Random ran = new Random();

            for (int i = 0; i < 16; i++)
            {
                random[i] = chars[ran.Next(0, length)];
            }

            string retStr = new String(random);

            return retStr;
        }

        public static string get_timestamp()
        {
            string timestamp = null;

            DateTime oldTime = new DateTime(1970, 1, 1);
            TimeSpan span = DateTime.Now.Subtract(oldTime);
            int seconds = (int)span.TotalSeconds;
            timestamp = Convert.ToString(seconds);
            return timestamp;
        }

        public static int get_timestamp_to_second()
        {
            string timestamp = null;

            DateTime oldTime = new DateTime(1970, 1, 1);
            TimeSpan span = DateTime.Now.Subtract(oldTime);
            return (int)span.TotalSeconds;
        }

        public static int get_delta_second(DateTime oldTime)
        {
            TimeSpan span = DateTime.Now.Subtract(oldTime);
            return (int)span.TotalSeconds;
        }
    }

    public class WeChatAccessTokenEntity
    {
        public string Access_token { get; set; }
        public string Expires_in { get; set; }
    }

    public class AccessTokenEntiny
    {
        public string access_token;
        public int expires_in;
        public DateTime refresh_time;
    }

    public class WeChatJsapiTicketEntity
    {
        public string errcode { get; set; }
        public string errmsg { get; set; }
        public string ticket { get; set; }
        public string expires_in { get; set; }
    }

    public class JsapiTicketEntiny
    {
        public string jsapi_ticket;
        public int expires_in;
        public DateTime refresh_time;
    }

    public class WechatJsapiConfig
    {
        public string timestamp;
        public string nonceStr;
        public string signature;
        public string appId;
    }

    public class WeChatOauth2TokenEntity
    {
        public string Access_token { get; set; }
        public string Expires_in { get; set; }
        public string Refresh_token { get; set; }
        public string Openid { get; set; }
        public string Scope { get; set; }
    }

    public class WeChatUserEntity
    {
        public string Subscribe { get; set; }
        public string Openid { get; set; }
        public string Nickname { get; set; }
        public string Sex { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Country { get; set; }
        public string HeadImgUrl { get; set; }
        public string Subscribe_time { get; set; }
        public string Language { get; set; }
    }
}