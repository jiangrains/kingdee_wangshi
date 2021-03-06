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

using System.Configuration;

namespace kingdee_wangshi
{
    public class DBOperation : IDisposable
    {
        private static String ConServerStr = @"Data Source=127.0.0.1;Initial Catalog=kingdee_wangshi;Integrated Security=False;User ID=sa;Password=23Imaxgine";
        private static String RemoteConServerStr = @"Data Source=10.45.188.231;Initial Catalog=kingdee_wangshi;Integrated Security=False;User ID=sa;Password=23Imaxgine";

        static int access_token_expires = 3600; //one hour.
        static int jsapi_ticket_expires = 3600; //one hour.
        static int access_token_table = 0;
        static int jsapi_ticket_table = 1;

        //默认构造函数  
        public DBOperation()
        {
        }

        //关闭/销毁函数，相当于Close()  
        public void Dispose()
        {
        }

        public static SqlConnection getSqlConn()
        {
            SqlConnection conn = null;

            conn = new SqlConnection();
            conn.ConnectionString = ConServerStr;
            conn.Open();

            return conn;
        }

        public static SqlConnection getRemoteSqlConn()
        {
            SqlConnection conn = null;

            conn = new SqlConnection();
            conn.ConnectionString = RemoteConServerStr;
            conn.Open();

            return conn;
        }

        public static void destroySqlConn(SqlConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
            }
        }

        public static void set_local_token_ticket_slave(int table, string token_or_ticket, int expires_in, DateTime refresh)
        {
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

            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

            string sqlStr = "select top 1 * from " + table_str + " order by id desc";
            SqlCommand selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(selectCMD);
            DataSet ds = new DataSet();
            adapter.Fill(ds, table_str);

            DataRow newRow = ds.Tables[table_str].NewRow();
            newRow[key_str] = token_or_ticket;
            newRow["expires_in"] = expires_in;
            newRow["refresh_time"] = refresh;
            ds.Tables[table_str].Rows.Add(newRow);

            SqlCommandBuilder scb = new SqlCommandBuilder(adapter);
            adapter.Update(ds.Tables[table_str].GetChanges());

            DBOperation.destroySqlConn(conn);
            return;
        }

        public static AccessTokenEntiny get_remote_token_entiny()
        {
            AccessTokenEntiny entiny = null;
            string token = null;

            SqlConnection conn = null;
            conn = DBOperation.getRemoteSqlConn();

            string sqlStr = "select top 1 * from access_token_info order by id desc";
            SqlCommand selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(selectCMD);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "access_token_info");

            if (ds.Tables["access_token_info"].Rows.Count == 0)
                goto leave;
            else
            {
                entiny = new AccessTokenEntiny();
                entiny.access_token = ds.Tables["access_token_info"].Rows[0]["access_token"].ToString();
                entiny.expires_in = Convert.ToInt32(ds.Tables["access_token_info"].Rows[0]["expires_in"]);
                entiny.refresh_time = (DateTime)ds.Tables["access_token_info"].Rows[0]["refresh_time"];
            }

