using System;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.Odbc;
using System.Data.OracleClient;
namespace gzdemo
{
    // SqlServer连接数据库
    public class SqlDataLayer
    {
        //字段
        private  SqlConnection Connection;
        private string connectionString;
	    public   SqlCommand command ;

        //构造函数
        public SqlDataLayer(string newConnectionString)
        {
            connectionString = newConnectionString;
            try
            {
                Connection = new SqlConnection(connectionString);
                command = new SqlCommand("", Connection);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        //属性
        public string ConnectionString
        {
            get
            {
                return connectionString;
            }
        }

        public SqlConnection connection
        {
	        get
            {
                if (Connection.State == ConnectionState.Closed)
                {
                    try
                    {
                        Connection.Open();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message);
                    }
                }				    
		        return Connection;
	        }
        }

        //方法
        public SqlConnection GetConnection()
        {
            return Connection;
        }

        public SqlDataReader RunQuery( string sqlQuery )
        {
            SqlDataReader result = null;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
            command.CommandText = sqlQuery;
            //   result = command.ExecuteReader(CommandBehavior.CloseConnection); // ww 061102
            result = command.ExecuteReader();
            //   Connection.Close();
            return result;
        }

        public int RunNonQuery( string sqlNonQuery )
        {
            int result = -1;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlNonQuery;
            result = command.ExecuteNonQuery();
            // Connection.Close();
            return result;
        }

        public DataSet RunQuery( string sqlQuery, string tableName )
        {
            DataSet ds = new DataSet();
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
          
            command.CommandText = sqlQuery;
            SqlDataAdapter oraDA = new SqlDataAdapter();
            oraDA.SelectCommand = command;
            oraDA.Fill( ds, tableName );
            // Connection.Close();
            return ds;
        }

        public void RunQuery(string sqlQuery, DataSet dataSet, string tableName )
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            SqlDataAdapter oraDA = new SqlDataAdapter();
            oraDA.SelectCommand = command;
            oraDA.Fill(dataSet, tableName);
            // Connection.Close();			
        }

        public void ExeFunc(SqlCommand myCmd)
        { 
            //int result = -1;
            myCmd.Connection = this.Connection;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }
          
            //result = 
            myCmd.ExecuteNonQuery();
          //  Connection.Close();			
            //return result;
        }

        // 将查询到的数据填充为DataTable，并返回
        public DataTable FillTable( string sqlQuery)
        {
            DataTable dt = new DataTable();
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            SqlDataAdapter oraDA = new SqlDataAdapter();
            oraDA.SelectCommand = command;
            oraDA.Fill(dt);
            // Connection.Close();
            return dt;
        }

        public void FillSchema( string sqlQuery,DataSet dataSet,string tableName)
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            SqlDataAdapter oraDA = new SqlDataAdapter();
            oraDA.SelectCommand = command;
            oraDA.FillSchema(dataSet, SchemaType.Mapped, tableName);
            // Connection.Close();     
        }
        public SqlTransaction GetTrans()
        {
            return Connection.BeginTransaction();
        }

        public int Fill( DataTable dt, string sqlQuery)
        {
            int result;
            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
            }

            command.CommandText = sqlQuery;
            SqlDataAdapter oraDA = new SqlDataAdapter();
            oraDA.SelectCommand = command;
            result = oraDA.Fill(dt);
            // Connection.Close();
            return result;
        }

        public void CloseConnection()
        {
            Connection.Close();
        }
    }
}
