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
        string appId = "wxd73b44e3381aa8fd";
        string appSecret = "88ac9fe5320f96b69383a246b70dd1a6";
        //string appId = "wx42c72963b47ae521";
        //string appSecret = "62b506607b45778b76762428755a4a3e";
        //string appId = "wx127e9b641dc9ff55";
        //string appSecret = "4a2e07920d2a8380b87904a6a2512ef3";
        //static string access_token = null;
        //static string jsapi_ticket = null;
        //static AccessTokenEntiny accessTokenEntiny = null;
        //static JsapiTicketEntiny jsapiTicketEntiny = null;
        string get_accessToken_url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";
        string get_jsapi_ticket_url = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi";
        string oauth2_get_accessToken_url = "https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code";
        string kingdee_wangshi_entry_url = "https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri=http://mp.imaxgine.net/kingdee_wangshi.asmx/wechat_oauth2_cb&response_type=code&scope=snsapi_userinfo#wechat_redirect";
        string kingdee_wangshi_loading_page_url = "http://mp.imaxgine.net/kingdee_wangshi/app.html?openid={0}";
        string jsapi_signature = "jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}";
        
        private WechatJsapiConfig get_jssdk_config(string url)
        {
            JsapiTicketEntiny jsapi_ticket = null;
            WechatJsapiConfig config = null;

            /*if ((jsapi_ticket = DBOperation.get_local_ticket_entiny()) == null)
                goto leave;*/
            if ((jsapi_ticket = DBOperation.get_remote_ticket_entiny()) == null)
                goto leave;

            config = new WechatJsapiConfig();
            config.nonceStr = utils.get_random_string();
            config.timestamp = utils.get_timestamp();
            config.appId = appId;

            string signature = string.Format(jsapi_signature, jsapi_ticket.jsapi_ticket, config.nonceStr, config.timestamp, url);

            config.signature = FormsAuthentication.HashPasswordForStoringInConfigFile(signature, "SHA1");

        leave:
            return config;
        }

        /*
        [WebMethod]
        public void tokenTimer()
        {
            string token = null;
            string expires = null;

            AccessTokenEntiny entiny = DBOperation.get_local_token_entiny();
            if (entiny == null)
                return;
            else
            {
                int delta = utils.get_delta_second(entiny.refresh_time);
                string refresh_time = entiny.refresh_time.ToString("s");
                string now_time = DateTime.Now.ToString("s");
                var tmpobj = new
                {
                    delta,
                    refresh_time,
                    now_time,
                };
                Context.Response.Write(new JavaScriptSerializer().Serialize(tmpobj));
                if (delta <= 1200000)
                    return;
            }

            WeChatAccessTokenEntity myTokenEntity;
            JavaScriptSerializer jss = new JavaScriptSerializer();

            string accessTokenUrl = string.Format(get_accessToken_url, appId, appSecret);
            string accessTokenStr = utils.load_data_from_url(accessTokenUrl, false, null);
            myTokenEntity = jss.Deserialize<WeChatAccessTokenEntity>(accessTokenStr);
            token = myTokenEntity.Access_token;
            expires = myTokenEntity.Expires_in;

            DBOperation.set_local_token_ticket(0, token, Convert.ToInt32(expires));

            return;
        }

        
        [WebMethod]
        public void ticketTimer()
        {
            string ticket = null;
            string expires = null;
            AccessTokenEntiny access_token = null;

            JsapiTicketEntiny entiny = DBOperation.get_local_ticket_entiny();
            if (entiny == null)
                return;
            else
            {
                int delta = utils.get_delta_second(entiny.refresh_time);
                string refresh_time = entiny.refresh_time.ToString("s");
                string now_time = DateTime.Now.ToString("s");
                var tmpobj = new
                {
                    delta,
                    refresh_time,
                    now_time,
                };
                Context.Response.Write(new JavaScriptSerializer().Serialize(tmpobj));
                if (delta < 120000)
                    return;
            }

            WeChatJsapiTicketEntity myJsapiTokenEntity;
            JavaScriptSerializer jss = new JavaScriptSerializer();

            access_token = DBOperation.get_local_token_entiny();
            if (access_token == null)
                return;
            string jsapiTicketUrl = string.Format(get_jsapi_ticket_url, access_token.access_token);
            string jsapiTicketStr = utils.load_data_from_url(jsapiTicketUrl, false, null);
            myJsapiTokenEntity = jss.Deserialize<WeChatJsapiTicketEntity>(jsapiTicketStr);
            ticket = myJsapiTokenEntity.ticket;
            expires = myJsapiTokenEntity.expires_in;

            DBOperation.set_local_token_ticket(1, ticket, Convert.ToInt32(expires));

            return;
        }
        */

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
        public void getRemoteToken()
        {
            int errCode = 0;
            string token = null;

            AccessTokenEntiny entiny = DBOperation.get_remote_token_entiny();
            if (entiny == null)
                return;
            else
                token = entiny.access_token;

            leave:
            var tmpobj = new
            {
                errCode,
                token,
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
            string accessTokenStr = utils.load_data_from_url(accessTokenUrl, false, null);
            myTokenEntity = jss.Deserialize<WeChatOauth2TokenEntity>(accessTokenStr);
            oauth2AccessToken = myTokenEntity.Access_token;
            openId = myTokenEntity.Openid;

            redirect_url = string.Format(kingdee_wangshi_loading_page_url, openId);

            Context.Response.Redirect(redirect_url);
            Context.Response.End();
        }

        [WebMethod]
        public void test_loading_page(string openId)
        {
            int retCode = 0;

            string redirect_url = string.Format(kingdee_wangshi_loading_page_url, openId);

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
    }
}
