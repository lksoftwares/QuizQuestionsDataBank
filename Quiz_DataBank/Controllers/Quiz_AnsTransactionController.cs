using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Quiz_DataBank.Classes;
using Quiz_DataBank.Model;
using System.Data;
using System.Net.Security;

namespace Quiz_DataBank.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class Quiz_AnsTransactionController : ControllerBase
    {
        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public Quiz_AnsTransactionController(Quiz_DataBank.Classes.Connection connection)
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
        [Route("QuizAnsTransaction")]
        public IActionResult GetQuestionBasedOnTopic([FromQuery] IDictionary<string, string> param)

        {
            string query = $"select QZ.*,U.User_Name,U.User_Email,Q.*,T.Topic_Name  from Quiz_AnsTransaction_mst QZ join Questions_mst Q ON  Q.Ques_ID=QZ.Ques_ID Join Users_mst U ON U.User_ID=QZ.User_ID  join Topics_mst T On T.Topic_ID=Q.Topic_ID   ";

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


            var Ansewers_List = new List<Quiz_AnsTransactionModel>();
            foreach (DataRow row in Table.Rows)
            {
                Ansewers_List.Add(new Quiz_AnsTransactionModel
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

        //[HttpPost]
        //[Route("SubmitAnswer")]
        //public IActionResult SubmitAnswer([FromBody] List<Quiz_AnsTransactionModel> quizList)
        //{
        //    if (quizList == null || quizList.Count == 0)
        //    {
        //        return Ok("No answers to submit.");
        //    }

        //    try
        //    {



        //        string insertQuery = "INSERT INTO Quiz_AnsTransaction_mst (Ques_ID, User_ID, Answer) VALUES ";

        //        List<string> valueRows = new List<string>();
        //        foreach (var quiz in quizList)
        //        {
        //            valueRows.Add($"({quiz.Ques_ID}, {quiz.User_ID}, '{quiz.Answer}')");
        //        }

        //        insertQuery += string.Join(", ", valueRows);
        //        var connection = new LkDataConnection.Connection();

        //        connection.bindmethod(insertQuery);


        //        return Ok("Answers Submitted Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        //    }
        //}
        //[AllowAnonymous]
        //[HttpPost]
        //[Route("SubmitAnswer")]
        //public IActionResult SubmitAnswer([FromBody] List<Quiz_AnsTransactionModel> quizList)
        //{
        //    if (quizList == null || quizList.Count == 0)
        //    {
        //        return Ok("No answers to submit.");
        //    }


        //    try
        //    {
        //        foreach (var quiz in quizList)
        //        {
        //            string checkQuery = $"SELECT Quiz_Date, Allowed_Time FROM Quiz_Transaction_mst WHERE Ques_ID = {quiz.Ques_ID} AND User_ID = {quiz.User_ID}";
        //            var connection = new LkDataConnection.Connection();
        //            var parameters = new Dictionary<string, object>
        //    {

        //    };

        //            DataTable transactionDetails = _connection.ExecuteQueryWithResults(checkQuery, parameters);

        //            if (transactionDetails.Rows.Count > 0)
        //            {

        //                DateTime quizStartTime = Convert.ToDateTime(transactionDetails.Rows[0]["Quiz_Date"]);
        //                int allowedTimeInMinutes = Convert.ToInt32(transactionDetails.Rows[0]["Allowed_Time"]);


        //                DateTime quizEndTime = quizStartTime.AddMinutes(allowedTimeInMinutes);


        //                if (DateTime.Now > quizEndTime)
        //                {
        //                    return Ok($"Time limit exceeded for Question ID {quiz.Ques_ID}. Answers cannot be submitted.");
        //                }
        //            }
        //            else
        //            {
        //                return StatusCode(StatusCodes.Status404NotFound, $"Quiz transaction not found for Question ID {quiz.Ques_ID} and User ID {quiz.User_ID}.");
        //            }
        //        }


        //        string insertQuery = "INSERT INTO Quiz_AnsTransaction_mst (Ques_ID, User_ID, Answer) VALUES ";
        //        List<string> valueRows = new List<string>();
        //        foreach (var quiz in quizList)
        //        {
        //            valueRows.Add($"({quiz.Ques_ID}, {quiz.User_ID}, '{quiz.Answer}')");
        //        }

        //        insertQuery += string.Join(", ", valueRows);
        //        var connectionInsert = new LkDataConnection.Connection();

        //        connectionInsert.bindmethod(insertQuery);

        //        return Ok("Answers Submitted Successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        //    }
        //}
        [AllowAnonymous]
        [HttpPost]
        [Route("SubmitAnswer")]
        public IActionResult SubmitAnswer([FromBody] List<Quiz_AnsTransactionModel> quizList)
        {
            if (quizList == null || quizList.Count == 0)
            {
                return Ok("No answers to submit.");
            }

            try
            {
                foreach (var quiz in quizList)
                {
                    string checkQuery = $"SELECT Quiz_Date, Allowed_Time FROM Quiz_Transaction_mst WHERE Ques_ID = {quiz.Ques_ID} AND User_ID = {quiz.User_ID}";
                    var connection = new LkDataConnection.Connection();
                    var parameters = new Dictionary<string, object>
                    {
                    };

                    DataTable transactionDetails = _connection.ExecuteQueryWithResults(checkQuery, parameters);

                    if (transactionDetails.Rows.Count > 0)
                    {
                        DateTime quizStartTime = Convert.ToDateTime(transactionDetails.Rows[0]["Quiz_Date"]);
                        int allowedTimeInMinutes = Convert.ToInt32(transactionDetails.Rows[0]["Allowed_Time"]);

                        DateTime quizEndTime = quizStartTime.AddMinutes(allowedTimeInMinutes);

                        if (DateTime.Now > quizEndTime)
                        {
                            return Ok($"Time limit exceeded for Question ID {quiz.Ques_ID}. Answers cannot be submitted.");
                        }
                        var checkDuplicacy = $"SELECT * FROM Quiz_AnsTransaction_mst WHERE (Ques_ID = {quiz.Ques_ID} AND User_ID = {quiz.User_ID})";

                        DataTable result = _connection.ExecuteQueryWithResult(checkDuplicacy);
                        if (result.Rows.Count > 0)
                        {
                            return Ok($"Duplicate submission detected for Question ID {quiz.Ques_ID} and User ID {quiz.User_ID}. Answers cannot be submitted.");

                        }


                    }
                        
                   
                    
                    else
                    {
                        return StatusCode(StatusCodes.Status404NotFound, $"Quiz transaction not found for Question ID {quiz.Ques_ID} and User ID {quiz.User_ID}.");
                    }
                }

                string insertQuery = "INSERT INTO Quiz_AnsTransaction_mst (Ques_ID, User_ID, Answer) VALUES ";
                List<string> valueRows = new List<string>();

                foreach (var quiz in quizList)
                {
                    valueRows.Add($"({quiz.Ques_ID}, {quiz.User_ID}, '{quiz.Answer}')");
                }

                insertQuery += string.Join(", ", valueRows);
                var connectionInsert = new LkDataConnection.Connection();

                connectionInsert.bindmethod(insertQuery);

                return Ok("Answers Submitted Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        //================================================TotalQuizAnsTransaction======================
        [HttpGet]
        [Route("TotalQuizAnsTransaction")]
        public IActionResult TotalQuizAnsTransaction()
        {
            string query = $"  SELECT COUNT(DISTINCT Answer_Date) AS TotalTests FROM Quiz_AnsTransaction_mst;";

            //var connection
            //= new LkDataConnection.Connection();

            //var result = connection.bindmethod(query);
            //DataTable Table = result._DataTable;
            DataTable Table = _connection.ExecuteQueryWithResult(query);

            if (Table.Rows.Count > 0)
            {
                int totalTest = Convert.ToInt32(Table.Rows[0]["TotalTests"]);
                return Ok(new { TotalTest= totalTest });
            }

            return Ok(new { TotalQuestions = 0 });
        }
        [HttpGet]
        [Route("TodayAnsTransaction")]
        public IActionResult TodayAnsTransaction()
        {
            string query = $" SELECT COUNT(DISTINCT User_ID) AS TotalRecordsSubmittedToday   FROM Quiz_AnsTransaction_mst   WHERE CAST(Answer_Date AS DATE) = CAST(GETDATE() AS DATE);";

            //var connection = new LkDataConnection.Connection();

            //var result = connection.bindmethod(query);
            //DataTable Table = result._DataTable;
            DataTable Table = _connection.ExecuteQueryWithResult(query);

            if (Table.Rows.Count > 0)
            {
                int totalTestToday = Convert.ToInt32(Table.Rows[0]["TotalRecordsSubmittedToday"]);
                return Ok(new { TotalTestToday = totalTestToday });
            }

            return Ok(new { TotalRecordsSubmittedToday = 0 });
        }
    
}

}