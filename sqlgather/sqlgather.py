# -*- coding: utf-8 -*-
import MySQLdb
import pymssql
from datetime import *

DATABASE_NAME = "kingdee_wangshi"
MYSQL_HOST = "localhost"
MYSQL_USER = "root"
MYSQL_PASSWORD = "23Imaxgine"
MYSQL_DATABASE = "kingdee_wangshi"

GET_SQL_ID_INTERVAL = 100

#TABLE INDEX
RECORD_INFO_LAST_ID_INDEX = 3
PAGE_VISIT_INFO_ID_INDEX = 0
BUTTON_VISIT_INFO_ID_INDEX = 0
FROM_INFO_ID_INDEX = 0

def mysql_connect():
    conn = MySQLdb.connect(host=MYSQL_HOST, port=3306, user=MYSQL_USER, passwd=MYSQL_PASSWORD, db=MYSQL_DATABASE, charset="utf8")
    cur = conn.cursor()
    return (conn, cur)


def mysql_close(conn, cur):
    cur.close()
    conn.close()


def sqlserver_connect(server, user, password, database):
    conn = pymssql.connect(server=server, port="1433", user=user, password=password, database=database,charset="UTF-8")
    cur = conn.cursor()
    return (conn, cur)


def sqlserver_close(conn, cur):
    cur.close()
    conn.close()


def read_sql_config(conf_path):
    sqlconf = []
    file = open(conf_path)
    for line in file:
        line = line.strip()
        if line != "":
            sqlconf.append(line.split(":"))
        #len(sqlconf)
    return sqlconf


def get_record_lastid(sql_address_value, mysql_conn, mysql_cur):
    mysql_cur.execute("select * from record_info where sql_address=%s and table_name='page_vist_info'", (sql_address_value))
    rows = mysql_cur.fetchall()
    if mysql_cur.rowcount != 1:
        page_visit_index = 0
    else:
        page_visit_index = rows[0][RECORD_INFO_LAST_ID_INDEX]

    mysql_cur.execute("select * from record_info where sql_address=%s and table_name='button_vist_info'", (sql_address_value))
    rows = mysql_cur.fetchall()
    if mysql_cur.rowcount != 1:
        button_visit_index = 0
    else:
        button_visit_index = rows[0][RECORD_INFO_LAST_ID_INDEX]

    mysql_cur.execute("select * from record_info where sql_address=%s and table_name='from_info'", (sql_address_value))
    rows = mysql_cur.fetchall()
    if mysql_cur.rowcount != 1:
        from_info_index = 0
    else:
        from_info_index = rows[0][RECORD_INFO_LAST_ID_INDEX]         
    return (page_visit_index, button_visit_index, from_info_index)


def set_record_lastid(sql_address_value, page_visit_lastid, button_visit_lastid, from_info_lastid, mysql_conn, mysql_cur):
    refresh_time = str(datetime.now())

    mysql_cur.execute("select * from record_info where sql_address=%s and table_name='page_vist_info'", (sql_address_value))
    rows = mysql_cur.fetchall()
    if mysql_cur.rowcount == 0:
        mysql_cur.execute("insert into record_info (sql_address, table_name, last_id, refresh_time) values (%s, %s, %s, %s)", (sql_address_value, "page_visit_info", page_visit_lastid, refresh_time))
    elif mysql_cur.rowcount == 1:
        mysql_cur.execute("update record_info set last_id=%s, refresh_time=%s where sql_address=%s and table_name='page_vist_info'", (page_visit_lastid, refresh_time, sql_address_value))

    mysql_cur.execute("select * from record_info where sql_address=%s and table_name='button_vist_info'", (sql_address_value))
    rows = mysql_cur.fetchall()
    if mysql_cur.rowcount == 0:
        mysql_cur.execute("insert into record_info (sql_address, table_name, last_id, refresh_time) values(%s, %s, %s, %s)", (sql_address_value, "button_visit_info", button_visit_lastid, refresh_time))
    elif mysql_cur.rowcount == 1:
        mysql_cur.execute("update record_info set last_id=%s, refresh_time=%s where sql_address=%s and table_name='button_vist_info'", (button_visit_lastid, refresh_time, sql_address_value))

    mysql_cur.execute("select * from record_info where sql_address=%s and table_name='from_info'", (sql_address_value))
    rows = mysql_cur.fetchall()
    if mysql_cur.rowcount == 0:
        mysql_cur.execute("insert into record_info (sql_address, table_name, last_id, refresh_time) values(%s, %s, %s, %s)", (sql_address_value, "from_info", from_info_lastid, refresh_time))
    elif mysql_cur.rowcount == 1:
        mysql_cur.execute("update record_info set last_id=%s, refresh_time=%s where sql_address=%s and table_name='from_info'", (from_info_lastid, refresh_time, sql_address_value))

    mysql_conn.commit()


