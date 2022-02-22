using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace News_Parser
{
    public static class Helper
    {
        /// <summary>
        /// Соединение с базой данных.
        /// </summary>
        /// <returns>SqlConnection, представляющее соединение с Sql Server.</returns>
        public static SqlConnection ConnectToDB(bool isFirstStart = false)
        {
            var datasource = @"localhost\SQLEXPRESS";
            var database = "NewsDB";
            string connString = $"Server={datasource};Integrated Security=True;";
            if (!isFirstStart) connString += $"Database={database};";
            SqlConnection conn = new(connString);
            try
            {
                conn.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            return conn;
        }
    }
}
