#define DEBUG_WECHAT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

using System.Web.Script.Services;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Web.Security; 

namespace kingdee_wangshi
{
    public class WeChatAccessTokenEntity
    {
        public string Access_token { get; set; }
        public string Expires_in { get; set; }
    }

    public class AccessTokenEntiny
    {
        public string token;
        public int expires_timestamp;//Access_token的到期时间
        public int status;//0:normal,1:getting token,2:expired

        public AccessTokenEntiny()
        {
            this.token = null;
            this.expires_timestamp = 0;
            this.status = 0;
        }
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
        public string ticket;
        public int expires_timestamp;//JsapiTicket的到期时间
        public int status;//0:normal,1:getting token,2:expired

        public JsapiTicketEntiny()
        {
            this.ticket = null;
            this.expires_timestamp = 0;
            this.status = 0;
        }
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

    /// <summary>
    /// kingdee-wangshi 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://mp.imaxgine.net")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    // [System.Web.Script.Services.ScriptService]
    public class kingdee_wangshi : System.Web.Services.WebService
    {
        string appId = "wx42c72963b47ae521";
        string appSecret = "62b506607b45778b76762428755a4a3e";
        //string appId = "wx127e9b641dc9ff55";
        //string appSecret = "4a2e07920d2a8380b87904a6a2512ef3";
        //static string access_token = null;
        //static string jsapi_ticket = null;
        static int accessToken_expires_delta = 600;
        static int jsapiTicket_expires_delta = 600;
        //static AccessTokenEntiny accessTokenEntiny = null;
        //static JsapiTicketEntiny jsapiTicketEntiny = null;
        string get_accessToken_url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";
        string get_jsapi_ticket_url = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi";
        string oauth2_get_accessToken_url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code";
        string kingdee_wangshi_entry_url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri=http://mp.imaxgine.net/kingdee_wangshi.asmx/wechat_oauth2_cb&response_type=code&scope=snsapi_userinfo#wechat_redirect";
        string kingdee_wangshi_loading_page_url = "http://mp.imaxgine.net/kingdee_wangshi/app.html?openid={0}";
        string jsapi_signature = "jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}";
        string randomStr = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890";

        private string load_data_from_url(string url, bool post, string param)
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

        private string get_random_string()
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

        private string get_timestamp()
        {
            string timestamp = null;

            DateTime oldTime = new DateTime(1970, 1, 1);
            TimeSpan span = DateTime.Now.Subtract(oldTime);
            int seconds = (int)span.TotalSeconds;
            timestamp = Convert.ToString(seconds);
            return timestamp;
        }

        private int get_timestamp_to_second()
        {
            string timestamp = null;

            DateTime oldTime = new DateTime(1970, 1, 1);
            TimeSpan span = DateTime.Now.Subtract(oldTime);
            return (int)span.TotalSeconds;
        }

