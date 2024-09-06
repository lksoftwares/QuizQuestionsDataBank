using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quiz_DataBank.Classes;
using Quiz_DataBank.Model;
using System.Data;

namespace Quiz_DataBank.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class QuizTransactionController : ControllerBase
    {
        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public QuizTransactionController(Quiz_DataBank.Classes.Connection connection)
        {
            _connection = connection;
            DataAccessMethod();
        }

        private void DataAccessMethod()
        {
            LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;
            LkDataConnection.Connection.Connect();
            _dc = new LkDataConnection.DataAccess();
            _query = new LkDataConnection.SqlQueryResult();
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("GetAllQuizQuestion")]
        public IActionResult GetAllQuizQuestion([FromQuery] IDictionary<string, string> param)
        {
            string query = $" select QT.*,Q.*,U.* from Quiz_Transaction_mst QT join Questions_mst Q ON QT.Ques_ID=Q.Ques_ID join Users_mst U ON QT.User_ID= U.User_ID";

            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();
            if (param.TryGetValue("quizDate", out string QuizDate) && DateTime.TryParse(QuizDate, out DateTime quizDate))
            {
                filter.Add("QT.Quiz_Date = @quizDate");
                sqlparams.Add("@quizDate", quizDate);
            }
            if (param.TryGetValue("Email", out string Email))
            {
                filter.Add("  U.User_Email = @Email");
                sqlparams.Add("@Email", Email);
            }
            if (filter.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filter);
            }


            DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);


            var QuesList = new List<QuizTransactionModel>();
            foreach (DataRow row in Table.Rows)
            {
                QuesList.Add(new QuizTransactionModel
                {
                     Quiz_ID = Convert.ToInt32(row["Quiz_ID"]),
                    User_ID = Convert.ToInt32(row["User_ID"]),
                    Ques_ID = Convert.ToInt32(row["Ques_ID"]),
                    Ques_Desc = row["Ques_Desc"].ToString(),
                    Opt_A = row["Opt_A"].ToString(),
                    Opt_B = row["Opt_B"].ToString(),
                    Opt_C = row["Opt_C"].ToString(),
                    Opt_D = row["Opt_D"].ToString(),
                    Quiz_Date = row["Quiz_Date"].ToString(),
                    User_Email = row["User_Email"].ToString()


                });
            }
            return Ok(QuesList);
        }

        [HttpPost]
        [Route("AddQuizTransaction")]
        public IActionResult SubmitAnswer([FromBody] List<QuizTransactionModel> quizList)
        {
            if (quizList == null || quizList.Count == 0)
            {
                return Ok("No transaction.");
            }

            try
            {



                string insertQuery = "INSERT INTO Quiz_Transaction_mst (Ques_ID, Quiz_Date,User_ID) VALUES ";

                List<string> valueRows = new List<string>();
                foreach (var quiz in quizList)
                {
                    valueRows.Add($"('{quiz.Ques_ID}', '{quiz.Quiz_Date}','{quiz.User_ID}')");
                }

                insertQuery += string.Join(", ", valueRows);
                var connection = new LkDataConnection.Connection();

                connection.bindmethod(insertQuery);


                return Ok("Quiz_Transaction_mst Submitted Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

       

        [HttpPut]
        [Route("updateQuizTransaction/{Quiz_ID}")]
        public IActionResult updateQuizTransaction(int Quiz_ID, [FromBody] QuizTransactionModel quiz)
        {
            try
            {
                //var duplicacyChecker = new CheckDuplicacy(_connection);

                //bool isDuplicate = duplicacyChecker.CheckDuplicate("Quiz_Transaction_mst",
                //    new[] { "Ques_ID" },
                //    new[] { quiz.Ques_ID.ToString() },
                //    "Quiz_ID",Quiz_ID.ToString()
                //    );

                //if (isDuplicate)
                //{
                //    return BadRequest("Quiz_Transaction_mst already exists.");
                //}
                _query = _dc.InsertOrUpdateEntity(quiz, "Quiz_Transaction_mst", Quiz_ID, "Quiz_ID");
                return Ok("Quiz_Transaction_mst Updated Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }

        [HttpDelete]
        [Route("deleteQuizTransaction/{id}")]
        public IActionResult deleteQuizTransaction(int id)
        {
            string deleteQuery = $"Delete from Quiz_Transaction_mst where Quiz_ID='{id}'";
            try
            {
                LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);
                return Ok("Quiz_Transaction Deleted successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }

    }
}

