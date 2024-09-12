using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Quiz_DataBank.Classes;
using Quiz_DataBank.Model;
using System.Data;

namespace Quiz_DataBank.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class QuesTypeController : ControllerBase
    {
       
      

            private readonly Quiz_DataBank.Classes.Connection _connection;
            private LkDataConnection.DataAccess _dc;
            private LkDataConnection.SqlQueryResult _query;
            public QuesTypeController(Quiz_DataBank.Classes.Connection connection)
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
            [Route("QuesType")]
            public IActionResult GetAllQuesType()
            {
                string query = $"select * from Question_Type_mst";
            //var connection = new LkDataConnection.Connection();
            //var result = connection.bindmethod(query);

            //DataTable Table = result._DataTable;
            DataTable Table = _connection.ExecuteQueryWithResult(query);

            var ques_TypeList = new List<Ques_TypeModel>();
                foreach (DataRow row in Table.Rows)
                {
                ques_TypeList.Add(new Ques_TypeModel
                    {
                        QuesType_ID = row["QuesType_ID"] != DBNull.Value ? Convert.ToInt32(row["QuesType_ID"]) : 0,
                        QuesType_Label = row["QuesType_Label"].ToString() ?? string.Empty,
                        QuesType_Value = row["QuesType_Value"].ToString() ?? string.Empty,


                    });
                }
                return Ok(ques_TypeList);
            }
            // -------------------------AddTopics------------------------------
            [HttpPost]
            [Route("AddQuesType")]
            public IActionResult AddQuesType([FromBody] Ques_TypeModel quesType)
            {
                try

                {

                    var duplicacyChecker = new CheckDuplicacy(_connection);
                if (String.IsNullOrEmpty(quesType.QuesType_Value))
                {
                    return Ok("RoleName Can't be Blank Or Null ");
                }
                bool isDuplicate = duplicacyChecker.CheckDuplicate("Question_Type_mst",
                        new[] { "QuesType_Value" },
                        new[] { quesType.QuesType_Value });

                    if (isDuplicate)
                    {
                        return Ok("Question_Type already exists.");
                    }
                    _query = _dc.InsertOrUpdateEntity(quesType, "Question_Type_mst", -1);



                    return Ok("Question_Type Added Successfully");
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
                }
            }



            //---------------------------EditTopics--------------------------------------------------

            [HttpPut]
            [Route("updateQuestion_Type/{QuesType_ID}")]
            public IActionResult updateQuestion_Type(int QuesType_ID, [FromBody] Ques_TypeModel quesType)
            {
                try
                {
                    var duplicacyChecker = new CheckDuplicacy(_connection);

                    bool isDuplicate = duplicacyChecker.CheckDuplicate("Question_Type_mst",
                     new[] { "QuesType_Value" },
                     new[] { quesType.QuesType_Value },
                     "QuesType_ID", QuesType_ID.ToString());

                    if (isDuplicate)
                    {
                        return Ok("Duplicate ! Question_Type exists.");
                    }
                    _query = _dc.InsertOrUpdateEntity(quesType, "Question_Type_mst", QuesType_ID, "QuesType_ID");
                    return Ok("Question_Type Updated Successfully");
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
                }
            }
            //------------------------------DeleteTopic--------------------------
            [HttpDelete]
            [Route("deleteQuestion_Type/{QuesType_ID}")]
            public IActionResult deleteQuestion_Type(int QuesType_ID)
            {


                try
                {


                
                    string deleteQuery = $"Delete from Question_Type_mst where QuesType_ID='{QuesType_ID}'";
                    LkDataConnection.Connection.ExecuteNonQuery(deleteQuery);
                    return Ok("Question_Type Deleted successfully");

                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
                }
            }

        }
    }
