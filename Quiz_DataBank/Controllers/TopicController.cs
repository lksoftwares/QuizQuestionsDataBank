using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quiz_DataBank.Model;
using System.Data;
using LkDataConnection;
using Quiz_DataBank.Classes;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Data.Common;
using Microsoft.AspNetCore.Authorization;
namespace Quiz_DataBank.Controllers
{
    //[Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class TopicController : ControllerBase
    {


        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public TopicController(Quiz_DataBank.Classes.Connection connection)
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

        //-----------------------AllTopics---------------------------------
     
        [HttpGet]
        [Route("AllTopic")]
        public IActionResult GetAllTopics()
        {
            string query = $"select * from Topics_mst";
            //var connection = new LkDataConnection.Connection();

            //var result = connection.bindmethod(query);

            //DataTable Table = result._DataTable;
            DataTable Table = _connection.ExecuteQueryWithResult(query);

            var TopicsList = new List<TopicsModel>();
            foreach (DataRow row in Table.Rows)
            {
                TopicsList.Add(new TopicsModel
                {
                    Topic_ID = row["Topic_ID"] != DBNull.Value ? Convert.ToInt32(row["Topic_ID"]) : 0,

                    Topic_Name = row["Topic_Name"].ToString() ?? string.Empty,

                });
            }
            return Ok(TopicsList);
        }
        // -------------------------AddTopics------------------------------
        [AllowAnonymous]
        [HttpPost]
        [Route("AddTopic")]
        public IActionResult AddTopics([FromBody] TopicsModel topic)
        {
            try

            {

                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Topics_mst",
                    new[] { "Topic_Name" },
                    new[] { topic.Topic_Name });

                if (isDuplicate)
                {
                    return Ok("Topics already exists.");
                }
                if (String.IsNullOrEmpty(topic.Topic_Name))
                {
                    return Ok("Topic Name Can't be Blank Or Null ");
                }
                _query = _dc.InsertOrUpdateEntity(topic, "Topics_mst", -1);



                return Ok("Topics Added Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }



        //---------------------------EditTopics--------------------------------------------------

        [HttpPut]
        [Route("updateTopics/{Topic_ID}")]
        public IActionResult UpdateTopic(int Topic_ID, [FromBody] TopicsModel topic)
        {
            try
            {
               // Console.WriteLine(FromBody);
                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Topics_mst",
                 new[] { "Topic_Name" },
                 new[] { topic.Topic_Name },
                 "Topic_ID", Topic_ID.ToString());

                if (isDuplicate)
                {
                    return Ok("Duplicate ! Topic exists.");
                }
                if (String.IsNullOrEmpty(topic.Topic_Name))
                {
                    return Ok("Topic Name Can't be Blank Or Null ");
                }
                _query = _dc.InsertOrUpdateEntity(topic, "Topics_mst", Topic_ID, "Topic_ID");
                return Ok("Topic Updated Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }
        //------------------------------DeleteTopic--------------------------
        [HttpDelete]
        [Route("deleteTopic/{Topic_ID}")]
        public IActionResult DeleteTopicName(int Topic_ID)
        {
            string checkQuery = $"SELECT COUNT(*) AS recordCount FROM Questions_mst WHERE Topic_ID = {Topic_ID}";

         
            try
            {


                int result = Convert.ToInt32(_connection.ExecuteScalar(checkQuery));
                if(result>0)
                {
                    return Ok("Can't delete Exists in another table  ");
                }
               
                string deleteRoleQuery = $"Delete from Topics_mst where Topic_ID='{Topic_ID}'";
                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                return Ok("Topic Deleted successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }


    }
}
