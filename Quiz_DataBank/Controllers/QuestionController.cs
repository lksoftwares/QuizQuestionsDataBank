﻿using Microsoft.AspNetCore.Authorization;
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
    public class QuestionController : ControllerBase
    {

        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public QuestionController(Quiz_DataBank.Classes.Connection connection)
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
        [Route("AllQuestions")]
        public IActionResult GetAllQuestions([FromQuery] string? Topic_Name)
        {
            string query = "select Q.*, T.Topic_Name, T.Topic_ID ,QT.* from Questions_mst Q join Topics_mst T ON T.Topic_ID = Q.Topic_ID join Question_Type_mst QT ON QT.QuesType_ID = Q.QuesType_ID";
            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(Topic_Name))
            {
                var topicNames = Topic_Name.Split(',');
                List<string> paramPlaceholders = new List<string>();

                for (int i = 0; i < topicNames.Length; i++)
                {
                    string paramName = "@Topic_Name" + i;
                    paramPlaceholders.Add(paramName);
                    sqlparams.Add(paramName, topicNames[i]);
                }

                filter.Add($"T.Topic_Name IN ({string.Join(",", paramPlaceholders)})");
            }

            if (filter.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filter);
            }

            DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);
            var QuestionsList = new List<QuestionsModel>();

            foreach (DataRow row in Table.Rows)
            {
                QuestionsList.Add(new QuestionsModel
                {
                    Ques_ID = row["Ques_ID"] != DBNull.Value ? Convert.ToInt32(row["Ques_ID"]) : 0,
                    Ques_Desc = row["Ques_Desc"]?.ToString() ?? string.Empty,
                    Opt_A = row["Opt_A"]?.ToString() ?? string.Empty,
                    Opt_B = row["Opt_B"]?.ToString() ?? string.Empty,
                    Opt_C = row["Opt_C"]?.ToString() ?? string.Empty,
                    Opt_D = row["Opt_D"]?.ToString() ?? string.Empty,
                    Correct_Answer = row["Correct_Answer"]?.ToString() ?? string.Empty,
                    Status = row["Status"]?.ToString() ?? string.Empty,
                    Topic_Name = row["Topic_Name"]?.ToString() ?? string.Empty,
                    Topic_ID = row["Topic_ID"] != DBNull.Value ? Convert.ToInt32(row["Topic_ID"]) : 0,
                    QuesType_ID = row["QuesType_ID"] != DBNull.Value ? Convert.ToInt32(row["QuesType_ID"]) : 0,
                    Remarks = row["Remarks"]?.ToString() ?? string.Empty,
                    QuesType_Label = row["QuesType_Label"]?.ToString() ?? string.Empty,
                    QuesType_Value = row["QuesType_Value"]?.ToString() ?? string.Empty
                });
            }

            return Ok(QuestionsList);
        }

        //[HttpGet]
        //[Route("AllQuestions")]
        //public IActionResult GetAllQuestions([FromQuery] IDictionary<string, string> param)
        //{
        //    string query = "select Q.*, T.Topic_Name, T.Topic_ID ,QT.* from Questions_mst Q join Topics_mst T ON T.Topic_ID = Q.Topic_ID join Question_Type_mst QT ON QT.QuesType_ID = Q.QuesType_ID";
        //    List<string> filter = new List<string>();
        //    Dictionary<string, object> sqlparams = new Dictionary<string, object>();
        //    if (param.TryGetValue("Topic_Name", out string Topic_Name))
        //    {
        //        filter.Add("  T.Topic_Name = @Topic_Name");
        //        sqlparams.Add("@Topic_Name", Topic_Name);
        //    }
        //    if (filter.Count > 0)
        //    {
        //        query += " WHERE " + string.Join(" AND ", filter);
        //    }
        //    //var connection = new LkDataConnection.Connection();
        //    //var result = connection.bindmethod(query);

        //    //if (result == null || result._DataTable == null)
        //    //{
        //    //    return NotFound("No data found or error in data retrieval.");
        //    //}

        //    //DataTable Table = result._DataTable;
        //    DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);
        //    var QuestionsList = new List<QuestionsModel>();

        //    foreach (DataRow row in Table.Rows)
        //    {
        //        QuestionsList.Add(new QuestionsModel
        //        {
        //            Ques_ID = row["Ques_ID"] != DBNull.Value ? Convert.ToInt32(row["Ques_ID"]) : 0,
        //            Ques_Desc = row["Ques_Desc"]?.ToString() ?? string.Empty,
        //            Opt_A = row["Opt_A"]?.ToString() ?? string.Empty,
        //            Opt_B = row["Opt_B"]?.ToString() ?? string.Empty,
        //            Opt_C = row["Opt_C"]?.ToString() ?? string.Empty,
        //            Opt_D = row["Opt_D"]?.ToString() ?? string.Empty,
        //            Correct_Answer = row["Correct_Answer"]?.ToString() ?? string.Empty,
        //            Status = row["Status"]?.ToString() ?? string.Empty,
        //            Topic_Name = row["Topic_Name"]?.ToString() ?? string.Empty,
        //            Topic_ID = row["Topic_ID"] != DBNull.Value ? Convert.ToInt32(row["Topic_ID"]) : 0,
        //            QuesType_ID = row["QuesType_ID"] != DBNull.Value ? Convert.ToInt32(row["QuesType_ID"]) : 0,
        //            Remarks = row["Remarks"]?.ToString() ?? string.Empty,
        //            QuesType_Label = row["QuesType_Label"]?.ToString() ?? string.Empty,
        //            QuesType_Value = row["QuesType_Value"]?.ToString() ?? string.Empty
        //        });
        //    }

        //    return Ok(QuestionsList);
        //}

        //// -------------------------AddQuestions------------------------------
        [HttpPost]
        [Route("AddQuestions")]
        public IActionResult AddQuestions([FromBody] QuestionsModel ques)
        {
            try

            {

                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Questions_mst",
                    new[] { "Ques_Desc" },
                    new[] { ques.Ques_Desc });

                if (isDuplicate)
                {
                    return Ok("Question already exists.");
                }
                if (String.IsNullOrEmpty(ques.Ques_Desc) || String.IsNullOrEmpty(ques.Correct_Answer) || String.IsNullOrEmpty(ques.Topic_ID.ToString()))
                {
                    return Ok("Question description And Correct Answer and Related Topic Name Can' be Blank Or Null ");
                }
               


                _query = _dc.InsertOrUpdateEntity(ques, "Questions_mst", -1);
                



                return Ok("Question Added Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }



        //---------------------------EditQuestions--------------------------------------------------

        [HttpPut]
        [Route("updateQuestions/{Ques_ID}")]
        public IActionResult UpdateQuestion(int Ques_ID, [FromBody] QuestionsModel ques)
        {
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Questions_mst",
                 new[] { "Ques_Desc" },
                 new[] { ques.Ques_Desc },
                 "Ques_ID");

                if (isDuplicate)
                {
                    return Ok("Duplicate ! Question exists.");
                }
               
                _query = _dc.InsertOrUpdateEntity(ques, "Questions_mst", Ques_ID, "Ques_ID");
                return Ok("Question Updated Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }
        //------------------------------DeleteQuestions--------------------------
        [HttpDelete]
        [Route("deleteQuestion/{Ques_ID}")]
        public IActionResult DeleteQuestion(int Ques_ID)
        {
            string deleteQuesQuery = $"Delete from Questions_mst where Ques_ID='{Ques_ID}'";
            try
            {
                LkDataConnection.Connection.ExecuteNonQuery(deleteQuesQuery);
                return Ok("Question Deleted successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }
        //--------------------------TotalQuestionCards---------------------------------
        [HttpGet]
        [Route("TotalQuestion")]
        public IActionResult TotalQuestion()
        {
            string query = $"SELECT COUNT(Ques_ID) AS totalQues FROM Questions_mst";

            //var connection = new LkDataConnection.Connection();

            //var result = connection.bindmethod(query);
            //DataTable Table = result._DataTable;
            DataTable Table = _connection.ExecuteQueryWithResult(query);


            if (Table.Rows.Count > 0)
            {
                int totalQues = Convert.ToInt32(Table.Rows[0]["totalQues"]);
                return Ok(new { TotalQuestions = totalQues });
            }

            return Ok(new { TotalQuestions = 0 });
        }



    }
}


