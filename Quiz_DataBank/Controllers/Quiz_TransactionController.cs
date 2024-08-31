using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quiz_DataBank.Classes;
using Quiz_DataBank.Model;
using System.Data;

namespace Quiz_DataBank.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class Quiz_TransactionController : ControllerBase
    {
        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public Quiz_TransactionController(Quiz_DataBank.Classes.Connection connection)
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


        //[HttpPost]
        //[Route("SubmitAnswer")]
        //public IActionResult SubmitAnswer([FromBody] Quiz_TransactionModel quiz)
        //{
        //    try

        //    {



        //        _query = _dc.InsertOrUpdateEntity(quiz, "Quiz_Transaction_mst", -1);




        //        return Ok("Answer  Submitted Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        //    }
        //}






        [HttpGet]
        [Route("QuizTransaction")]
        public IActionResult GetQuestionBasedOnTopic([FromQuery] IDictionary<string, string> param)

        {
            string query = $"select QZ.*,U.User_Name,U.User_Email,Q.*,T.Topic_Name  from Quiz_Transaction_mst QZ join Questions_mst Q ON  Q.Ques_ID=QZ.Ques_ID Join Users_mst U ON U.User_ID=QZ.User_ID  join Topics_mst T On T.Topic_ID=Q.Topic_ID   ";

            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();
            if (param.TryGetValue("Username", out string Username))
            {
                filter.Add("  U.User_Name = @Username");
                sqlparams.Add("@Username", Username);
            }
            if (param.TryGetValue("Email", out string Email))
            {
                filter.Add("  U.User_Email = @Email");
                sqlparams.Add("@Email", Email);
            }

            if (param.TryGetValue("startDate", out string startDateStr) && DateTime.TryParse(startDateStr, out DateTime startDate))
            {
                filter.Add("QZ.Answer_Date >= @StartDate");
                sqlparams.Add("@StartDate", startDate);
            }

            if (param.TryGetValue("endDate", out string endDateStr) && DateTime.TryParse(endDateStr, out DateTime endDate))
            {
                filter.Add("QZ.Answer_Date <= @EndDate");
                sqlparams.Add("@EndDate", endDate);
            }
            if (filter.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filter);
            }


            DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);


            var Ansewers_List = new List<Quiz_TransactionModel>();
            foreach (DataRow row in Table.Rows)
            {
                Ansewers_List.Add(new Quiz_TransactionModel
                {
                    Answer_ID = Convert.ToInt32(row["Answer_ID"]),
                    Ques_ID = Convert.ToInt32(row["Ques_ID"]),
                    User_ID = Convert.ToInt32(row["User_ID"]),
                    Answer = row["Answer"].ToString(),
                    Answer_Date = row["Answer_Date"].ToString(),
                    Ques_Desc = row["Ques_Desc"].ToString(),
                    Opt_A = row["Opt_A"].ToString(),
                    Opt_B = row["Opt_B"].ToString(),
                    Opt_C = row["Opt_C"].ToString(),
                    Opt_D = row["Opt_D"].ToString(),
                    Correct_Answer = row["Correct_Answer"].ToString(),
                    Status = row["Status"].ToString(),
                    Topic_Name = row["Topic_Name"].ToString(),
                    Topic_ID = Convert.ToInt32(row["Topic_ID"]),
                    User_Name = row["User_Name"].ToString(),

                    User_Email = row["User_Email"].ToString()



                });
            }
            return Ok(Ansewers_List);

        }

        [HttpPost]
        [Route("SubmitAnswer")]
        public IActionResult SubmitAnswer([FromBody] List<Quiz_TransactionModel> quizList)
        {
            if (quizList == null || quizList.Count == 0)
            {
                return BadRequest("No answers to submit.");
            }

            try
            {
                string insertQuery = "INSERT INTO Quiz_Transaction_mst (Ques_ID, User_ID, Answer) VALUES ";

                List<string> valueRows = new List<string>();
                foreach (var quiz in quizList)
                {
                    valueRows.Add($"({quiz.Ques_ID}, {quiz.User_ID}, {quiz.Ques_ID})");
                }

                insertQuery += string.Join(", ", valueRows);

               _connection.ExecuteQueryWithResult(insertQuery);

                return Ok("Answers Submitted Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

    }
}