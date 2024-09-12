using System.Data.Common;
using System.Data;

namespace Quiz_DataBank.Classes
{
    public class CheckDuplicacy
    {
        private readonly Connection _connection;

        public CheckDuplicacy(Connection connection)
        {
            _connection = connection;
        }

       
        public bool CheckDuplicate(string tableName, string[] fields, string[] values, string idField = null, string idValue = null)
        {
            string conditions = string.Join(" OR ", Enumerable.Range(0, fields.Length)
                .Select(i => $"{fields[i]} = '{values[i]}'"));

            string query = $"SELECT * FROM {tableName} WHERE ({conditions})";

            if (!string.IsNullOrEmpty(idField) && idValue!=null)
            {
                query += $" AND {idField} != '{idValue}'";
            }

            DataTable result = _connection.ExecuteQueryWithResult(query);

            return result.Rows.Count > 0;
        }

        //    public bool CheckDuplicate(string tableName, string[] fields, string[] values, string ID = null)
        //    {
        //        string conditions = string.Join(" OR ", Enumerable.Range(0, fields.Length)
        //            .Select(i => $"{fields[i]} = '{values[i]}'"));

        //        string query = $"SELECT * FROM {tableName} WHERE ({conditions})";

        //        if (!string.IsNullOrEmpty(ID) && fields.Contains(ID))
        //        {
        //            int idIndex = Array.IndexOf(fields, ID);
        //            query += $" AND {ID} != '{values[idIndex]}'";
        //        }

        //        DataTable result = _connection.ExecuteQueryWithResult(query);

        //        return result.Rows.Count > 0;
        //    }



        //}
    }
}