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
        //---------------------AllQuestions---------------------
        [HttpGet]
        [Route("AllQuestions")]
        public IActionResult GetAllQuestions()
        {
            string query = $"select Q.* , T.Topic_Name  from Questions_mst Q join Topics_mst T  ON  T.Topic_ID = Q.Topic_ID";
            var connection = new LkDataConnection.Connection();

            var result = connection.bindmethod(query);

            DataTable Table = result._DataTable;
            var QuestionsList = new List<QuestionsModel>();
            foreach (DataRow row in Table.Rows)
            {
                QuestionsList.Add(new QuestionsModel
                {
                    Ques_ID = Convert.ToInt32(row["Ques_ID"]),
                    Ques_Desc = row["Ques_Desc"].ToString(),
                    Opt_A = row["Opt_A"].ToString(),
                    Opt_B = row["Opt_B"].ToString(),
                    Opt_C = row["Opt_C"].ToString(),
                    Opt_D = row["Opt_D"].ToString(),
                    Correct_Answer = row["Correct_Answer"].ToString(),
                    Status = row["Status"].ToString(),
                    Topic_Name = row["Topic_Name"].ToString(),
                    Topic_ID = Convert.ToInt32(row["Topic_ID"])





                }); ;
            }
            return Ok(QuestionsList);
        }
        // -------------------------AddQuestions------------------------------
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
                    return BadRequest("Question already exists.");
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
                    return BadRequest("Duplicate ! Question exists.");
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
        //--------------------------GET QuestionBased on Topics---------------------------------
        [HttpGet]
        [Route("QuestionBasedOnTopic/{Topic_ID}")]
        public IActionResult GetQuestionBasedOnTopic( int Topic_ID)
        {
            string query = $"select Q.* , T.Topic_Name  from Questions_mst Q join Topics_mst T  ON  T.Topic_ID = Q.Topic_ID where Q.Topic_ID={Topic_ID}";
            var connection = new LkDataConnection.Connection();

            var result = connection.bindmethod(query);

            DataTable Table = result._DataTable;
            var QuestionsList = new List<QuestionsModel>();
            foreach (DataRow row in Table.Rows)
            {
                QuestionsList.Add(new QuestionsModel
                {
                    Ques_ID = Convert.ToInt32(row["Ques_ID"]),
                    Ques_Desc = row["Ques_Desc"].ToString(),
                    Opt_A = row["Opt_A"].ToString(),
                    Opt_B = row["Opt_B"].ToString(),
                    Opt_C = row["Opt_C"].ToString(),
                    Opt_D = row["Opt_D"].ToString(),
                    Correct_Answer = row["Correct_Answer"].ToString(),
                    Status = row["Status"].ToString(),
                    Topic_Name = row["Topic_Name"].ToString(),
                    Topic_ID = Convert.ToInt32(row["Topic_ID"])





                }); ;
            }
            return Ok(QuestionsList);
        }


    }
}