        private int set_local_accessToken(string token, int expires)
        {
            int errCode = 0;
            //int expires_timestamp = get_timestamp_to_second() + expires - accessToken_expires_delta;
            int expires_timestamp = get_timestamp_to_second() + 60;
            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

            string sqlStr = "select top 1 * from access_token_info order by id desc";
            SqlCommand token_selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter token_adapter = new SqlDataAdapter(token_selectCMD);
            DataSet ds = new DataSet();
            token_adapter.Fill(ds, "access_token_info");

            if (ds.Tables["access_token_info"].Rows.Count == 0)
            {
                DataRow newRow = ds.Tables["access_token_info"].NewRow();
                newRow["access_token"] = token;
                newRow["expire_timestamp"] = expires_timestamp;
                newRow["flag"] = 0;
                newRow["refresh_time"] = DateTime.Now;
                ds.Tables["access_token_info"].Rows.Add(newRow);

                SqlCommandBuilder token_scb = new SqlCommandBuilder(token_adapter);
                token_adapter.Update(ds.Tables["access_token_info"].GetChanges());
            }
            else
            {
                ds.Tables["access_token_info"].Rows[0]["flag"] = Convert.ToInt32(ds.Tables["access_token_info"].Rows[0]["flag"]) + 1;

                DataRow newRow = ds.Tables["access_token_info"].NewRow();
                newRow["access_token"] = token;
                newRow["expire_timestamp"] = expires_timestamp;
                newRow["flag"] = 0;
                newRow["refresh_time"] = DateTime.Now;
                ds.Tables["access_token_info"].Rows.Add(newRow);

                SqlCommandBuilder token_scb = new SqlCommandBuilder(token_adapter);
                token_adapter.Update(ds.Tables["access_token_info"].GetChanges());
            }

        leave:
            DBOperation.destroySqlConn(conn);
            return errCode;
        }

#if DEBUG_WECHAT
        [WebMethod]
        public AccessTokenEntiny get_local_accessToken()
#else
        private AccessTokenEntiny get_local_accessToken()
#endif
        {
            AccessTokenEntiny token = new AccessTokenEntiny();
            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

            string sqlStr = "select top 1 * from access_token_info order by id desc";
            SqlCommand token_selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter token_adapter = new SqlDataAdapter(token_selectCMD);
            DataSet ds = new DataSet();
            token_adapter.Fill(ds, "access_token_info");

            if (ds.Tables["access_token_info"].Rows.Count == 0)
                goto leave;
            else
            {
                token.token = ds.Tables["access_token_info"].Rows[0]["access_token"].ToString();
                token.expires_timestamp = Convert.ToInt32(ds.Tables["access_token_info"].Rows[0]["expire_timestamp"]);
                token.status = Convert.ToInt32(ds.Tables["access_token_info"].Rows[0]["flag"]);
            }

        leave:
            DBOperation.destroySqlConn(conn);
            return token;
        }

        private int set_local_jsapiTicket(string ticket, int expires)
        {
            int errCode = 0;
            //int expires_timestamp = get_timestamp_to_second() + expires - accessToken_expires_delta;
            int expires_timestamp = get_timestamp_to_second() + 60;
            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

            string sqlStr = "select top 1 * from jsapi_ticket_info order by id desc";
            SqlCommand ticket_selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter ticket_adapter = new SqlDataAdapter(ticket_selectCMD);
            DataSet ds = new DataSet();
            ticket_adapter.Fill(ds, "jsapi_ticket_info");

            if (ds.Tables["jsapi_ticket_info"].Rows.Count == 0)
            {
                DataRow newRow = ds.Tables["jsapi_ticket_info"].NewRow();
                newRow["jsapi_ticket"] = ticket;
                newRow["expire_timestamp"] = expires_timestamp;
                newRow["flag"] = 0;
                newRow["refresh_time"] = DateTime.Now;
                ds.Tables["jsapi_ticket_info"].Rows.Add(newRow);

                SqlCommandBuilder ticket_scb = new SqlCommandBuilder(ticket_adapter);
                ticket_adapter.Update(ds.Tables["jsapi_ticket_info"].GetChanges());
            }
            else
            {
                ds.Tables["jsapi_ticket_info"].Rows[0]["flag"] = Convert.ToInt32(ds.Tables["jsapi_ticket_info"].Rows[0]["flag"]) + 1;

                DataRow newRow = ds.Tables["jsapi_ticket_info"].NewRow();
                newRow["jsapi_ticket"] = ticket;
                newRow["expire_timestamp"] = expires_timestamp;
                newRow["flag"] = 0;
                newRow["refresh_time"] = DateTime.Now;
                ds.Tables["jsapi_ticket_info"].Rows.Add(newRow);

                SqlCommandBuilder ticket_scb = new SqlCommandBuilder(ticket_adapter);
                ticket_adapter.Update(ds.Tables["jsapi_ticket_info"].GetChanges());
            }

        leave:
            DBOperation.destroySqlConn(conn);
            return errCode;
        }

#if DEBUG_WECHAT
        [WebMethod]
        public JsapiTicketEntiny get_local_jsapiTicket()
#else
        private JsapiTicketEntiny get_local_jsapiTicket()
#endif
        {
            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

            JsapiTicketEntiny ticket = new JsapiTicketEntiny();

            string sqlStr = "select top 1 * from jsapi_ticket_info order by id desc";
            SqlCommand ticket_selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter ticket_adapter = new SqlDataAdapter(ticket_selectCMD);
            DataSet ds = new DataSet();
            ticket_adapter.Fill(ds, "jsapi_ticket_info");

            if (ds.Tables["jsapi_ticket_info"].Rows.Count == 0)
                goto leave;
            else
            {
                ticket.ticket = ds.Tables["jsapi_ticket_info"].Rows[0]["jsapi_ticket"].ToString();
                ticket.expires_timestamp = Convert.ToInt32(ds.Tables["jsapi_ticket_info"].Rows[0]["expire_timestamp"]);
                ticket.status = Convert.ToInt32(ds.Tables["jsapi_ticket_info"].Rows[0]["flag"]);
            }

        leave:
            DBOperation.destroySqlConn(conn);
            return ticket;
        }

