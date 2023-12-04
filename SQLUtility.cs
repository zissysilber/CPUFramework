using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace CPUFramework
{
    public class SQLUtility
    {
        public static string ConnectionString = "";
        public static DataTable GetDataTable(string sqlstatement) // - take a SQL statement and return a DataTable
        {
            Debug.Print(sqlstatement);
            DataTable dt = new();
            SqlConnection conn = new();
            conn.ConnectionString = ConnectionString;
            conn.Open();

            var cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sqlstatement;
            var dr = cmd.ExecuteReader();
            dt.Load(dr);

            SetAllColumnsAllowNull(dt);

            return dt;
        }

        private static void SetAllColumnsAllowNull(DataTable dt)
        {
            foreach (DataColumn c in dt.Columns)
            {
                c.AllowDBNull = true;
            }
        }
        public static void ExecuteSQL(string sqlstatement)
        {
            GetDataTable(sqlstatement);
        }

        public static int GetFirstColumnFirstRowValue(string sql)
        {
            int n = 0;

            DataTable dt = GetDataTable(sql);
            if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
            {
                if (dt.Rows[0][0] != DBNull.Value)
                {
                    int.TryParse(dt.Rows[0][0].ToString(), out n);
                }
                                
            }
            return n;
        }


        public static string GetFirstColumnFirstRowString(string sql)
        {
            string s = "";

            DataTable dt = GetDataTable(sql);
            if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
            {
                if (dt.Rows[0][0] != DBNull.Value)
                {
                    s = dt.Rows[0][0].ToString();
                }

            }
            return s;
        }
        /*
         *             DataTable dt = new();
            SqlConnection conn = new SqlConnection(SQLUtility.ConnectionString);
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "PartyGet";

            conn.Open();
            

            cmd.Parameters["@All"].Value = 1;

            SqlDataReader dr = cmd.ExecuteReader();
            dt.Load(dr);

            return dt;
         */
        public static SqlCommand GetSQLCommand(string sprocname)
        {
            
            
            SqlConnection conn = new SqlConnection(SQLUtility.ConnectionString);
            SqlCommand cmd = new();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sprocname;

            
            SqlCommandBuilder.DeriveParameters(cmd);

            conn.Open();
            return cmd;
        }
        
        public static SqlCommand GetTable(SqlCommand cmd)
        {
            DataTable dt = new();

            
            SqlDataReader dr = cmd.ExecuteReader();

            dt.Load(dr);

            return cmd;
            
        }

        public static void DebugPrintDataTable(DataTable dt)
            {
                foreach (DataRow r in dt.Rows)
                {
                    foreach (DataColumn c in dt.Columns)
                    {
                        Debug.Print(c.ColumnName + " = " + r[c.ColumnName].ToString());
                    }
                }
            }
        }
    }

