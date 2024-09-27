using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
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
            DateTime currentDate = DateTime.Now;

            string query = @"
            SELECT 
                QT.*, Q.*, U.*, T.*, 
                CASE 
                    WHEN @currentDate BETWEEN QT.Quiz_Date AND DATEADD(MINUTE, QT.Allowed_Time, QT.Quiz_Date) THEN 1 
                    ELSE 0 
                END AS IsAllowed 
            FROM 
                Quiz_Transaction_mst QT 
            JOIN 
                Questions_mst Q ON QT.Ques_ID = Q.Ques_ID 
            JOIN 
                Users_mst U ON QT.User_ID = U.User_ID 
            JOIN 
                Topics_mst T ON Q.Topic_ID = T.Topic_ID 
            LEFT JOIN 
                Quiz_AnsTransaction_mst QA ON QT.Ques_ID = QA.Ques_ID AND QT.User_ID = QA.User_ID";


            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>
        {
            { "@currentDate", currentDate }
        };


            if (param.TryGetValue("quizDate", out string QuizDate) && DateTime.TryParse(QuizDate, out DateTime quizDate))
            {
                filter.Add("QT.Quiz_Date = @quizDate");
                sqlparams.Add("@quizDate", quizDate);
            }

            //{
            //    filter.Add("U.User_Email = @Email");
            //    sqlparams.Add("@Email", Email);
            //}

            // Handling user filter if provided
            if (param.TryGetValue("User_ID", out string User_ID))
            {
                filter.Add("U.User_ID = @User_ID");
                sqlparams.Add("@User_ID", User_ID);
            }

            // Exclude questions that have already been answered
            filter.Add("QA.Answer_ID IS NULL");

            // Apply filters if any exist
            if (filter.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filter);
            }

            // Sorting logic
            if (param.TryGetValue("sort", out string sortOrder))
            {
                switch (sortOrder.ToLower())
                {
                    case "random":
                        query += " "; // Random sorting
                        break;
                    case "topic":
                        query += " ORDER BY T.Topic_Name"; // Sort by topic name
                        break;
                    default:
                        break;
                }
            }

            // Default sorting by Quiz Date descending
            query += " ORDER BY QT.Quiz_Date DESC";

            // Execute the query
            DataTable table = _connection.ExecuteQueryWithResults(query, sqlparams);

            // Convert result to list of QuizTransactionModel
            var quesList = new List<QuizTransactionModel>();
            foreach (DataRow row in table.Rows)
            {
                bool isAllowed = Convert.ToBoolean(row["IsAllowed"]);
                quesList.Add(new QuizTransactionModel
                {
                    Quiz_ID = Convert.ToInt32(row["Quiz_ID"]),
                    User_ID = Convert.ToInt32(row["User_ID"]),
                    Ques_ID = Convert.ToInt32(row["Ques_ID"]),
                    Ques_Desc = row["Ques_Desc"].ToString(),
                    Opt_A = row["Opt_A"].ToString(),
                    Opt_B = row["Opt_B"].ToString(),
                    Opt_C = row["Opt_C"].ToString(),
                    Opt_D = row["Opt_D"].ToString(),
                    Quiz_Date = Convert.ToDateTime(row["Quiz_Date"]).ToString(),
                    User_Email = row["User_Email"].ToString(),
                    Topic_Name = row["Topic_Name"].ToString(),
                    Quiz_Name = row["Quiz_Name"].ToString(),
                    IsAllowed = isAllowed // Display only if within allowed time
                });
            }

            return Ok(quesList);
        }


        //[HttpGet]
        //[Route("GetAllQuizQuestion")]
        //public IActionResult GetAllQuizQuestion([FromQuery] IDictionary<string, string> param)
        //{
        //    DateTime currentDate = DateTime.Now;

        //    string query = "SELECT QT.*, Q.*, U.*, T.*, " +
        //                   "(CASE " +
        //                   "WHEN @currentDate BETWEEN QT.Quiz_Date AND DATEADD(MINUTE, QT.Allowed_Time, QT.Quiz_Date) THEN 1 " +
        //                   "ELSE 0 " +
        //                   "END) AS IsAllowed " +
        //                   "FROM Quiz_Transaction_mst QT " +
        //                   "JOIN Questions_mst Q ON QT.Ques_ID = Q.Ques_ID " +
        //                   "JOIN Users_mst U ON QT.User_ID = U.User_ID " +
        //                   "JOIN Topics_mst T ON Q.Topic_ID = T.Topic_ID " +
        //                   "LEFT JOIN Quiz_AnsTransaction_mst QA ON QT.Ques_ID = QA.Ques_ID AND QT.User_ID = QA.User_ID ";

        //    List<string> filter = new List<string>();
        //    Dictionary<string, object> sqlparams = new Dictionary<string, object>
        //{
        //    { "@currentDate", currentDate }
        //};

        //    if (param.TryGetValue("quizDate", out string QuizDate) && DateTime.TryParse(QuizDate, out DateTime quizDate))
        //    {
        //        filter.Add("QT.Quiz_Date = @quizDate");
        //        sqlparams.Add("@quizDate", quizDate);
        //    }

        //    if (param.TryGetValue("Email", out string Email))
        //    {
        //        filter.Add("U.User_Email = @Email");
        //        sqlparams.Add("@Email", Email);
        //    }

        //    if (param.TryGetValue("User_ID", out string User_ID))
        //    {
        //        filter.Add("U.User_ID = @User_ID");
        //        sqlparams.Add("@User_ID", User_ID);
        //    }

        //    filter.Add("QA.Answer_ID IS NULL");

        //    if (filter.Count > 0)
        //    {
        //        query += " WHERE " + string.Join(" AND ", filter);
        //    }

        //    if (param.TryGetValue("sort", out string sortOrder))
        //    {
        //        switch (sortOrder.ToLower())
        //        {
        //            case "random":
        //                query += " ";
        //                break;
        //            case "topic":
        //                query += " ORDER BY T.Topic_Name";
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //    query += " Order By QT.Quiz_Date DESC ";
        //    DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);
        //    var QuesList = new List<QuizTransactionModel>();

        //    foreach (DataRow row in Table.Rows)
        //    {
        //        bool isAllowed = Convert.ToBoolean(row["IsAllowed"]);
        //        QuesList.Add(new QuizTransactionModel
        //        {
        //            Quiz_ID = Convert.ToInt32(row["Quiz_ID"]),
        //            User_ID = Convert.ToInt32(row["User_ID"]),
        //            Ques_ID = Convert.ToInt32(row["Ques_ID"]),
        //            Ques_Desc = row["Ques_Desc"].ToString(),
        //            Opt_A = row["Opt_A"].ToString(),
        //            Opt_B = row["Opt_B"].ToString(),
        //            Opt_C = row["Opt_C"].ToString(),
        //            Opt_D = row["Opt_D"].ToString(),
        //            Quiz_Date = row["Quiz_Date"].ToString(),
        //            User_Email = row["User_Email"].ToString(),
        //            Topic_Name = row["Topic_Name"].ToString(),
        //            Quiz_Name = row["Quiz_Name"].ToString(),
        //            IsAllowed = isAllowed ? true : false
        //        });
        //    }

        //    return Ok(QuesList);
        //}


        [HttpPost]
        [Route("AddQuizTransaction")]
        public IActionResult AddQuizTransaction([FromBody] List<QuizTransactionModel> quizList)
        {
            if (quizList == null || quizList.Count == 0)
            {
                return Ok("No transaction.");
            }

            try
            {
                var connection = new LkDataConnection.Connection();

                if (Request.Headers.ContainsKey("Proceed") && Request.Headers["Proceed"] == "true")
                {
                    string insertQuery = "INSERT INTO Quiz_Transaction_mst (Ques_ID, Quiz_Date, User_ID, Allowed_Time, Quiz_Name) VALUES ";
                    List<string> valueRows = new List<string>();

                    foreach (var quiz in quizList)
                    {
                        valueRows.Add($"('{quiz.Ques_ID}', '{quiz.Quiz_Date}', '{quiz.User_ID}', {quiz.Allowed_Time}, '{quiz.Quiz_Name}')");
                    }

                    insertQuery += string.Join(", ", valueRows);
                    connection.bindmethod(insertQuery);

                    return Ok("Quiz_Transaction_mst Submitted Successfully");
                }

                var duplicates = new List<string>();

                foreach (var quiz in quizList)
                {
                    string duplicateQueryCheck = $@"SELECT COUNT(*) FROM Quiz_Transaction_mst 
                                             WHERE User_ID = {quiz.User_ID} 
                                             AND Ques_ID = {quiz.Ques_ID} 
                                             AND Quiz_Name = '{quiz.Quiz_Name}'";

                    int duplicateCount = Convert.ToInt32(_connection.ExecuteScalar(duplicateQueryCheck));
                    _connection.GetSqlConnection().Close();

                    if (duplicateCount > 0)
                    {
                        duplicates.Add($"User_ID: {quiz.User_ID}, Ques_ID: {quiz.Ques_ID}, Quiz_Name: {quiz.Quiz_Name}");
                    }
                }

                if (duplicates.Count > 0)
                {
                    string duplicateMessage = "Quiz already present in the QuizBank for the following entries:" + string.Join(" ", duplicates) +
                                              "Do you want to add more questions?";
                    return Ok(new { message = duplicateMessage, canProceed = true });
                }

                string insertQueryWithoutDuplicates = "INSERT INTO Quiz_Transaction_mst (Ques_ID, Quiz_Date, User_ID, Allowed_Time, Quiz_Name) VALUES ";
                List<string> valueRowsWithoutDuplicates = new List<string>();

                foreach (var quiz in quizList)
                {
                    valueRowsWithoutDuplicates.Add($"('{quiz.Ques_ID}', '{quiz.Quiz_Date}', '{quiz.User_ID}', {quiz.Allowed_Time}, '{quiz.Quiz_Name}')");
                }

                insertQueryWithoutDuplicates += string.Join(", ", valueRowsWithoutDuplicates);
                connection.bindmethod(insertQueryWithoutDuplicates);

                return Ok("Quiz_Transaction_mst Submitted Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }



        [HttpPost]
        [Route("CopyQuizTransaction/{quizName}")]
        public IActionResult CopyQuizTransaction(string quizName, [FromBody] List<QuizTransactionModel> quizList)
        {
            if (quizList == null || quizList.Count == 0)
            {
                return Ok("No transaction.");
            }

            try
            {
                var connection = new LkDataConnection.Connection();

                string fetchQuestionsQuery = $@"
            SELECT Ques_ID FROM Quiz_Transaction_mst 
            WHERE Quiz_Name = '{quizName}'";

                var questions = _connection.ExecuteQueryWithResult(fetchQuestionsQuery);

                if (questions == null || questions.Rows.Count == 0)
                {
                    return Ok("No questions found for the given quiz name.");
                }

                foreach (var quiz in quizList)
                {
                    foreach (DataRow row in questions.Rows)
                    {
                        int quesId = Convert.ToInt32(row["Ques_ID"]);


                        string duplicateQueryCheck = $@"SELECT * FROM Quiz_Transaction_mst WHERE (User_ID = {quiz.User_ID} AND Ques_ID = {quesId} AND  Quiz_Name = '{quiz.Quiz_Name}' )";
                        DataTable duplicateTable = _connection.ExecuteQueryWithResult(duplicateQueryCheck);
                        if(duplicateTable.Rows.Count>0)
                        {
                            continue;
                        }

                        string insertQuery = $@"
                    INSERT INTO Quiz_Transaction_mst (Ques_ID, Quiz_Date, User_ID, Allowed_Time, Quiz_Name) 
                    VALUES ('{quesId}', '{quiz.Quiz_Date}', '{quiz.User_ID}', {quiz.Allowed_Time}, '{quiz.Quiz_Name}')";

                        connection.bindmethod(insertQuery);
                    }
                }

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

        [HttpGet]
        [Route("willBeQuiz/{User_ID}")]
        public IActionResult willBeQuiz( int User_ID)
        {
            try
            {
                DateTime currentDate = DateTime.Now;


                //string query = $"  SELECT    Q.Quiz_Date,    Q.Quiz_Name,   Q.Allowed_Time,   CASE       WHEN '{currentDate}' BETWEEN Q.Quiz_Date AND DATEADD(MINUTE, Q.Allowed_Time, Q.Quiz_Date) THEN 1       ELSE 0    END AS IsAllowed FROM    Quiz_Transaction_mst Q JOIN    Users_mst U ON Q.User_ID = U.User_ID WHERE    Q.Quiz_Date >= CAST(GETDATE() AS DATE)    AND Q.Quiz_Date < DATEADD(DAY, 30, CAST(GETDATE() AS DATE))    AND U.User_ID = {User_ID} AND Q.Quiz_Date > '{currentDate}' GROUP BY    Q.Quiz_Date, Q.Quiz_Name, Q.Allowed_Time  ORDER BY    Q.Quiz_Date; ";

                //string query = $"  SELECT    Q.Quiz_Date,    Q.Quiz_Name,   Q.Allowed_Time,   CASE       WHEN '{currentDate}' BETWEEN Q.Quiz_Date AND DATEADD(MINUTE, Q.Allowed_Time, Q.Quiz_Date) THEN 1       ELSE 0    END AS IsAllowed FROM    Quiz_Transaction_mst Q JOIN    Users_mst U ON Q.User_ID = U.User_ID WHERE    Q.Quiz_Date >= CAST(GETDATE() AS DATE)    AND Q.Quiz_Date < DATEADD(DAY, 30, CAST(GETDATE() AS DATE))    AND U.User_ID = {User_ID} GROUP BY    Q.Quiz_Date, Q.Quiz_Name, Q.Allowed_Time ORDER BY    Q.Quiz_Date; ";

                string query = $@"
    SELECT 
      Distinct(Q.Quiz_Date) , 
        Q.Quiz_Name, 
        Q.Allowed_Time, 
        CASE
            WHEN '{currentDate}' BETWEEN Q.Quiz_Date AND DATEADD(MINUTE, Q.Allowed_Time, Q.Quiz_Date) THEN 1
            ELSE 0 
        END AS IsAllowed
    FROM 
        Quiz_Transaction_mst Q 
    JOIN 
        Users_mst U ON Q.User_ID = U.User_ID
    WHERE 
        Q.Quiz_Date >= CAST(GETDATE() AS DATE)
        AND Q.Quiz_Date < DATEADD(DAY, 30, CAST(GETDATE() AS DATE))
        AND U.User_ID = {User_ID}
        AND (Q.Quiz_Date > '{currentDate}' OR DATEADD(MINUTE, Q.Allowed_Time, Q.Quiz_Date) > '{currentDate}')  
    ORDER BY 
        Q.Quiz_Date;
";
                DataTable Table = _connection.ExecuteQueryWithResult(query);

                var willBeQuiz = new List<QuizTransactionModel>();
                foreach (DataRow row in Table.Rows)
                {
                    bool isAllowed = Convert.ToBoolean(row["IsAllowed"]);

                    var quiz = new QuizTransactionModel
                    {

                        Quiz_Date = row["Quiz_Date"].ToString(),
                        Quiz_Name = row["Quiz_Name"].ToString(),
                        IsAllowed = isAllowed ? true : false


                    };

                    willBeQuiz.Add(quiz);
                }

                return Ok(willBeQuiz);
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


        [HttpGet]
        [Route("upcomingQuiz")]
        public IActionResult GetUpcomingQuizzes([FromQuery] IDictionary<string, string> param)
        {
            try
            {
                string query = $" SELECT  Distinct(Q.Quiz_Date) ,Q.Quiz_Name,U.User_ID FROM Quiz_Transaction_mst Q join Users_mst U ON Q.User_ID = U.User_ID   WHERE Quiz_Date >= GETDATE()   AND Quiz_Date <= DATEADD(DAY, 30, GETDATE())  ";
                List<string> filter = new List<string>();
                Dictionary<string, object> sqlparams = new Dictionary<string, object>();

                if (param.TryGetValue("User_ID", out string User_ID))
                {
                    filter.Add("U.User_ID = @User_ID");
                    sqlparams.Add("@User_ID", User_ID);
                }
                if (filter.Count > 0)
                {
                    query += " AND " + string.Join(" ", filter);
                }
                query += " ORDER BY Q.Quiz_Date ";
                DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);

                var upcomingQuizzes = new List<QuizTransactionModel>();
                //DataTable Table = _connection.ExecuteQueryWithResult(query);

                //          var upcomingQuizzes = new List<QuizTransactionModel>();
                foreach (DataRow row in Table.Rows)
                {
                    var quiz = new QuizTransactionModel
                    {
                       
                        Quiz_Date = row["Quiz_Date"].ToString(),
                        Quiz_Name = row["Quiz_Name"].ToString(),
                        User_ID = Convert.ToInt32(row["User_ID"])


                    };

                    upcomingQuizzes.Add(quiz);
                }

                return Ok(upcomingQuizzes);
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("GetQuizDates")]
        public IActionResult GetQuizDates()
        {
            try
            {
                string query = $"SELECT  Distinct(Quiz_Date) FROM Quiz_Transaction_mst where  Quiz_Date <= GETDATE() ORDER BY Quiz_Date";

                DataTable Table = _connection.ExecuteQueryWithResult(query);

                var QuizDates = new List<QuizTransactionModel>();
                foreach (DataRow row in Table.Rows)
                {
                    var quiz = new QuizTransactionModel
                    {

                        Quiz_Date = row["Quiz_Date"].ToString(),

                    };

                    QuizDates.Add(quiz);
                }

                return Ok(QuizDates);
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


        [HttpDelete]
        [Route("deleteQuizTransaction/{id}")]
        public IActionResult deleteQuizTransaction(int id)
        {
            try
            {
                string checkQuery = $"SELECT Quiz_Date FROM Quiz_Transaction_mst WHERE Quiz_ID = {id}";
                var quizDate = (DateTime)_connection.ExecuteScalar(checkQuery);

                DateTime currentDateTime = DateTime.UtcNow;

                if (currentDateTime > quizDate)
                {
                    return Ok("Cannot delete the quiz because it has expired.");
                }

                string deleteQuery = $"DELETE FROM Quiz_Transaction_mst WHERE Quiz_ID ={id}";
                LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);

                return Ok("Quiz_Transaction deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

       

    }
   
}

