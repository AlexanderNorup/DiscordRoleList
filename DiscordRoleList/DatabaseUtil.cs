using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;


namespace DiscordRoleList
{
    class DatabaseUtil
    {

        public MySqlConnection conn;
        private String authUrl;
        public String lastError = "";

        public DatabaseUtil(String authUrl)
        {
            this.authUrl = authUrl;
        }

        public void Close() //Backwards compatability. I'm sorry
        {
            Disconnect();
        }

        public bool isConnected()
        {
            if (conn == null)
            {
                return false;
            }
            return conn.State != System.Data.ConnectionState.Closed && conn.State != System.Data.ConnectionState.Broken;
        }

        public void Disconnect()
        {
            if (conn != null)
            {
                conn.Close();
            }
        }

        public bool Connect()
        {
            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = authUrl;
                conn.Open();
            }
            catch (MySqlException ex)
            {
                lastError = ex.Message;
                _ = Logger.Log("Database error: " + ex.Message, Discord.LogSeverity.Error, "MySQL Connection");
                return false;
            }
            return true;
        }

        public MySqlDataReader Execute(String query)
        {
            return Execute(query, new Dictionary<String, object>());
        }

        public int InsertUpdateVals(String table, Dictionary<String, String> map)
        {
            if (!isConnected())
            {
                if (!Connect())
                {
                    throw new Exception("Database could not connect: " + lastError);
                }
            }

            String keys_ = "(";
            String vals_ = "(";
            String update_ = "";

            foreach (KeyValuePair<string, string> entry in map)
            {
                vals_ += "'" + MySqlHelper.EscapeString(entry.Value) + "',";
                keys_ += entry.Key + ",";
                update_ += entry.Key + "= '" + MySqlHelper.EscapeString(entry.Value) + "',";
            }
            keys_ = keys_.Substring(0, keys_.Length - 1) + ")";
            vals_ = vals_.Substring(0, vals_.Length - 1) + ")";
            update_ = update_.Substring(0, update_.Length - 1);
            String query = "INSERT INTO " + table + " " + keys_ + " values " + vals_ + " ON DUPLICATE KEY UPDATE " + update_ + ";";
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = query;
            return cmd.ExecuteNonQuery();
        }

        public int ExecuteNoResult(String query, Dictionary<String, object> map)
        {
            if (!isConnected())
            {
                if (!Connect())
                {
                    throw new Exception("Database could not connect: " + lastError);
                }
            }
            //Connected
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = query;
            cmd.Prepare();
            foreach (KeyValuePair<string, object> entry in map)
            {
                _ = cmd.Parameters.AddWithValue(entry.Key, entry.Value);
            }
            return cmd.ExecuteNonQuery();
        }

        public MySqlDataReader Execute(String query, Dictionary<String, object> map)
        {
            if (!isConnected())
            {
                if (!Connect())
                {
                    throw new Exception("Database could not connect: " + lastError);
                }
            }
            //Connected
            MySqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = query;
            cmd.Prepare();
            foreach (KeyValuePair<string, object> entry in map)
            {
                _ = cmd.Parameters.AddWithValue(entry.Key, entry.Value);
            }
            return cmd.ExecuteReader();
        }


    }
}
