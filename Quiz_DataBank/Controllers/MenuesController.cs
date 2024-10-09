
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quiz_DataBank.Classes;
using Quiz_DataBank.Model;
using System.Data;
using LkDataConnection;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Quiz_DataBank.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class MenuesController : ControllerBase
    {

        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public MenuesController(Quiz_DataBank.Classes.Connection connection)
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
        [Route("getallMenues")]
        public IActionResult GetAllRole()
        {
           
            var tokenRoleId = User.Claims.FirstOrDefault(c => c.Type == "Role_ID")?.Value;
            Console.WriteLine(tokenRoleId);

            if (string.IsNullOrEmpty(tokenRoleId))
            {
                return Unauthorized(new { message = "Token does not contain a valid Role_ID." });
            }

            string query = $"select * from Menues_mst where Role_ID={tokenRoleId}";

     
            DataTable Table = _connection.ExecuteQueryWithResult(query);

            var MenuesList = new List<MenuesModel>();
            foreach (DataRow row in Table.Rows)
            {
                if (row["Role_ID"].ToString() == tokenRoleId)
                {
                    MenuesList.Add(new MenuesModel
                    {
                        Menu_ID = Convert.ToInt32(row["Menu_ID"]),
                        Menu_Name = row["Menu_Name"].ToString(),
                        Menu_URL = row["Menu_URL"].ToString(),
                        Role_ID = Convert.ToInt32(row["Role_ID"])
                    });
                }
            }

            if (MenuesList.Count == 0)
            {
                return Forbid( "No menus are accessible for the given Role_ID." );
            }

           
            return Ok(MenuesList);
        }

        //  [AllowAnonymous]
        //[HttpGet]
        //[Route("getallMenues")]
        //public IActionResult GetAllRole([FromQuery] IDictionary<string, string> param)
        //{
        //    string query = $"select * from Menues_mst";

        //    var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        //    var roleId = User.Claims.FirstOrDefault(c => c.Type == "Role_ID")?.Value;
        //    List<string> filter = new List<string>();
        //    Dictionary<string, object> sqlparams = new Dictionary<string, object>();
        //    if (param.TryGetValue("Role_ID", out string Role_ID))
        //    {
        //        filter.Add(" Role_ID = @Role_ID");
        //        sqlparams.Add("@Role_ID", Role_ID);
        //    }

        //    if (filter.Count > 0)
        //    {
        //        query += " WHERE " + string.Join(" AND ", filter);
        //    }


        //    DataTable Table = _connection.ExecuteQueryWithResults(query, sqlparams);
        //    //var connection = new LkDataConnection.Connection();

        //    //var result = connection.bindmethod(query);

        //    //DataTable Table = result._DataTable;
        //   // DataTable Table = _connection.ExecuteQueryWithResult(query);

        //    var MenuesList = new List<MenuesModel>();
        //    foreach (DataRow row in Table.Rows)
        //    {
        //        MenuesList.Add(new MenuesModel
        //        {
        //            Menu_ID = Convert.ToInt32(row["Menu_ID"]),
        //            Menu_Name = row["Menu_Name"].ToString(),
        //            Menu_URL = row["Menu_URL"].ToString(),
        //            Role_ID = Convert.ToInt32(row["Role_ID"]),



        //        });
        //    }
        //    return Ok(MenuesList);
        //}
        [HttpPost]
        [Route("AddMenue")]
        public IActionResult AddMenue([FromBody] MenuesModel menu)
        {
            try

            {

                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Menues_mst",
                    new[] { "Menu_URL" },
                    new[] { menu.Menu_URL });

                if (isDuplicate)
                {
                    return Ok("Menu_URL already exists.");
                }
                if (String.IsNullOrEmpty(menu.Menu_URL)|| String.IsNullOrEmpty(menu.Menu_Name))
                {
                    return Ok("MenuName,MenueURL Can't be Blank Or Null ");
                }
                _query = _dc.InsertOrUpdateEntity(menu, "Menues_mst", -1);




                return Ok("Menue Added Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }


        [HttpPut]
        [Route("updateMenue/{Menu_ID}")]
        public IActionResult UpdateRole(int Menu_ID, [FromBody] MenuesModel menu)
        {
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Menues_mst",
                 new[] { "Menu_URL" },
                 new[] { menu.Menu_URL },
                 "Menu_ID", Menu_ID.ToString());

                if (isDuplicate)
                {
                    return Ok("Duplicate Menue URL  exists.");
                }
                if (String.IsNullOrEmpty(menu.Menu_Name)|| String.IsNullOrEmpty(menu.Menu_URL))
                {
                    return Ok("MenuName,Menue URL Can't be Blank Or Null ");
                }
                _query = _dc.InsertOrUpdateEntity(menu, "Menues_mst", Menu_ID, "Menu_ID");
                return Ok("Menues  Updated Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }

        [HttpDelete]
        [Route("deleteRole/{Menu_ID}")]
        public IActionResult DeleteRoleName(int Menu_ID)
        {
            string deleteRoleQuery = $"Delete from Menues_mst where Menu_ID='{Menu_ID}'";
            try
            {
                LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
                // _connection.ExecuteQueryWithoutResult(deleteDepQuery);
                return Ok("Menue Deleted successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }

    }
}
