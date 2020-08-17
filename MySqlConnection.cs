using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for MySqlConnection
/// </summary>
public class MySqlConnection
{
    public static SqlConnection getconnection()
    {
        SqlConnection connection = new SqlConnection();
        connection.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        return connection;


    }

    public static DataTable gettable(SqlCommand command)
    {
        SqlDataAdapter adapter = new SqlDataAdapter(command);
        DataTable dt = new DataTable();
        adapter.Fill(dt);
     
        return dt;
    }
}