def gather_data_from_pagevisitinfo(lastid, sqlserver_conn, sqlserver_cur, mysql_conn, mysql_cur):
    maxid = 0
    sqlserver_cur.execute("select top 1 * from page_visit_info order by id desc")
    rows = sqlserver_cur.fetchall()
    if sqlserver_cur.rowcount == 1:
        maxid = rows[0][PAGE_VISIT_INFO_ID_INDEX]
        if maxid > lastid:
            fromid = lastid
            while 1:
                toid = fromid + GET_SQL_ID_INTERVAL
                sqlserver_cur.execute("select top %d * from page_visit_info where id between %d and %d", (GET_SQL_ID_INTERVAL, fromid, toid))
                rows = sqlserver_cur.fetchall()
                mysql_cur.executemany("insert into page_visit_info (old_id, openid, page, visit_time) values (%s, %s, %s, %s)", rows)
                if toid >= maxid:
                    break
                else:
                    fromid = toid
            mysql_conn.commit()
    return maxid


def gather_data_from_buttonvisitinfo(lastid, sqlserver_conn, sqlserver_cur, mysql_conn, mysql_cur):
    maxid = 0
    sqlserver_cur.execute("select top 1 * from button_visit_info order by id desc")
    rows = sqlserver_cur.fetchall()
    if sqlserver_cur.rowcount == 1:
        maxid = rows[0][BUTTON_VISIT_INFO_ID_INDEX]
        if maxid > lastid:
            fromid = lastid
            while 1:
                toid = fromid + GET_SQL_ID_INTERVAL
                sqlserver_cur.execute("select top %d * from button_visit_info where id between %d and %d", (GET_SQL_ID_INTERVAL, fromid, toid))
                rows = sqlserver_cur.fetchall()
                mysql_cur.executemany("insert into button_visit_info (old_id, openid, button, visit_time) values (%s, %s, %s, %s)", rows)
                if toid >= maxid:
                    break
                else:
                    fromid = toid
            mysql_conn.commit()
    return maxid    


def gather_data_from_frominfo(lastid, sqlserver_conn, sqlserver_cur, mysql_conn, mysql_cur):
    maxid = 0
    sqlserver_cur.execute("select top 1 * from from_info order by id desc")
    rows = sqlserver_cur.fetchall()
    if sqlserver_cur.rowcount == 1:
        maxid = rows[0][FROM_INFO_ID_INDEX]
        if maxid > lastid:
            fromid = lastid
            while 1:
                toid = fromid + GET_SQL_ID_INTERVAL
                sqlserver_cur.execute("select top %d * from from_info where id between %d and %d", (GET_SQL_ID_INTERVAL, fromid, toid))
                rows = sqlserver_cur.fetchall()
                mysql_cur.executemany("insert into from_info (old_id, openid, fromwhere, visit_time) values (%s, %s, %s, %s)", rows)
                if toid >= maxid:
                    break
                else:
                    fromid = toid
            mysql_conn.commit()
    return maxid  


def gather_data_from_sqlserver(sqlconf):
    for config in sqlconf:
        sqlserver_conn, sqlserver_cur = sqlserver_connect(config[0], config[1], config[2], DATABASE_NAME)
        (mysql_conn, mysql_cur) = mysql_connect()

        (page_visit_lastid, button_visit_lastid, from_info_lastid) = get_record_lastid(config[0], mysql_conn, mysql_cur)

        #gather data
        page_visit_lastid = gather_data_from_pagevisitinfo(page_visit_lastid, sqlserver_conn, sqlserver_cur, mysql_conn, mysql_cur)
        button_visit_lastid = gather_data_from_buttonvisitinfo(button_visit_lastid, sqlserver_conn, sqlserver_cur, mysql_conn, mysql_cur)
        from_info_lastid = gather_data_from_frominfo(from_info_lastid, sqlserver_conn, sqlserver_cur, mysql_conn, mysql_cur)

        set_record_lastid(config[0], page_visit_lastid, button_visit_lastid, from_info_lastid, mysql_conn, mysql_cur)

        sqlserver_close(sqlserver_conn, sqlserver_cur)
        mysql_close(mysql_conn, mysql_cur)


def main():
    sqlconf = read_sql_config("sql.conf")
    gather_data_from_sqlserver(sqlconf)
