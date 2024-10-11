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






        [HttpGet]
        [Route("GetAllQuizQuestion")]
        public IActionResult GetAllQuizQuestion([FromQuery] IDictionary<string, string> param)
        {
            string query = "SELECT QT.*, Q.*, U.*, T.* " +
                           "FROM Quiz_Transaction_mst QT " +
                           "JOIN Questions_mst Q ON QT.Ques_ID = Q.Ques_ID " +
                           "JOIN Users_mst U ON QT.User_ID = U.User_ID " +
                           "JOIN Topics_mst T ON Q.Topic_ID = T.Topic_ID " +
                           "LEFT JOIN Quiz_AnsTransaction_mst QA ON QT.Ques_ID = QA.Ques_ID AND QT.User_ID = QA.User_ID ";

            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();

            if (param.TryGetValue("quizDate", out string QuizDate) && DateTime.TryParse(QuizDate, out DateTime quizDate))
            {
                DateTime currentDate = DateTime.Now;
                filter.Add("QT.Quiz_Date = @quizDate");
                sqlparams.Add("@quizDate", quizDate);

                filter.Add("QT.Quiz_Date = @currentDate");
                sqlparams.Add("@currentDate", currentDate.Date);
            }

            if (param.TryGetValue("Email", out string Email))
            {
                filter.Add("U.User_Email = @Email");
                sqlparams.Add("@Email", Email);
            }

            if (param.TryGetValue("User_ID", out string User_ID))
            {
                filter.Add("U.User_ID = @User_ID");
                sqlparams.Add("@User_ID", User_ID);
            }

            filter.Add("QA.Answer_ID IS NULL");

            if (filter.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filter);
            }

            if (param.TryGetValue("sort", out string sortOrder))
            {
                switch (sortOrder.ToLower())
                {
                    case "random":
                        query += " "; 
                        break;
                    case "topic":
                        query += " ORDER BY T.Topic_Name"; 
                        break;
                    default:
                        break;
                }
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
                    Quiz_Date = Convert.ToDateTime(row["Quiz_Date"]),
                    User_Email = row["User_Email"].ToString(),
                    Topic_Name = row["Topic_Name"].ToString(),
                    Quiz_Name = row["Quiz_Name"].ToString()
                });
            }

            return Ok(QuesList);
        }


        //[HttpGet]
        //[Route("QuizAnsTransaction")]
        //public IActionResult GetQuestionBasedOnTopic([FromQuery] IDictionary<string, string> param)

        //{
        //    string query = $"select QZ.*,U.User_Name,U.User_Email,Q.*,T.Topic_Name  from Quiz_AnsTransaction_mst QZ join Questions_mst Q ON  Q.Ques_ID=QZ.Ques_ID Join Users_mst U ON U.User_ID=QZ.User_ID  join Topics_mst T On T.Topic_ID=Q.Topic_ID   ";

        //    List<string> filter = new List<string>();
        //    Dictionary<string, object> sqlparams = new Dictionary<string, object>();
        //    if (param.TryGetValue("Username", out string Username))
        //    {
        //        filter.Add("  U.User_Name = @Username");
        //        sqlparams.Add("@Username", Username);
        //    }
        //    if (param.TryGetValue("Email", out string Email))
        //    {
        //        filter.Add("  U.User_Email = @Email");
        //        sqlparams.Add("@Email", Email);
        //    }


        //    if (param.TryGetValue("startDate", out string startDateStr) && DateTime.TryParse(startDateStr, out DateTime startDate))
        //    {
        //        filter.Add("QZ.Answer_Date >= @StartDate");
        //        sqlparams.Add("@StartDate", startDate);
        //    }

        //    if (param.TryGetValue("endDate", out string endDateStr) && DateTime.TryParse(endDateStr, out DateTime endDate))
        //    {
        //        filter.Add("QZ.Answer_Date <= @EndDate");
        //        sqlparams.Add("@EndDate", endDate);
        //    }
        //    if (filter.Count > 0)
        //    {
        //        query += " WHERE " + string.Join(" AND ", filter);
        //    }


        //    DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);


        //    var Ansewers_List = new List<Quiz_AnsTransactionModel>();
        //    foreach (DataRow row in Table.Rows)
        //    {
        //        Ansewers_List.Add(new Quiz_AnsTransactionModel
        //        {
        //            Answer_ID = Convert.ToInt32(row["Answer_ID"]),
        //            Ques_ID = Convert.ToInt32(row["Ques_ID"]),
        //            User_ID = Convert.ToInt32(row["User_ID"]),
        //            Answer = row["Answer"].ToString(),
        //            Answer_Date = row["Answer_Date"].ToString(),
        //            Ques_Desc = row["Ques_Desc"].ToString(),
        //            Opt_A = row["Opt_A"].ToString(),
        //            Opt_B = row["Opt_B"].ToString(),
        //            Opt_C = row["Opt_C"].ToString(),
        //            Opt_D = row["Opt_D"].ToString(),
        //            Correct_Answer = row["Correct_Answer"].ToString(),
        //            Status = row["Status"].ToString(),
        //            Topic_Name = row["Topic_Name"].ToString(),
        //            Topic_ID = Convert.ToInt32(row["Topic_ID"]),
        //            User_Name = row["User_Name"].ToString(),

        //            User_Email = row["User_Email"].ToString()



        //        });
        //    }

        //    string queryDates = $"select  DISTINCT CAST(Quiz_Date AS DATE) AS Quiz_Date  FROM Quiz_Transaction_mst Order By Quiz_Date ";
        //    DataTable queryDatesTable = _connection.ExecuteQueryWithResult(queryDates);


        //    var DatesList = new List<Quiz_AnsTransactionModel>();
        //    foreach (DataRow row in queryDatesTable.Rows)
        //    {
        //        DatesList.Add(new Quiz_AnsTransactionModel
        //        {

        //            Quiz_Date = row["Quiz_Date"].ToString(),





        //        });
        //    }
        //    return Ok( new {
        //        Ansewer_List = Ansewers_List,
        //        DateList = DatesList
        //    });

        //}

       // [AllowAnonymous]
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
                    string checkQuery = $"SELECT Quiz_Date, Allowed_Time FROM Quiz_Transaction_mst WHERE Ques_ID = {quiz.Ques_ID} AND User_ID = {quiz.User_ID} AND Quiz_Date = '{quiz.Quiz_Date}' ";

                    //string checkQuery = $"SELECT Quiz_Date, Allowed_Time FROM Quiz_Transaction_mst WHERE Ques_ID = {quiz.Ques_ID} AND User_ID = {quiz.User_ID} AND Quiz_Date='{quiz.Quiz_Date}'  ";
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
                        var checkDuplicacy = $"SELECT * FROM Quiz_AnsTransaction_mst WHERE (Ques_ID = {quiz.Ques_ID} AND User_ID = {quiz.User_ID} AND Quiz_Name='{quiz.Quiz_Name}'   )";

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

                string insertQuery = "INSERT INTO Quiz_AnsTransaction_mst (Ques_ID, User_ID, Answer,Quiz_Name,Answer_Date) VALUES ";
                List<string> valueRows = new List<string>();

                foreach (var quiz in quizList)
                {
                    valueRows.Add($"({quiz.Ques_ID}, {quiz.User_ID}, '{quiz.Answer}','{quiz.Quiz_Name}','{quiz.Quiz_Date}')");
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

        //================================================TotalQuizAnsTransaction===============================
        [HttpGet]
        [Route("TotalQuizAnsTransaction")]
        public IActionResult TotalQuizAnsTransaction()
        {
            //string query = $"  SELECT COUNT(DISTINCT Answer_Date) AS TotalTests FROM Quiz_AnsTransaction_mst;";
            string query = $" SELECT  COUNT(DISTINCT(Answer_Date)) AS TotalTests FROM Quiz_AnsTransaction_mst;";

            //var connection
            //= new LkDataConnection.Connection();

            //var result = connection.bindmethod(query);
            //DataTable Table = result._DataTable;
            DataTable Table = _connection.ExecuteQueryWithResult(query);

            if (Table.Rows.Count > 0)
            {
                int totalTest = Convert.ToInt32(Table.Rows[0]["TotalTests"]);
                return Ok(new { TotalTest = totalTest });
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
        //[HttpGet]
        //[Route("Result")]
        //public IActionResult UsersResult([FromQuery] IDictionary<string, string> param)
        //{
        //    string query = @"SELECT at.Answer_ID, at.Answer_Date, at.Ques_ID, at.User_ID, at.Answer, 
        //                        q.Ques_Desc, q.Correct_Answer, U.User_Name, QZ.Quiz_Date, QZ.Quiz_Name, 
        //                        CASE WHEN at.Answer = q.Correct_Answer THEN 'Correct' ELSE 'Incorrect' END AS Result 
        //                 FROM Quiz_AnsTransaction_mst at
        //                 JOIN Questions_mst q ON at.Ques_ID = q.Ques_ID
        //                 JOIN Users_mst U ON at.User_ID = U.User_ID
        //                 JOIN Quiz_Transaction_mst QZ ON at.Ques_ID = QZ.Ques_ID";

        //    List<string> filter = new List<string>();
        //    Dictionary<string, object> sqlparams = new Dictionary<string, object>();

        //    if (param.TryGetValue("User_ID", out string User_ID))
        //    {
        //        filter.Add("U.User_ID = @User_ID");
        //        sqlparams.Add("@User_ID", User_ID);
        //    }

        //    if (param.TryGetValue("QZ.Quiz_Name", out string Quiz_Name))
        //    {
        //        filter.Add("QZ.Quiz_Name = @Quiz_Name");
        //        sqlparams.Add("@Quiz_Name", Quiz_Name);
        //    }

        //    if (param.TryGetValue("Quiz_Date", out string Quiz_Date))
        //    {
        //        if (DateTime.TryParse(Quiz_Date, out DateTime quizDateValue))
        //        {
        //            filter.Add("CAST(QZ.Quiz_Date AS DATE) = @Quiz_Date");
        //            sqlparams.Add("@Quiz_Date", quizDateValue.ToString("yyyy-MM-dd"));
        //        }
        //        else
        //        {
        //            return Ok("Invalid Quiz_Date format. Please provide a valid date.");
        //        }
        //    }

        //    if (filter.Count > 0)
        //    {
        //        query += " WHERE " + string.Join(" AND ", filter);
        //    }

        //    query += " ORDER BY QZ.Quiz_Date";

        //    DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);

        //    var AnsList = new List<Quiz_AnsTransactionModel>();
        //    int correctAns = 0;
        //    int totalAnswers = Table.Rows.Count;


        //    string totalQuestionsQuery = @" SELECT COUNT( at.Ques_ID) AS TotalQuestions 
        //FROM Quiz_AnsTransaction_mst at
        //JOIN Users_mst U ON at.User_ID = U.User_ID 
        //JOIN Quiz_Transaction_mst QZ ON at.Ques_ID = QZ.Ques_ID";
        //    if (filter.Count > 0)
        //    {
        //        totalQuestionsQuery += " WHERE " + string.Join(" AND ", filter);
        //    }

        //    DataTable totalQuestionsTable = _connection.ExecuteQueryWithResults(totalQuestionsQuery, sqlparams);
        //    int totalQuestions = Convert.ToInt32(totalQuestionsTable.Rows[0]["TotalQuestions"]);


        //    foreach (DataRow row in Table.Rows)
        //    {
        //        string result = row["Result"].ToString();
        //        if (result == "Correct")
        //        {
        //            correctAns++;
        //        }

        //        AnsList.Add(new Quiz_AnsTransactionModel
        //        {
        //            User_Name = row["User_Name"].ToString(),
        //            Ques_Desc = row["Ques_Desc"].ToString(),
        //            Answer = row["Answer"].ToString(),
        //            Correct_Answer = row["Correct_Answer"].ToString(),
        //            Quiz_Date = row["Quiz_Date"].ToString(),
        //            Result = result,
        //            Quiz_Name = row["Quiz_Name"].ToString(),
        //            User_ID = Convert.ToInt32(row["User_ID"]),
        //        });
        //    }

        //    var score = new
        //    {
        //        CorrectAnswer = correctAns,
        //        TotalQuestions = totalQuestions,
        //    };

        //    return Ok(new { ResultList = AnsList, ScoreResult = score });
        //}


        [HttpGet]
        [Route("Result")]
        public IActionResult UsersResult([FromQuery] IDictionary<string, string> param)
        {
            string query = @"SELECT at.Answer_ID, at.Answer_Date, at.Ques_ID, at.User_ID, at.Answer, 
               q.Ques_Desc, q.Correct_Answer, U.User_Name, QZ.Quiz_Date, QZ.Quiz_Name, 
               CASE WHEN at.Answer = q.Correct_Answer THEN 'Correct' ELSE 'Incorrect' END AS Result 
        FROM Quiz_AnsTransaction_mst at
        JOIN Questions_mst q ON at.Ques_ID = q.Ques_ID
        JOIN Users_mst U ON at.User_ID = U.User_ID
        JOIN Quiz_Transaction_mst QZ ON at.Ques_ID = QZ.Ques_ID
        WHERE at.Answer_ID IS NOT NULL 
          AND at.Answer_Date IS NOT NULL
          AND QZ.Quiz_Name = at.Quiz_Name  
          AND at.User_ID = QZ.User_ID     
          AND at.Ques_ID = QZ.Ques_ID     ";

            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();

            if (param.TryGetValue("User_ID", out string User_ID))
            {
                filter.Add("U.User_ID = @User_ID");
                sqlparams.Add("@User_ID", User_ID);
            }

            if (param.TryGetValue("QZ.Quiz_Name", out string Quiz_Name))
            {
                filter.Add("QZ.Quiz_Name = @Quiz_Name");
                sqlparams.Add("@Quiz_Name", Quiz_Name);
            }

            if (param.TryGetValue("Quiz_Date", out string Quiz_Date))
            {
                if (DateTime.TryParse(Quiz_Date, out DateTime quizDateValue))
                {
                    filter.Add("CAST(QZ.Quiz_Date AS DATE) = @Quiz_Date");
                    sqlparams.Add("@Quiz_Date", quizDateValue.ToString("yyyy-MM-dd"));
                }
                else
                {
                    return Ok("Invalid Quiz_Date format. Please provide a valid   date.");
                }
            }


            if (filter.Count > 0)
            {
                query += " AND " + string.Join(" AND ", filter);
            }

            query += " ORDER BY QZ.Quiz_Date";

            DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);

            var AnsList = new List<Quiz_AnsTransactionModel>();
            int correctAns = 0;
            int totalAnswers = Table.Rows.Count;




            foreach (DataRow row in Table.Rows)
            {
                string result = row["Result"].ToString();
                if (result == "Correct")
                {
                    correctAns++;
                }

                AnsList.Add(new Quiz_AnsTransactionModel
                {
                    User_Name = row["User_Name"].ToString(),
                    Ques_Desc = row["Ques_Desc"].ToString(),
                    Answer = row["Answer"].ToString(),
                    Correct_Answer = row["Correct_Answer"].ToString(),
                    Quiz_Date = row["Quiz_Date"].ToString(),
                    Result = result,
                    Quiz_Name = row["Quiz_Name"].ToString(),
                    User_ID = Convert.ToInt32(row["User_ID"]),
                });
            }

            var score = new
            {
                CorrectAnswer = correctAns,
                TotalQuestions = totalAnswers,
            };

            return Ok(new { ResultList = AnsList, ScoreResult = score });
        }


    }
}
