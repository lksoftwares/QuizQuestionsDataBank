using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Quiz_DataBank.Model;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        /// [AllowAnonymous]
        [HttpGet]
        [Route("GetAllQuizQuestion")]
        public IActionResult GetAllQuizQuestion([FromQuery] IDictionary<string, string> param)
        {
            DateTime currentDate = DateTime.Now;

            bool hasUserId = param.TryGetValue("User_ID", out string User_ID);

            string query = hasUserId ?
            $@"
        SELECT 
            QT.*,
            Q.Ques_Desc,Q.Ques_ID ,
            Q.Opt_A, Q.Opt_B, Q.Opt_C, Q.Opt_D, Q.Correct_Answer, Q.Status, Q.Topic_ID, Q.QuesType_ID, Q.Remarks,
            U.*, 
            T.*, 
            CASE 
                WHEN '{currentDate}' BETWEEN QT.Quiz_Date AND DATEADD(MINUTE, QT.Allowed_Time, QT.Quiz_Date) THEN 1 
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
            Quiz_AnsTransaction_mst QA ON QT.Ques_ID = QA.Ques_ID AND QT.User_ID = QA.User_ID
    " :
            $@"
        SELECT 
            MAX(QT.Quiz_ID) AS Quiz_ID,
            Q.Ques_Desc, Q.Ques_ID ,
            Q.Opt_A, Q.Opt_B, Q.Opt_C, Q.Opt_D, Q.Correct_Answer, Q.Status, Q.Topic_ID, Q.QuesType_ID, Q.Remarks,
            MAX(U.User_ID) AS User_ID, 
            MAX(U.User_Email) AS User_Email,
            MAX(U.User_Name) AS User_Name,

            MAX(T.Topic_Name) AS Topic_Name,
            MAX(QT.Quiz_Name) AS Quiz_Name,
            MAX(QT.Quiz_Date) AS Quiz_Date, 
            CASE 
                WHEN '{currentDate}' BETWEEN MAX(QT.Quiz_Date) AND DATEADD(MINUTE, MAX(QT.Allowed_Time), MAX(QT.Quiz_Date)) THEN 1 
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
            Quiz_AnsTransaction_mst QA ON QT.Ques_ID = QA.Ques_ID AND QT.User_ID = QA.User_ID
    ";

            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();

            if (param.TryGetValue("quizDate", out string QuizDate) && DateTime.TryParse(QuizDate, out DateTime quizDate))
            {
                filter.Add("QT.Quiz_Date = @quizDate");
                sqlparams.Add("@quizDate", quizDate);
            }

            if (hasUserId)
            {
                filter.Add("U.User_ID = @User_ID");
                sqlparams.Add("@User_ID", User_ID);
            }

            if (param.TryGetValue("Quiz_Name", out string Quiz_Name))
            {
                filter.Add("QT.Quiz_Name = @Quiz_Name");
                sqlparams.Add("@Quiz_Name", Quiz_Name);
            }

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

            if (!hasUserId)
            {
                query += @"
            GROUP BY 
               Q.Ques_ID, Q.Ques_Desc, Q.Opt_A, Q.Opt_B, Q.Opt_C, Q.Opt_D, Q.Correct_Answer, Q.Status, Q.Topic_ID, Q.QuesType_ID, Q.Remarks 
            ORDER BY MAX(QT.Quiz_Date) DESC;
        ";
            }
            else
            {
                query += " ORDER BY QT.Quiz_Date DESC"; 
            }

            DataTable table = _connection.ExecuteQueryWithResults(query, sqlparams);

            // Process the results
            var quesList = new List<QuizTransactionModel>();
            foreach (DataRow row in table.Rows)
            {
              
                    DateTime date = Convert.ToDateTime(row["Quiz_Date"]);
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
                        Quiz_Date = date,
                        User_Email = row["User_Email"].ToString(),
                        Topic_Name = row["Topic_Name"].ToString(),
                        User_Name = row["User_Name"].ToString(),
                        Quiz_Name = row["Quiz_Name"].ToString(),
                        IsAllowed = isAllowed
                    });
                
              
            }

            return Ok(quesList);
        }


        //[HttpGet]
        //[Route("GetAllQuizQuestion")]
        //public IActionResult GetAllQuizQuestion([FromQuery] IDictionary<string, string> param)
        //{
        //    DateTime currentDate = DateTime.Now;

        //    string query = $@"
        //    SELECT  
        //   QT.*,
        //  Q.Ques_Desc, Q.Opt_A,Q.Opt_B,Q.Opt_C,Q.Opt_D,Q.Correct_Answer,Q.Status,Q.Topic_ID,Q.QuesType_ID,Q.Remarks,
        //    U.*, 
        //    T.*, 
        //    CASE 
        //        WHEN '{currentDate}' BETWEEN QT.Quiz_Date AND DATEADD(MINUTE, QT.Allowed_Time, QT.Quiz_Date) THEN 1 
        //        ELSE 0 
        //    END AS IsAllowed 
        //FROM 
        //    Quiz_Transaction_mst QT 
        //JOIN 
        //    Questions_mst Q ON QT.Ques_ID = Q.Ques_ID 
        //JOIN 
        //    Users_mst U ON QT.User_ID = U.User_ID 
        //JOIN 
        //    Topics_mst T ON Q.Topic_ID = T.Topic_ID 
        //LEFT JOIN 
        //    Quiz_AnsTransaction_mst QA ON QT.Ques_ID = QA.Ques_ID AND QT.User_ID = QA.User_ID 
        //";

        //    List<string> filter = new List<string>();
        //    Dictionary<string, object> sqlparams = new Dictionary<string, object>();

        //    if (param.TryGetValue("quizDate", out string QuizDate) && DateTime.TryParse(QuizDate, out DateTime quizDate))
        //    {
        //        filter.Add("QT.Quiz_Date = @quizDate");
        //        sqlparams.Add("@quizDate", quizDate);
        //    }

        //    if (param.TryGetValue("User_ID", out string User_ID))
        //    {
        //        filter.Add("U.User_ID = @User_ID");
        //        sqlparams.Add("@User_ID", User_ID);
        //    }
        //    if (param.TryGetValue("Quiz_Name", out string Quiz_Name))
        //    {
        //        filter.Add("QT.Quiz_Name = @Quiz_Name");
        //        sqlparams.Add("@Quiz_Name", Quiz_Name);
        //    }

        //    //filter.Add("QA.Answer_ID IS NULL");

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

        //    query += " ORDER BY QT.Quiz_Date DESC";
        //    //query += " Group By   Q.Ques_Desc ";


        //    DataTable table = _connection.ExecuteQueryWithResults(query, sqlparams);

        //    var quesList = new List<QuizTransactionModel>();
        //    foreach (DataRow row in table.Rows)
        //    {

        //        DateTime date = Convert.ToDateTime(row["Quiz_Date"]);
        //        //date.ToString("dd-MM-yyyy HH:mm")

        //        bool isAllowed = Convert.ToBoolean(row["IsAllowed"]);
        //        quesList.Add(new QuizTransactionModel
        //        {
        //            Quiz_ID = Convert.ToInt32(row["Quiz_ID"]),
        //            User_ID = Convert.ToInt32(row["User_ID"]),
        //            Ques_ID = Convert.ToInt32(row["Ques_ID"]),
        //            Ques_Desc = row["Ques_Desc"].ToString(),
        //            Opt_A = row["Opt_A"].ToString(),
        //            Opt_B = row["Opt_B"].ToString(),
        //            Opt_C = row["Opt_C"].ToString(),
        //            Opt_D = row["Opt_D"].ToString(),
        //            Quiz_Date = date,

        //            User_Email = row["User_Email"].ToString(),
        //            Topic_Name = row["Topic_Name"].ToString(),
        //            User_Name = row["User_Name"].ToString(),
        //            Quiz_Name = row["Quiz_Name"].ToString(),
        //            IsAllowed = isAllowed

        //        }); ;
        //    }

        //    return Ok(quesList);
        //}



        [HttpPost]
        [Route("AddQuizTransaction")]
        public IActionResult AddQuizTransaction([FromBody] List<QuizTransactionModel> quizList)
        {
            if (quizList == null || quizList.Count == 0)
            {
                return Ok("No transaction Plese select Question , Allowed Time And Users !!!");
            }

            try
            {
                var connection = new LkDataConnection.Connection();

                //if (Request.Headers.ContainsKey("Proceed") && Request.Headers["Proceed"] == "true")
                //{
                //    string insertQuery = "INSERT INTO Quiz_Transaction_mst (Ques_ID, Quiz_Date, User_ID, Allowed_Time, Quiz_Name) VALUES ";
                //    List<string> valueRows = new List<string>();

                //    foreach (var quiz in quizList)
                //    {
                //        valueRows.Add($"('{quiz.Ques_ID}', '{quiz.Quiz_Date}', '{quiz.User_ID}', {quiz.Allowed_Time}, '{quiz.Quiz_Name}')");
                //    }

                //    insertQuery += string.Join(", ", valueRows);
                //    connection.bindmethod(insertQuery);

                //    return Ok("Quiz_Transaction_mst Submitted Successfully");
                //}
           

               
                foreach (var quiz in quizList)
                {
                    if (quiz.Quiz_Name.IsNullOrEmpty())
                    {
                        return Ok("Please Provide A QuizName");
                    }
                    if (quiz.Allowed_Time.ToString().IsNullOrEmpty())                  {
                        return Ok("Plese Provide Allowed_Time");
                    }

                    if (!quiz.Quiz_Date.HasValue)
                    {
                        return Ok("Quiz_Date is required for quiz: " + quiz.Quiz_Name);
                    }
                    if (quiz.User_ID==null)
                    {
                        return Ok("Please Select a User  for quiz: " + quiz.Quiz_Name);
                    }
                    //var duplicates = new List<string>();
                    string duplicateQueryCheck = $@"SELECT COUNT(*) FROM Quiz_Transaction_mst 
                                             WHERE User_ID = {quiz.User_ID} 
                                             AND Ques_ID = {quiz.Ques_ID}
                     AND Quiz_Name = '{quiz.Quiz_Name}'";

                    int duplicateCount = Convert.ToInt32(_connection.ExecuteScalar(duplicateQueryCheck));

                    //if (duplicateCount > 0)
                    //{
                    //    duplicates.Add($"User_ID: {quiz.User_ID}, Ques_ID: {quiz.Ques_ID}, Quiz_Name: {quiz.Quiz_Name}");
                    //}


                    if (duplicateCount > 0)
                    {
                        // string duplicateMessage = "quiz Already exists ";
                        //string duplicateMessage = "Quiz already present in the QuizBank for the following entries:" + string.Join(" ", duplicates) +
                        //                          "Do you want to add more questions?";
                        return Ok("questions already exists for same users ");
                        //return Ok(new { message = duplicateMessage, canProceed = true });
                    }
                    _connection.GetSqlConnection().Close();
                }

                string insertQueryWithoutDuplicates = "INSERT INTO Quiz_Transaction_mst (Ques_ID, Quiz_Date, User_ID, Allowed_Time, Quiz_Name) VALUES ";
                List<string> valueRowsWithoutDuplicates = new List<string>();

                foreach (var quiz in quizList)
                {
                    if (quiz.Quiz_Name.IsNullOrEmpty())
                    {
                        return Ok("Please Provide A QuizName");
                    }
                    if (quiz.Allowed_Time.ToString().IsNullOrEmpty())
                    {
                        return Ok("Plese Provide Allowed_Time");
                    }

                    if (!quiz.Quiz_Date.HasValue)
                    {
                        return Ok("Quiz_Date is required for quiz: " + quiz.Quiz_Name);
                    }
                    if (quiz.User_ID == null)
                    {
                        return Ok("Please Select a User  for quiz: " + quiz.Quiz_Name);
                    }
                    DateTime quizDate = quiz.Quiz_Date.Value;
                    quizDate = new DateTime(quizDate.Year, quizDate.Month, quizDate.Day, quizDate.Hour, quizDate.Minute, 0);
                    valueRowsWithoutDuplicates.Add($"('{quiz.Ques_ID}', '{quizDate}', '{quiz.User_ID}', {quiz.Allowed_Time}, '{quiz.Quiz_Name}')");
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
        [Route("CopyQuizTransaction")]
        public IActionResult CopyQuizTransaction([FromBody] List<QuizTransactionModel> quizList)
        {

            if (quizList == null || quizList.Count == 0)
            {
                return Ok("No transaction provided.");
            }

            try
            {
                var connection = new LkDataConnection.Connection();

                foreach (var quiz in quizList)
                {
                    if (quiz.Quiz_Name.IsNullOrEmpty())
                    {
                        return Ok("Plese Provide A QuizName");
                    }
                    if (quiz.SelectedQuestions == null || quiz.SelectedQuestions.Count == 0)
                    {
                        return Ok("No questions selected for quiz: " + quiz.Quiz_Name);
                    }
                    if (quiz.Allowed_Time.ToString().IsNullOrEmpty())
                    {
                        return Ok("Plese Provide Allowed_Time");
                    }
                    if (!quiz.Quiz_Date.HasValue)
                    {
                        return Ok("Quiz_Date is required for quiz: " + quiz.Quiz_Name);
                    }
                    if (quiz.User_ID == null)
                    {
                        return Ok("Please Select a User  for quiz: " + quiz.Quiz_Name);
                    }
                    foreach (int quesId in quiz.SelectedQuestions)
                    {

                        string duplicateQueryCheck = $@"
                    SELECT * FROM Quiz_Transaction_mst 
                    WHERE User_ID = {quiz.User_ID} 
                    AND Ques_ID = {quesId} 
                   AND Quiz_Name = '{quiz.Quiz_Name}'";

                        DataTable duplicateTable = _connection.ExecuteQueryWithResult(duplicateQueryCheck);
                        if (duplicateTable.Rows.Count > 0)
                        {
                            return StatusCode(StatusCodes.Status205ResetContent, "Already exists question for same User ");


                            // continue; 
                        }

                        DateTime quizDate = quiz.Quiz_Date.Value;
                        quizDate = new DateTime(quizDate.Year, quizDate.Month, quizDate.Day, quizDate.Hour, quizDate.Minute, 0);

                        string insertQuery = $@"
                    INSERT INTO Quiz_Transaction_mst (Ques_ID, Quiz_Date, User_ID, Allowed_Time, Quiz_Name) 
                    VALUES ('{quesId}', '{quizDate}', '{quiz.User_ID}', {quiz.Allowed_Time}, '{quiz.Quiz_Name}')";

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
                DateTime quizDate = quiz.Quiz_Date.Value;
                quizDate = new DateTime(quizDate.Year, quizDate.Month, quizDate.Day, quizDate.Hour, quizDate.Minute, 0);
                quiz.Quiz_Date = quizDate;

                _query = _dc.InsertOrUpdateEntity(quiz, "Quiz_Transaction_mst", Quiz_ID, "Quiz_ID");
                return Ok("Quiz_Transaction_mst Updated Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }
        [HttpGet]
        [Route("WithInDayQuiz")]
        public IActionResult WithInDayQuiz([FromQuery] IDictionary<string, string> param)
        {
            try
            {


                string query = $@"
              SELECT 
  Distinct(Quiz_Date ) As Quiz_Date ,Quiz_Name
FROM 
    Quiz_Transaction_mst 
         
       
        ";

                List<string> filter = new List<string>();
                Dictionary<string, object> sqlparams = new Dictionary<string, object>();

                if (param.TryGetValue("quizDate", out string QuizDate) && DateTime.TryParse(QuizDate, out DateTime quizDate))
                {
                    filter.Add("CAST(Quiz_Date AS DATE) = @quizDate");
                    sqlparams.Add("@quizDate", quizDate);
                }
                if (filter.Count > 0)
                {
                    query += " WHERE " + string.Join(" AND ", filter);
                }
                query += " Order By Quiz_Date DESC ";


                DataTable table = _connection.ExecuteQueryWithResults(query, sqlparams);

                var QuizTransaction = new List<QuizTransactionModel>();
                foreach (DataRow row in table.Rows)
                {

                    var quiz = new QuizTransactionModel
                    {

                        Quiz_Date = Convert.ToDateTime(row["Quiz_Date"]),
                        Quiz_Name = row["Quiz_Name"].ToString(),



                    };

                    QuizTransaction.Add(quiz);
                }

                return Ok(QuizTransaction);
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("QuizTransactionDates")]
        public IActionResult QuizTransactionDates()
        {
            try
            {


                string query = $@"
              SELECT 
  Distinct(CAST(Quiz_Date AS DATE) ) As Quiz_Date 
FROM 
    Quiz_Transaction_mst Order By Quiz_Date DESC
         
       
        ";
                DataTable Table = _connection.ExecuteQueryWithResult(query);

                var QuizTransaction = new List<QuizTransactionModel>();
                foreach (DataRow row in Table.Rows)
                {

                    var quiz = new QuizTransactionModel
                    {

                        Quiz_Date = Convert.ToDateTime(row["Quiz_Date"]),


                    };

                    QuizTransaction.Add(quiz);
                }

                return Ok(QuizTransaction);
            }

            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


        [HttpGet]
        [Route("willBeQuiz/{User_ID}")]
        public IActionResult willBeQuiz(int User_ID)
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

                        Quiz_Date = Convert.ToDateTime(row["Quiz_Date"]),
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

                        Quiz_Date = Convert.ToDateTime(row["Quiz_Date"]),
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
        public IActionResult GetQuizDates([FromQuery] IDictionary<string, string> param)
        {
            try
            {
                //  string query = $"SELECT DISTINCT CAST(Quiz_Date AS DATE) AS Quiz_Date  FROM Quiz_Transaction_mst WHERE CAST(Quiz_Date AS DATE) < CAST(GETDATE() AS DATE) AND User_ID = {User_ID} ORDER BY Quiz_Date DESC;";
                string query = $"    SELECT DISTINCT CAST(Answer_Date as Date) As Answer_Date  FROM [Quiz_AnsTransaction_mst]  ";
                List<string> filter = new List<string>();
                Dictionary<string, object> sqlparams = new Dictionary<string, object>();

                if (param.TryGetValue("User_ID", out string User_ID))
                {
                    query += $@" WHERE  CAST(Answer_Date AS DATE) <= CAST(GETDATE() AS DATE) AND User_ID = {User_ID} ";
                }

                query += " ORDER BY Answer_Date DESC; ";
                DataTable Table = _connection.ExecuteQueryWithResult(query);

                var QuizDates = new List<Quiz_AnsTransactionModel>();
                foreach (DataRow row in Table.Rows)
                {
                    var quiz = new Quiz_AnsTransactionModel
                    {

                        Answer_Date = row["Answer_Date"].ToString(),

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
        //[HttpGet]
        //[Route("GetQuizDates/{User_ID}")]
        //public IActionResult GetQuizDates(int User_ID)
        //{
        //    try
        //    {
        //        //  string query = $"SELECT DISTINCT CAST(Quiz_Date AS DATE) AS Quiz_Date  FROM Quiz_Transaction_mst WHERE CAST(Quiz_Date AS DATE) < CAST(GETDATE() AS DATE) AND User_ID = {User_ID} ORDER BY Quiz_Date DESC;";
        //        string query = $"    SELECT DISTINCT CAST(Answer_Date as Date) As Answer_Date  FROM [Quiz_AnsTransaction_mst] WHERE  CAST(Answer_Date AS DATE) <= CAST(GETDATE() AS DATE) AND User_ID = {User_ID} ORDER BY Answer_Date DESC;";
        //        DataTable Table = _connection.ExecuteQueryWithResult(query);

        //        var QuizDates = new List<Quiz_AnsTransactionModel>();
        //        foreach (DataRow row in Table.Rows)
        //        {
        //            var quiz = new Quiz_AnsTransactionModel
        //            {

        //                Answer_Date = row["Answer_Date"].ToString(),

        //            };

        //            QuizDates.Add(quiz);
        //        }

        //        return Ok(QuizDates);
        //    }

        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        //    }
        //}

        [HttpDelete]
        [Route("deleteQuizTransaction/{id}")]
        public IActionResult deleteQuizTransaction(int id)
        {
            try
            {
                string quizDataQuery = $"SELECT Quiz_Date, Ques_ID, User_ID,Quiz_Name FROM Quiz_Transaction_mst WHERE Quiz_ID = {id}";
                var dataTable = _connection.ExecuteQueryWithResult(quizDataQuery);
                _connection.GetSqlConnection().Close();

                if (dataTable == null)
                {
                    return NotFound("Quiz not found.");
                }
                var row = dataTable.Rows[0];

                DateTime quizDate = Convert.ToDateTime(row["Quiz_Date"]);
                int quesId = Convert.ToInt32(row["Ques_ID"]);
                int userId = Convert.ToInt32(row["User_ID"]);
                string Quiz_Name = row["Quiz_Name"].ToString();

                //DateTime quizDate = Data.Quiz_Date;
                //    int quesId = Data.Ques_ID;
                //    int userId = Data.User_ID;



                string answerCheckQuery = $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE Ques_ID = {quesId} AND User_ID = {userId} AND Quiz_Name='{Quiz_Name}'";
                var answerCount = (int)_connection.ExecuteScalar(answerCheckQuery);

                _connection.GetSqlConnection().Close();

                if (answerCount > 0)
                {
                    return Ok("Cannot delete the quiz because answers have already been submitted.");
                }


                string deleteQuery = $"DELETE FROM Quiz_Transaction_mst WHERE Quiz_ID = {id}";
                LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);

                return Ok("Quiz_Transaction deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        //[HttpDelete]
        //[Route("deleteQuizTransaction/{id}")]
        //public IActionResult deleteQuizTransaction(int id)
        //{
        //    try
        //    {
        //        string checkQuery = $"SELECT Quiz_Date FROM Quiz_Transaction_mst WHERE Quiz_ID = {id}";
        //        var quizDate = (DateTime)_connection.ExecuteScalar(checkQuery);
        //        string answerCheckQuery = $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE  "


        //        var answerCount = (int)_connection.ExecuteScalar(answerCheckQuery);
        //        _connection.GetSqlConnection().Close();

        //        if (answerCount > 0)
        //        {
        //            return Ok("Cannot delete the quiz because answers have already been submitted.");
        //        }

        //        DateTime currentDateTime = DateTime.UtcNow;

        //        if (currentDateTime > quizDate)
        //        {
        //            return Ok("Cannot delete the quiz because it has expired.");
        //        }

        //        string deleteQuery = $"DELETE FROM Quiz_Transaction_mst WHERE Quiz_ID ={id}";
        //        LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);

        //        return Ok("Quiz_Transaction deleted successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        //    }
        //}

        [HttpDelete]
        [Route("DeleteQuiz/{date}/{userId?}")]
        public IActionResult DeleteQuiz(DateTime date,int? userId=null)
        {
            try
            {
                bool hasTimeComponent = date.TimeOfDay != TimeSpan.Zero;

                string checkQuery = hasTimeComponent
                    ? $"SELECT COUNT(*) FROM Quiz_Transaction_mst WHERE Quiz_Date = '{date}' "
                    : $"SELECT COUNT(*) FROM Quiz_Transaction_mst WHERE CAST(Quiz_Date AS DATE) = '{date.Date}'";

                var quizCount = (int)_connection.ExecuteScalar(checkQuery);
                _connection.GetSqlConnection().Close();

                if (quizCount == 0)
                {
                    return Ok("No quizzes found for the specified date.");
                }

               if(userId!=null)
                {
                    string answerCheckQuery = hasTimeComponent
               ? $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE Answer_Date = '{date}' AND User_ID={userId}"
               : $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE CAST(Answer_Date AS DATE) = '{date.Date}'  AND User_ID={userId}";
                   

                    var answerCount = (int)_connection.ExecuteScalar(answerCheckQuery);
                    _connection.GetSqlConnection().Close();

                    if (answerCount > 0)
                    {
                        return Ok("Cannot delete the quiz because answers have already been submitted.");
                    }
                }
                else
                {
                    string answerCheckQuery = hasTimeComponent
                      ? $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE Answer_Date = '{date}'"
                      : $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE CAST(Answer_Date AS DATE) = '{date.Date}'";

                    var answerCount = (int)_connection.ExecuteScalar(answerCheckQuery);
                    _connection.GetSqlConnection().Close();

                    if (answerCount > 0)
                    {
                        return Ok("Cannot delete the quiz because answers have already been submitted.");
                    }
                }

                if(userId!=null)
                {
                    string deleteQuery = hasTimeComponent
                ? $"DELETE FROM Quiz_Transaction_mst WHERE Quiz_Date = '{date}' AND User_ID={userId}"
                : $"DELETE FROM Quiz_Transaction_mst WHERE CAST(Quiz_Date AS DATE) = '{date.Date}' AND User_ID={userId}";
                    LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);

                }
                else
                {
                    string deleteQuery = hasTimeComponent
                ? $"DELETE FROM Quiz_Transaction_mst WHERE Quiz_Date = '{date}'"
                : $"DELETE FROM Quiz_Transaction_mst WHERE CAST(Quiz_Date AS DATE) = '{date.Date}' ";
                    LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);

                }



                return Ok(hasTimeComponent
                    ? "Quiz transaction for the specified date and time has been deleted successfully."
                    : "All quiz transactions for the specified date have been deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }
















        //[HttpDelete]
        //[Route("DeleteQuiz/{date}")]
        //public IActionResult DeleteQuiz(DateTime date)
        //{
        //    try
        //    {
        //        bool hasTimeComponent = date.TimeOfDay != TimeSpan.Zero;

        //        string checkQuery = hasTimeComponent
        //            ? $"SELECT COUNT(*) FROM Quiz_Transaction_mst WHERE Quiz_Date = '{date}'" 
        //            : $"SELECT COUNT(*) FROM Quiz_Transaction_mst WHERE CAST(Quiz_Date AS Date) = '{date.Date}'"; 

        //        var count = (int)_connection.ExecuteScalar(checkQuery);

        //        if (count == 0)
        //        {
        //            return Ok("No quizzes found for the specified date.");
        //        }
        //        string query = "select "; 

        //       DateTime currentDateTime = DateTime.UtcNow;

        //        if (currentDateTime > date)
        //        {
        //            return Ok("Cannot delete the quizzes because they have expired.");
        //        }

        //        string deleteQuery = hasTimeComponent
        //            ? $"DELETE FROM Quiz_Transaction_mst WHERE Quiz_Date = '{date}'" 
        //            : $"DELETE FROM Quiz_Transaction_mst WHERE CAST(Quiz_Date AS Date) = '{date.Date}'"; 

        //        LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);

        //        return Ok(hasTimeComponent
        //            ? "Quiz transaction for the specified date and time has been deleted successfully."
        //            : "All quiz transactions for the specified date have been deleted successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
        //    }
        //}


        //--------------------EDit Quiz Dates-----------------------

        [HttpPut]
        [Route("updateQuizDate/{oldDate}/{newDate}")]
        public IActionResult UpdateQuizDate(DateTime oldDate, DateTime newDate,int? userid=null)
        {
            try
            {
                //bool oldHasTimeComponent = oldDate.TimeOfDay != TimeSpan.Zero;
                //bool newHasTimeComponent = newDate.TimeOfDay != TimeSpan.Zero;

                //string checkQuery = oldHasTimeComponent
                //    ? $"SELECT COUNT(*) FROM Quiz_Transaction_mst WHERE Quiz_Date =' {oldDate}'"
                //    : $"SELECT COUNT(*) FROM Quiz_Transaction_mst WHERE CAST(Quiz_Date AS DATE) =' {oldDate.Date}'";
                string checkQuery= $"SELECT COUNT(*) FROM Quiz_Transaction_mst WHERE Quiz_Date =' {oldDate}'";

                var count = (int)_connection.ExecuteScalar(checkQuery);

                if (count == 0)
                {
                    return Ok("No quizzes found for the specified old date.");
                }
                if (userid != null)
                {
                    // string answerCheckQuery = oldHasTimeComponent
                    //   ? $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE Answer_Date = '{oldDate}' AND User_ID={userid}"
                    //    : $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE CAST(Answer_Date AS DATE) = '{oldDate.Date}'  AND User_ID={userid}";
                    string answerCheckQuery = $"SELECT COUNT(*) FROM Quiz_AnsTransaction_mst WHERE Answer_Date = '{oldDate}'";
                    var answerCount = (int)_connection.ExecuteScalar(answerCheckQuery);
                    _connection.GetSqlConnection().Close();

                    if (answerCount > 0)
                    {
                        return Ok("Cannot update the quiz because answers have already been submitted.");
                    }
                }

                string updateQuery = $"UPDATE Quiz_Transaction_mst SET Quiz_Date = '{newDate}' WHERE Quiz_Date = '{oldDate}'";

                //if (oldHasTimeComponent)
                //{
                //    updateQuery = newHasTimeComponent
                //        ? $"UPDATE Quiz_Transaction_mst SET Quiz_Date =' {newDate} 'WHERE Quiz_Date = '{oldDate}'"
                //        : $"UPDATE Quiz_Transaction_mst SET Quiz_Date = '{newDate.Date}' WHERE Quiz_Date = '{oldDate.Date}'";
                //}
                //else
                //{
                //    updateQuery = newHasTimeComponent
                //        ? $"UPDATE Quiz_Transaction_mst SET Quiz_Date = '{newDate}' WHERE CAST(Quiz_Date AS DATE) = '{oldDate}'"
                //        : $"UPDATE Quiz_Transaction_mst SET Quiz_Date = '{newDate.Date}' WHERE CAST(Quiz_Date AS DATE) = '{oldDate.Date}'";
                //}

                _connection.ExecuteQueryWithResult(updateQuery);

                return Ok("Quiz dates updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }




    }

}

