using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data.SqlClient;

namespace kingdee_wangshi
{
    public class DBOperation : IDisposable
    {
        private static String ConServerStr = @"Data Source=iZ941e15yquZ;Initial Catalog=kingdeeStat;Integrated Security=False;User ID=sa;Password=23Imaxgine";

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

        public static void destroySqlConn(SqlConnection conn)
        {
            if (conn != null)
            {
                conn.Close();
            }
        }
    }
}