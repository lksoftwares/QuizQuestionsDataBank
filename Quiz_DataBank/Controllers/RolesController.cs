﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Quiz_DataBank.Classes;
using Quiz_DataBank.Model;
using System.Data;
using LkDataConnection;
using Microsoft.AspNetCore.Authorization;

namespace Quiz_DataBank.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {

        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public RolesController(Quiz_DataBank.Classes.Connection connection )
        {
            _connection = connection;
            DataAccessMethod();
        }

        private void DataAccessMethod()
        {
            try
            {
                LkDataConnection.Connection.ConnectionStr = _connection.GetSqlConnection().ConnectionString;
                LkDataConnection.Connection.Connect();
                _dc = new LkDataConnection.DataAccess();
                _query = new LkDataConnection.SqlQueryResult();
            }
            catch(Exception ex)
            {
                
            }
          
        }
       [AllowAnonymous]

        [HttpGet]

        [Route("getallrole")]

        public IActionResult GetAllRole()
        {
            string query = $"select * from Roles_mst ORDER BY RoleName ASC";
            Console.WriteLine(query);
            //var connection = new LkDataConnection.Connection();

            //var result = connection.bindmethod(query);
            

            //DataTable Table = result._DataTable;
            DataTable Table = _connection.ExecuteQueryWithResult(query);

            var RoleList = new List<RolesModel>();
            foreach (DataRow row in Table.Rows)
            {
                RoleList.Add(new RolesModel
                {
                    Role_ID = Convert.ToInt32(row["Role_ID"]),
                    RoleName = row["RoleName"].ToString(),

                });
            }
            return Ok(RoleList);
        }
        [HttpPost]

        [Route("AddRole")]
       // [RoleAuthorize("Admin", "User","user1")]


        public IActionResult AddRole([FromBody] RolesModel role)
        {
            try

            {

                var duplicacyChecker = new CheckDuplicacy(_connection);


                bool isDuplicate = duplicacyChecker.CheckDuplicate("Roles_mst",
                    new[] { "RoleName" },
                    new[] { role.RoleName });

                if (isDuplicate)
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "RoleName already exists.", DUP = true });

                }
                if (String.IsNullOrEmpty(role.RoleName))
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "RoleName Can't be Blank Or Null", DUP = false });

                }
                _query = _dc.InsertOrUpdateEntity(role, "Roles_mst", -1);


                return StatusCode(StatusCodes.Status200OK, new { message = "RoleName Added Successfully", DUP = false });


            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

       // [RoleAuthorize("Admin", "User")]
        [HttpPut]
        [Route("updateRole/{Role_ID}")]
        public IActionResult UpdateRole(int Role_ID, [FromBody] RolesModel role)
        {
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Roles_mst",
                 new[] { "RoleName" },
                 new[] { role.RoleName  },
                 "Role_ID" , Role_ID.ToString());

                if (isDuplicate)
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "RoleName already exists.", DUP = true });

                }
                if (String.IsNullOrEmpty(role.RoleName) )
                {
                    return StatusCode(StatusCodes.Status200OK, new { message = "RoleName Can't be Blank Or Null", DUP = true });
                }
                _query = _dc.InsertOrUpdateEntity(role, "Roles_mst", Role_ID, "Role_ID");
                return StatusCode(StatusCodes.Status200OK, new { message = "RoleName Updated Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }

        [HttpDelete]
        [Route("deleteRole/{id}")]
        public IActionResult DeleteRoleName(int id)
        {
          
                string checkQuery = $"SELECT COUNT(*) AS recordCount FROM Users_mst WHERE Role_ID = {id}";


                try
                {


                    int result = Convert.ToInt32(_connection.ExecuteScalar(checkQuery));
                    if (result > 0)
                    {
                        return Ok("Can't delete Exists in another table  ");
                    }
                    string deleteRoleQuery = $"Delete from Roles_mst where Role_ID='{id}'";

                    LkDataConnection.Connection.ExecuteNonQuery(deleteRoleQuery);
               // _connection.ExecuteQueryWithoutResult(deleteDepQuery);
                return Ok("RoleName Deleted successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }

    }
}
