using Microsoft.Data.SqlClient;
using System.Data;

namespace Quiz_DataBank.Classes
{
    public class Connection
    {
        private  string _ConnectionString;
        private  SqlConnection _connection;
        public Connection(IConfiguration configuration)
        {
            //string encryptedConnectionString = configuration.GetConnectionString("dbcs");
            LkDataConnection.EncryptDecrypt _lkencr = new LkDataConnection.EncryptDecrypt();

            ////   _ConnectionString = EncryptionHelper.Decrypt(encryptedConnectionString);
            //_ConnectionString = _lkencr.Decrypt("ABC",encryptedConnectionString);
            string encryptedServer = configuration.GetConnectionString("server");
            string encryptedUser = configuration.GetConnectionString("user");
            string encryptedPassword = configuration.GetConnectionString("password");
            string encryptedDatabase = configuration.GetConnectionString("database");

            string decryptedServer = _lkencr.Decrypt("ABC", encryptedServer);
            string decryptedUser = _lkencr.Decrypt("ABC", encryptedUser);
            string decryptedPassword = _lkencr.Decrypt("ABC", encryptedPassword);
            string decryptedDatabase = _lkencr.Decrypt("ABC", encryptedDatabase);

            string connectionString = configuration.GetConnectionString("dbcs");

            _ConnectionString = connectionString
                .Replace("$server", decryptedServer)
                .Replace("$user", decryptedUser)
                .Replace("$password", decryptedPassword)
                .Replace("$database", decryptedDatabase);
            _connection = new SqlConnection(_ConnectionString);

        }
        public SqlConnection GetSqlConnection()
        {
            return _connection;
        }
        public DataTable ExecuteQueryWithResult(string query)
        {
            using (SqlCommand command = new SqlCommand(query, _connection))
            {
                if (_connection.State != ConnectionState.Open)
                {
                    try
                    {
                        _connection.Open();
                    }
                    catch(Exception ex)
                    {
                        
                    }
                }
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        //public DataTable ExecuteQueryWithResults(string query, IDictionary<string, object> sqlParam)
        //{
        //    using (SqlCommand command = new SqlCommand(query, _connection))
        //    {
        //        if (_connection.State != ConnectionState.Open)
        //        {
        //            _connection.Open();
        //        }

        //        foreach (var parameter in sqlParam)
        //        {
        //            if (parameter.Value is DateTime)
        //            {
        //                command.Parameters.Add(parameter.Key, SqlDbType.DateTime).Value = parameter.Value;
        //            }
        //            else if (parameter.Value is int)
        //            {
        //                command.Parameters.Add(parameter.Key, SqlDbType.Int).Value = parameter.Value;
        //            }
        //            else
        //            {
        //                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
        //            }
        //        }

        //        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
        //        {
        //            DataTable table = new DataTable();
        //            adapter.Fill(table);
        //            if (table.Rows.Count == 0)
        //            {
        //                Console.WriteLine("No records found.");
        //            }
        //            return table;
        //        }
        //    }
        //}

        public DataTable ExecuteQueryWithResults(string query, IDictionary<string, object> sqlParam)
        {
            using (SqlCommand command = new SqlCommand(query, _connection))
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                foreach (var parameter in sqlParam)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);

                }
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
        public void ExecuteQueryWithoutResult(string query)
        {

            using (SqlCommand com = new SqlCommand(query, _connection))
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                com.ExecuteNonQuery();
            }
        }
        public object ExecuteScalar(string query)
        {
            {
                SqlCommand command = new SqlCommand(query, _connection);
                _connection.Open();
                return command.ExecuteScalar();
            }
        }
        public void ExecuteInsertOrUpdate(string query, Dictionary<string, object> parameters)
        {
            using (SqlCommand com = new SqlCommand(query, _connection))
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }


      
                foreach (var parameter in parameters)
                {
                    com.Parameters.AddWithValue("@" + parameter.Key, parameter.Value);
                }

                com.ExecuteNonQuery();
            }
         


        }
        public string GetOldImagePathFromDatabase(int User_ID)
        {
            string oldImagePath = null;

            try
            {
                string query = "SELECT image FROM Users_mst WHERE User_ID = @User_ID";
                //Console.WriteLine($"_connection{_ConnectionString}");
                //using (SqlConnection connection = GetSqlConnection())
                //{

                using (SqlCommand command = new SqlCommand(query, _connection))
                {
                    _connection.Open();

                    command.Parameters.AddWithValue("@User_ID", User_ID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldImagePath = reader["image"].ToString();
                            Console.WriteLine(oldImagePath);
                        }
                    }
                    _connection.Close();
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving old image path from database: {ex.Message}");

            }

            return oldImagePath;
        }
    }

}