        //table:0 for token, 1 for ticket
        private int set_local_flag(int table, string key_value, int flag)
        {
            int errCode = 0;
            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

            string table_str = null;
            string key_str = null;

            if (table == 0)
            {
                table_str = "access_token_info";
                key_str = "access_token";
            }
            else
            {
                table_str = "jsapi_ticket_info";
                key_str = "jsapi_ticket";
            }

            string sqlStr = "select * from " + table_str + " where " + key_str + " = '" + key_value + "'";
            SqlCommand selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(selectCMD);
            DataSet ds = new DataSet();
            adapter.Fill(ds, table_str);

            ds.Tables[table_str].Rows[0]["flag"] = flag;

            SqlCommandBuilder scb = new SqlCommandBuilder(adapter);
            adapter.Update(ds.Tables[table_str].GetChanges());

        leave:
            DBOperation.destroySqlConn(conn);
            return errCode;
        }

#if DEBUG_WECHAT
        [WebMethod]
        public string get_accessToken()
#else
        //此函数有同步的风险
        private string get_accessToken()
#endif
        {
            string token = null;
            string expires = null;

            AccessTokenEntiny accessTokenEntiny = get_local_accessToken();

            if (accessTokenEntiny.expires_timestamp <= get_timestamp_to_second())
            {
                if (accessTokenEntiny.status == 1)
                {
                    token = accessTokenEntiny.token;
                    goto leave;
                }
                //重要
                set_local_flag(0, accessTokenEntiny.token, 1);

                WeChatAccessTokenEntity myTokenEntity;
                JavaScriptSerializer jss = new JavaScriptSerializer();

                string accessTokenUrl = string.Format(get_accessToken_url, appId, appSecret);
                string accessTokenStr = load_data_from_url(accessTokenUrl, false, null);
                myTokenEntity = jss.Deserialize<WeChatAccessTokenEntity>(accessTokenStr);
                token = myTokenEntity.Access_token;
                expires = myTokenEntity.Expires_in;

                if (token == accessTokenEntiny.token)
                    set_local_flag(0, accessTokenEntiny.token, 0);
                else
                    set_local_accessToken(token, Convert.ToInt32(expires));
            }

            token = accessTokenEntiny.token;

        leave:
            return token;
        }

#if DEBUG_WECHAT
        [WebMethod]
        public string get_jsapiTicket()
#else
        //此函数有同步的风险
        private string get_jsapiTicket()
#endif
        {
            string ticket = null;
            string expires = null;
            string access_token = null;

            JsapiTicketEntiny jsapiTicketEntiny = get_local_jsapiTicket();

            if (jsapiTicketEntiny.expires_timestamp <= get_timestamp_to_second())
            {
                if (jsapiTicketEntiny.status == 1)
                {
                    ticket = jsapiTicketEntiny.ticket;
                    goto leave;
                }
                //重要
                set_local_flag(1, jsapiTicketEntiny.ticket, 1);

                WeChatJsapiTicketEntity myJsapiTokenEntity;
                JavaScriptSerializer jss = new JavaScriptSerializer();

                access_token = get_accessToken();
                if (access_token == null)
                    goto leave;
                string jsapiTicketUrl = string.Format(get_jsapi_ticket_url, access_token);
                string jsapiTicketStr = load_data_from_url(jsapiTicketUrl, false, null);
                myJsapiTokenEntity = jss.Deserialize<WeChatJsapiTicketEntity>(jsapiTicketStr);

                if (myJsapiTokenEntity.errcode != "0")
                {
                    set_local_flag(1, jsapiTicketEntiny.ticket, 0);
                    goto leave;
                }

                ticket = myJsapiTokenEntity.ticket;
                expires = myJsapiTokenEntity.expires_in;

                if (ticket == jsapiTicketEntiny.ticket)
                    set_local_flag(1, jsapiTicketEntiny.ticket, 0);
                else
                    set_local_jsapiTicket(ticket, Convert.ToInt32(expires));
            }

            ticket = jsapiTicketEntiny.ticket;

        leave:
            return ticket;
        }


