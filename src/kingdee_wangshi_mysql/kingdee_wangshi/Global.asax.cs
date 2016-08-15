using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

using System.Web.Script.Services;
using System.Data.SqlClient;
using System.Data;
using System.Web.Script.Serialization;
using System.Net;
using System.Text;
using System.Web.Security;

namespace kingdee_wangshi
{
    public class Global : System.Web.HttpApplication
    {
        string appId = "wxd73b44e3381aa8fd";
        string appSecret = "88ac9fe5320f96b69383a246b70dd1a6";
        //string appId = "wx42c72963b47ae521";
        //string appSecret = "62b506607b45778b76762428755a4a3e";
        string get_accessToken_url = "https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";
        string get_jsapi_ticket_url = "https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={0}&type=jsapi";
        static int access_token_table = 0;
        static int jsapi_ticket_table = 1;
        static int access_token_timer_expires = 60000; //one minute.
        static int jsapi_ticket_timer_expires = 60000;
        static int access_token_expires = 3600; //one hour.
        static int jsapi_ticket_expires = 3600; //one hour.

        void tokenTimer_elapsed(object sender, EventArgs e)
        {
            string token = null;
            string expires = null;

            AccessTokenEntiny entiny = DBOperation.get_local_token_entiny();
            if (entiny == null)
                return;
            else
            {
                int delta = utils.get_delta_second(entiny.refresh_time);
                if (delta < access_token_expires)
                    return;
            }

            WeChatAccessTokenEntity myTokenEntity;
            JavaScriptSerializer jss = new JavaScriptSerializer();

            string accessTokenUrl = string.Format(get_accessToken_url, appId, appSecret);
            string accessTokenStr = utils.load_data_from_url(accessTokenUrl, false, null);
            myTokenEntity = jss.Deserialize<WeChatAccessTokenEntity>(accessTokenStr);
            token = myTokenEntity.Access_token;
            expires = myTokenEntity.Expires_in;

            DBOperation.set_local_token_ticket(access_token_table, token, Convert.ToInt32(expires));

            return;
        }

        void ticketTimer_elapsed(object sender, EventArgs e)
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
                if (delta < jsapi_ticket_expires)
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

            DBOperation.set_local_token_ticket(jsapi_ticket_table, ticket, Convert.ToInt32(expires));

            return;
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine("Hello world.");
            string token = null;
            string ticket = null;
            string expires = null;
            WeChatAccessTokenEntity myTokenEntity;
            WeChatJsapiTicketEntity myJsapiTokenEntity;
            JavaScriptSerializer jss = new JavaScriptSerializer();

            string accessTokenUrl = string.Format(get_accessToken_url, appId, appSecret);
            string accessTokenStr = utils.load_data_from_url(accessTokenUrl, false, null);
            myTokenEntity = jss.Deserialize<WeChatAccessTokenEntity>(accessTokenStr);
            token = myTokenEntity.Access_token;
            expires = myTokenEntity.Expires_in;
            DBOperation.set_local_token_ticket(access_token_table, token, Convert.ToInt32(expires));

            string jsapiTicketUrl = string.Format(get_jsapi_ticket_url, token);
            string jsapiTicketStr = utils.load_data_from_url(jsapiTicketUrl, false, null);
            myJsapiTokenEntity = jss.Deserialize<WeChatJsapiTicketEntity>(jsapiTicketStr);
            ticket = myJsapiTokenEntity.ticket;
            expires = myJsapiTokenEntity.expires_in;
            DBOperation.set_local_token_ticket(jsapi_ticket_table, ticket, Convert.ToInt32(expires));

            System.Timers.Timer tokenTimer = new System.Timers.Timer();
            tokenTimer.Interval = access_token_timer_expires;
            tokenTimer.Enabled = true;
            tokenTimer.Elapsed += new System.Timers.ElapsedEventHandler(tokenTimer_elapsed);

            System.Timers.Timer ticketTimer = new System.Timers.Timer();
            ticketTimer.Interval = jsapi_ticket_timer_expires;
            ticketTimer.Enabled = true;
            ticketTimer.Elapsed += new System.Timers.ElapsedEventHandler(ticketTimer_elapsed);

            return;
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}