using Microsoft.Data.SqlClient;
using System.Data;

namespace Quiz_DataBank.Classes
{
    public class Connection
    {
        private readonly string _ConnectionString;
        private readonly SqlConnection _connection;
        public Connection(IConfiguration configuration)
        {
            _ConnectionString = configuration.GetConnectionString("dbcs");
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
                    _connection.Open();
                }
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable table = new DataTable();
                    adapter.Fill(table);
                    return table;
                }
            }
        }
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


                // Add parameters
                foreach (var parameter in parameters)
                {
                    com.Parameters.AddWithValue("@" + parameter.Key, parameter.Value);
                }

                com.ExecuteNonQuery();
            }
         


        }
    }
}