        private WechatJsapiConfig get_jssdk_config(string url)
        {
            string jsapi_ticket = null;
            WechatJsapiConfig config = null;

            if ((jsapi_ticket = get_jsapiTicket()) == null)
                goto leave;

            config = new WechatJsapiConfig();
            config.nonceStr = get_random_string();
            config.timestamp = get_timestamp();
            config.appId = appId;

            string signature = string.Format(jsapi_signature, jsapi_ticket, config.nonceStr, config.timestamp, url);

            config.signature = FormsAuthentication.HashPasswordForStoringInConfigFile(signature, "SHA1");

        leave:
            return config;
        }


        [WebMethod]
        public void getjssdk(string openid)
        {
            int errCode = 0;
            string url = string.Format(kingdee_wangshi_loading_page_url, openid);

            WechatJsapiConfig ContextData = get_jssdk_config(url);
        leave:
            var tmpobj = new
            {
                errCode,
                ContextData,
            };
            Context.Response.Write(new JavaScriptSerializer().Serialize(tmpobj));
            return;
        }


        [WebMethod]
        [ScriptMethod(UseHttpGet = true)]
        public void wechatCheckSignature(string signature, string timestamp, string nonce, string echostr)
        {
            Context.Response.Write(echostr);
            Context.Response.End();
        }

        [WebMethod]
        public string HelloWorld()
        {
            Context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            return "Hello World";
        }

        [WebMethod]
        public void signin()
        {
            string url = null;

            url = string.Format(kingdee_wangshi_entry_url, appId);

            Context.Response.Redirect(url);
            Context.Response.End();
        }

        [WebMethod]
        public void wechat_oauth2_cb()
        {
            int retCode = 0;

            WeChatOauth2TokenEntity myTokenEntity;
            WeChatUserEntity user = new WeChatUserEntity();
            string oauth2AccessToken;
            string openId;
            string redirect_url;

            string code = Context.Request.QueryString["code"];

            if (code == null)
                return;

            JavaScriptSerializer jss = new JavaScriptSerializer();

            string accessTokenUrl = string.Format(oauth2_get_accessToken_url, appId, appSecret, code);
            string accessTokenStr = load_data_from_url(accessTokenUrl, false, null);
            myTokenEntity = jss.Deserialize<WeChatOauth2TokenEntity>(accessTokenStr);
            oauth2AccessToken = myTokenEntity.Access_token;
            openId = myTokenEntity.Openid;

            redirect_url = string.Format(kingdee_wangshi_loading_page_url, openId);

            Context.Response.Redirect(redirect_url);
            Context.Response.End();
        }

        [WebMethod]
        public void RecordPv(string openid, string page)
        {
            int errCode = 0;
            string errInfo = null;
            SqlConnection conn = null;

            conn = DBOperation.getSqlConn();

            string sqlStr = "select * from page_visit_info where id=1";
            SqlCommand visit_selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter visit_adapter = new SqlDataAdapter(visit_selectCMD);
            DataSet ds = new DataSet();
            visit_adapter.Fill(ds, "page_visit_info");

            DataRow newRow = ds.Tables["page_visit_info"].NewRow();
            newRow["openid"] = openid;
            newRow["page"] = page;
            newRow["visit_time"] = DateTime.Now;
            ds.Tables["page_visit_info"].Rows.Add(newRow);

            SqlCommandBuilder share_scb = new SqlCommandBuilder(visit_adapter);
            visit_adapter.Update(ds.Tables["page_visit_info"].GetChanges());
        leave:
            DBOperation.destroySqlConn(conn);
            var tmpobj = new
            {
                errCode,
                errInfo,
            };
            Context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            Context.Response.Write(new JavaScriptSerializer().Serialize(tmpobj));
            return;
        }

