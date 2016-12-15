using PacketSniffer.PackageTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsFormsApplication.Utils;

namespace WindowsFormsApplication.DataAccessLayer
{
    public class DAL
    {
        private SQLiteConnection connection;
        private string startTime;
        private string startDate;

        private object synclock;

        public DAL()
        {
            // Initializare baza de date
            connection = new SQLiteConnection("Data Source=:memory:;Page Size=3072;Cache Size=4000;Journal Mode=OFF");
            connection.Open();

            // date ale pornirii noii sesiuni
            startDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            startTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);

            synclock = new object();

            createDatabase();
        }

        private void createDatabase()
        {
            // crearea tabelului IPPackets
            string sql = "CREATE TABLE IPPackets (ID INTEGER PRIMARY KEY, TimeStamp VARCHAR (30), Date VARCHAR (30), Direction VARCHAR (255, 0), Protocol VARCHAR (30), ApplicationName VARCHAR (40), SourceAddress VARCHAR (20), SourcePort VARCHAR (10), DestinationAddress VARCHAR (20), DestinationPort VARCHAR (10), TTL INT, PacketLength INT)";
            SQLiteCommand command = new SQLiteCommand(sql, connection);

            lock (synclock)
            {
                command.ExecuteNonQuery();
            }  
        }

        // resetarea timpilor de inceput al monitorizarii pentru curatarea graficelor
        public void resetDatabase()
        {
            startDate = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            startTime = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        }

        public void AddIPPacketToDataBase(string sql)
        {
            var command = new SQLiteCommand(sql, connection);
            command.ExecuteNonQueryAsync();
        }

        // Intoarce date pentru tabela pachetelor IP
        public DataTable GetIPPackets(DataTable dt)
        {
            string sql = "select * from IPPackets where";

            if (dt.Rows.Count == 0)
            {
                sql += " TimeStamp >= '" + startTime + "' AND Date >= '" + startDate + "'";
            }
            else
            {
                sql += " ID > " + dt.Rows[dt.Rows.Count - 1]["ID"].ToString();
            }

            var command = new SQLiteCommand(sql, connection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {                
                dt.Rows.Add(reader["ID"], dt.Rows.Count + 1, reader["Date"], reader["TimeStamp"], reader["Direction"], reader["Protocol"], reader["ApplicationName"], reader["SourceAddress"],
                    reader["SourcePort"], reader["DestinationAddress"], reader["DestinationPort"], reader["TTL"], reader["PacketLength"]);
            }
            
            return dt;
        }

        public Dictionary<string, int> GetPiechartData(ChartContent content, ContentData contentData)
        {
            Dictionary<string, int> data = new Dictionary<string, int>();

            string sql = "select " + content.ToString() + ", " + contentData + "(PacketLength) as " + contentData + " from";

            if (content == ChartContent.Port)
            {
                sql += " (select case when Direction = 'Incoming' then DestinationPort else SourcePort end as 'Port', PacketLength from IPPackets where TimeStamp >= '" + startTime + "' AND Date >= '" + startDate + "')";
            }
            else
            {
                sql += " IPPackets where TimeStamp >= '" + startTime + "' AND Date >= '" + startDate + "' AND length(" + content.ToString() + ") > 0";
            }

            sql += " group by " + content.ToString();

            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReaderAsync().Result;

            while (reader.Read())
            {
                data.Add(reader[content.ToString()].ToString(), int.Parse(reader[contentData.ToString()].ToString()));
            }

            return data;
        }

        public List<object[]> GetTimeChartData(ChartContent content, ContentData contentData, DataTable dataTable, out Dictionary<string, string> dates)
        {
            // durata in secunde a graficului in timp
            int arrayLength = 120;

            Dictionary<string, int> data = new Dictionary<string, int>();
            dates = new Dictionary<string, string>();

            if (dataTable.Rows.Count == 0)
            {
                return null;
            }

            string sql = "select TimeStamp, Date from IPPackets where TimeStamp >= '" + startTime + "' AND Date >= '"
                + startDate + "' group by TimeStamp, Date";

            var command = new SQLiteCommand(sql, connection);
            var reader = command.ExecuteReaderAsync().Result;

            while (reader.Read())
            {
                dates.Add(reader["TimeStamp"].ToString(), reader["Date"].ToString());
            }

            if (dates.Count > arrayLength)
            {
                var newDates = new Dictionary<string, string>();
                for (int i = dates.Count - arrayLength; i < dates.Count; i++)
                {
                    newDates.Add(dates.ElementAt(i).Key, dates.ElementAt(i).Value);
                }
                dates = newDates;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("select {0}", content.ToString());

            for (int i = 0; i < dates.Count; i++)
            {
                sb.AppendFormat(", {0}(time{1}) as time{1}", contentData, i);
            }

            sb.AppendLine("\nfrom (");
            sb.AppendFormat("select {0},\n", content.ToString());

            for (int i = 0; i < dates.Count - 1; i++)
            {
                sb.AppendFormat("case when TimeStamp >= '{0}' AND TimeStamp < '{1}' AND Date >= '{2}' AND ", dates.ElementAt(i).Key, dates.ElementAt(i + 1).Key, dates.ElementAt(i).Value);
                sb.AppendFormat(" Date <= '{0}' then PacketLength end as 'time{1}',\n", dates.ElementAt(i + 1).Value, i);
            }

            sb.AppendFormat("case when TimeStamp >= '{0}' AND Date >= '{1}' then PacketLength end as 'time{2}'\n", dates.Last().Key, dates.Last().Value, dates.Count - 1);

            if(content == ChartContent.Port)
            {
                sb.Append("from (select case when Direction = 'Incoming' then DestinationPort else SourcePort end as 'Port', TimeStamp, Date, PacketLength from IPPackets)");
            }
            else
            {
                sb.Append("from IPPackets");
            }
            
            sb.AppendFormat(" where length({0}) > 0)\n", content.ToString());
            sb.AppendFormat("group by {0}\n", content.ToString());
            sb.Append("having ");
            sb.Append(" Count(time0) > 0 ");

            for (int i = 1; i < dates.Count; i++)
            {
                sb.AppendFormat(" or Count(time{0}) > 0", i);
            }

            command = new SQLiteCommand(sb.ToString(), connection);
            reader = command.ExecuteReaderAsync().Result;

            DataTable dt = new DataTable();

            List<object[]> objects = new List<object[]>();
            while (reader.Read())
            {
                object[] results = new object[arrayLength];

                reader.GetValues(results);
                objects.Add(results);
            }
            
            return objects;
        }
    }
}