            leave:
            DBOperation.destroySqlConn(conn);
            return entiny;
        }

        public static AccessTokenEntiny get_local_token_entiny_slave()
        {
            AccessTokenEntiny entiny = null;
            string token = null;
            int delta = 0;

            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

            string sqlStr = "select top 1 * from access_token_info order by id desc";
            SqlCommand selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(selectCMD);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "access_token_info");

            if (ds.Tables["access_token_info"].Rows.Count == 0)
            {
                entiny = get_remote_token_entiny();
                if (entiny == null)
                    goto leave;
                delta = utils.get_delta_second(entiny.refresh_time);
                if (delta < access_token_expires)
                {
                    set_local_token_ticket_slave(access_token_table, entiny.access_token, entiny.expires_in, entiny.refresh_time);
                }
            }
            else
            {
                entiny = new AccessTokenEntiny();
                entiny.access_token = ds.Tables["access_token_info"].Rows[0]["access_token"].ToString();
                entiny.expires_in = Convert.ToInt32(ds.Tables["access_token_info"].Rows[0]["expires_in"]);
                entiny.refresh_time = (DateTime)ds.Tables["access_token_info"].Rows[0]["refresh_time"];

                delta = utils.get_delta_second(entiny.refresh_time);
                if (delta >= access_token_expires)
                {
                    entiny = get_remote_token_entiny();
                    if (entiny == null)
                        goto leave;
                    delta = utils.get_delta_second(entiny.refresh_time);
                    if (delta < access_token_expires)
                    {
                        set_local_token_ticket_slave(access_token_table, entiny.access_token, entiny.expires_in, entiny.refresh_time);
                    }
                }
            }

            leave:
            DBOperation.destroySqlConn(conn);
            return entiny;
        }



        public static JsapiTicketEntiny get_remote_ticket_entiny()
        {
            JsapiTicketEntiny entiny = null;
            string ticket = null;

            SqlConnection conn = null;
            conn = DBOperation.getRemoteSqlConn();

            string sqlStr = "select top 1 * from jsapi_ticket_info order by id desc";
            SqlCommand selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(selectCMD);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "jsapi_ticket_info");

            if (ds.Tables["jsapi_ticket_info"].Rows.Count == 0)
                goto leave;
            else
            {
                entiny = new JsapiTicketEntiny();
                entiny.jsapi_ticket = ds.Tables["jsapi_ticket_info"].Rows[0]["jsapi_ticket"].ToString();
                entiny.expires_in = Convert.ToInt32(ds.Tables["jsapi_ticket_info"].Rows[0]["expires_in"]);
                entiny.refresh_time = (DateTime)ds.Tables["jsapi_ticket_info"].Rows[0]["refresh_time"];
            }

            leave:
            DBOperation.destroySqlConn(conn);
            return entiny;
        }

        public static JsapiTicketEntiny get_local_ticket_entiny_slave()
        {
            JsapiTicketEntiny entiny = null;
            string ticket = null;
            int delta = 0;

            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

            string sqlStr = "select top 1 * from jsapi_ticket_info order by id desc";
            SqlCommand selectCMD = new SqlCommand(sqlStr, conn);
            SqlDataAdapter adapter = new SqlDataAdapter(selectCMD);
            DataSet ds = new DataSet();
            adapter.Fill(ds, "jsapi_ticket_info");

            if (ds.Tables["jsapi_ticket_info"].Rows.Count == 0)
            {
                entiny = get_remote_ticket_entiny();
                if (entiny == null)
                    goto leave;
                delta = utils.get_delta_second(entiny.refresh_time);
                if (delta < jsapi_ticket_expires)
                {
                    set_local_token_ticket_slave(jsapi_ticket_table, entiny.jsapi_ticket, entiny.expires_in, entiny.refresh_time);
                }
            }
            else
            {
                entiny = new JsapiTicketEntiny();
                entiny.jsapi_ticket = ds.Tables["jsapi_ticket_info"].Rows[0]["jsapi_ticket"].ToString();
                entiny.expires_in = Convert.ToInt32(ds.Tables["jsapi_ticket_info"].Rows[0]["expires_in"]);
                entiny.refresh_time = (DateTime)ds.Tables["jsapi_ticket_info"].Rows[0]["refresh_time"];

                delta = utils.get_delta_second(entiny.refresh_time);
                if (delta >= jsapi_ticket_expires)
                {
                    entiny = get_remote_ticket_entiny();
                    if (entiny == null)
                        goto leave;
                    delta = utils.get_delta_second(entiny.refresh_time);
                    if (delta < jsapi_ticket_expires)
                    {
                        set_local_token_ticket_slave(jsapi_ticket_table, entiny.jsapi_ticket, entiny.expires_in, entiny.refresh_time);
                    }
                }
            }

            leave:
            DBOperation.destroySqlConn(conn);
            return entiny;
        }
    }
}