        [WebMethod]
        public void RecordButtonVisit(string openid, string buttonid)
        {
            int errCode = 0;
            string errInfo = null;
            SqlConnection conn = null;

            conn = DBOperation.getSqlConn();

            string sqlStr = "select * from button_visit_info where id=1";
            SqlCommand visit_selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter visit_adapter = new SqlDataAdapter(visit_selectCMD);
            DataSet ds = new DataSet();
            visit_adapter.Fill(ds, "button_visit_info");

            DataRow newRow = ds.Tables["button_visit_info"].NewRow();
            newRow["openid"] = openid;
            newRow["button"] = buttonid;
            newRow["visit_time"] = DateTime.Now;
            ds.Tables["button_visit_info"].Rows.Add(newRow);

            SqlCommandBuilder share_scb = new SqlCommandBuilder(visit_adapter);
            visit_adapter.Update(ds.Tables["button_visit_info"].GetChanges());
        leave:
            DBOperation.destroySqlConn(conn);
            var tmpobj = new
            {
                errCode,
                errInfo,
            };
            Context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            Context.Response.Write(new JavaScriptSerializer().Serialize(tmpobj));
            return;
        }

        [WebMethod]
        public void wechat_show_accessToken()
        {
            int errCode = 0;
            AccessTokenEntiny accessTokenEntiny = get_local_accessToken();
            string access_token = accessTokenEntiny.token;
            int expires_timestamp = accessTokenEntiny.expires_timestamp;
            int timestamp = get_timestamp_to_second();
            int flag = accessTokenEntiny.status;

            var tmpobj = new
            {
                errCode,
                access_token,
                expires_timestamp,
                flag,
                timestamp
            };
            Context.Response.Write(new JavaScriptSerializer().Serialize(tmpobj));
            return;
        }

        [WebMethod]
        public void wechat_show_jsapiTicket()
        {
            int errCode = 0;
            JsapiTicketEntiny jsapiTicketEntiny = get_local_jsapiTicket();
            string jsapi_ticket = jsapiTicketEntiny.ticket;
            int expires_timestamp = jsapiTicketEntiny.expires_timestamp;
            int timestamp = get_timestamp_to_second();
            int flag = jsapiTicketEntiny.status;

            var tmpobj = new
            {
                errCode,
                jsapi_ticket,
                expires_timestamp,
                flag,
                timestamp
            };
            Context.Response.Write(new JavaScriptSerializer().Serialize(tmpobj));
            return;
        }

        [WebMethod]
        public void kingdee_wangshi_init()
        {
            WeChatAccessTokenEntity myTokenEntity;
            JavaScriptSerializer jss = new JavaScriptSerializer();

            string accessTokenUrl = string.Format(get_accessToken_url, appId, appSecret);
            string accessTokenStr = load_data_from_url(accessTokenUrl, false, null);
            myTokenEntity = jss.Deserialize<WeChatAccessTokenEntity>(accessTokenStr);
            set_local_accessToken(myTokenEntity.Access_token, Convert.ToInt32(myTokenEntity.Expires_in));

            WeChatJsapiTicketEntity myJsapiTokenEntity;
            string jsapiTicketUrl = string.Format(get_jsapi_ticket_url, myTokenEntity.Access_token);
            string jsapiTicketStr = load_data_from_url(jsapiTicketUrl, false, null);
            myJsapiTokenEntity = jss.Deserialize<WeChatJsapiTicketEntity>(jsapiTicketStr);
            set_local_jsapiTicket(myJsapiTokenEntity.ticket, Convert.ToInt32(myJsapiTokenEntity.expires_in));
        }
    }
}
