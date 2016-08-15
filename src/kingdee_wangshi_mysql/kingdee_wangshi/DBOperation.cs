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

using MySql.Data.MySqlClient;
using System.Configuration;

namespace kingdee_wangshi
{
    public class DBOperation : IDisposable
    {
        private static String ConServerStr = @"Data Source=iZ941e15yquZ;Initial Catalog=kingdee_wangshi;Integrated Security=False;User ID=sa;Password=23Imaxgine";

        //默认构造函数  
        public DBOperation()
        {
        }

        //关闭/销毁函数，相当于Close()  
        public void Dispose()
        {
        }

        public static void testMysql()
        {
            MySqlConnection conn;
            MySqlCommand com;
            conn = new MySqlConnection(ConfigurationManager.AppSettings["mysql-notjustidea"]);
            conn.Open();
            com = new MySqlCommand();
            com.Connection = conn;

            string sqlStr = "select * from page_visit_info";
            DataSet ds = new DataSet();
            MySqlDataAdapter da = new MySqlDataAdapter(sqlStr, conn);
            da.Fill(ds);

            return;
        }

        public static MySqlConnection getMySqlConn()
        {
            MySqlConnection conn;
            conn = new MySqlConnection(ConfigurationManager.AppSettings["mysql-notjustidea"]);
            conn.Open();

            return conn;
        }

        public static void destroyMySqlConn(MySqlConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
            }
        }




        public static SqlConnection getSqlConn()
        {
            SqlConnection conn = null;

            conn = new SqlConnection();
            conn.ConnectionString = ConServerStr;
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

        public static void set_local_token_ticket(int table, string token_or_ticket, int expires_in)
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
            newRow["refresh_time"] = DateTime.Now;
            ds.Tables[table_str].Rows.Add(newRow);

            SqlCommandBuilder scb = new SqlCommandBuilder(adapter);
            adapter.Update(ds.Tables[table_str].GetChanges());

            DBOperation.destroySqlConn(conn);
            return;
        }

        public static AccessTokenEntiny get_local_token_entiny()
        {
            AccessTokenEntiny entiny = null;
            string token = null;

            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

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

        public static JsapiTicketEntiny get_local_ticket_entiny()
        {
            JsapiTicketEntiny entiny = null;
            string ticket = null;

            SqlConnection conn = null;
            conn = DBOperation.getSqlConn();

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
    }
}