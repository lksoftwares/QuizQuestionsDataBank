using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Quiz_DataBank.Classes;
using Quiz_DataBank.Model;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LkDataConnection;
using Microsoft.AspNetCore.Cors;
namespace Quiz_DataBank.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly IConfiguration _config;

        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public UsersController( IConfiguration configuration ,Quiz_DataBank.Classes.Connection connection)
        {
            _config = configuration;
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
        [Route("AllUSers")]
  
        public IActionResult GetAllUSers()
        {
            var query = $"select U.*,R.RoleName From Users_mst U join Roles_mst R ON R.Role_ID=U.Role_ID";
            DataTable UserTable = _connection.ExecuteQueryWithResult(query);
            var UsersList = new List<UsersModel>();
                
            foreach (DataRow row in UserTable.Rows)
            {
                //string encryptedPassword = row["User_Password"].ToString();
                //string realPassword = PasswordUtility.DecryptPassword(encryptedPassword);

                UsersList.Add(new UsersModel
                {

                    User_ID = Convert.ToInt32(row["User_ID"]),
                    Role_ID = Convert.ToInt32(row["Role_ID"]),

                    User_Name = row["User_Name"].ToString(),
                    User_Email = row["User_Email"].ToString(),
                    // User_Password = realPassword,
                   User_Password = row["User_Password"].ToString(),

                    userRole = row["RoleName"].ToString(),

                    Status = Convert.ToInt32(row["Status"])


                }); ;



            }
            return Ok(UsersList);


        }
        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromBody] UsersModel newUser)
        {
           
            if (newUser.User_Password != null)
            {
                string encryptedPassword = PasswordUtility.EncryptPassword(newUser.User_Password);
                Console.WriteLine(encryptedPassword);
                //string hashedPassword = HashedPassword.HashPassword(newUser.User_Password);
                // newUser.User_Password = hashedPassword;
                newUser.User_Password = encryptedPassword;
            }
            newUser.Status = 1;
            newUser.Role_ID = 3;
             try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Users_mst",
                 new[] { "User_Email", "User_Name" },
                 new[] { newUser.User_Email,newUser.User_Name }
                );

                if (isDuplicate)
                {
                    return Ok("Duplicate ! Users exists.");
                }
                if (String.IsNullOrEmpty(newUser.User_Email) || String.IsNullOrEmpty(newUser.User_Name) || String.IsNullOrEmpty(newUser.User_Password))
                {
                    return Ok("User Email ,Name,Password");
                }
                _query = _dc.InsertOrUpdateEntity(newUser, "Users_mst", -1);

                return Ok("USer Register successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error:{ex.Message}");
            }

        }
        private string GenerateToken(UsersModel users)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var issuedAt = DateTime.UtcNow;
            var localIssuedAt = TimeZoneInfo.ConvertTimeFromUtc(issuedAt, TimeZoneInfo.Local);

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                new List<Claim>
                {
            new Claim(ClaimTypes.NameIdentifier, users.User_ID.ToString()),
            new Claim(ClaimTypes.Name, users.User_Name),
            new Claim("iat", new DateTimeOffset(localIssuedAt).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer)
                },
                expires: localIssuedAt.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]

        public IActionResult Login(UsersModel user)
        {
            IActionResult response = Unauthorized();
            try
            {
                string hashedPassword = HashedPassword.HashPassword(user.User_Password);
               //string hashedPassword = PasswordUtility.EncryptPassword(user.User_Password);
               // string realp = PasswordUtility.DecryptPassword(hashedPassword);

                //         string query = $"SELECT U.*, R.RoleName, R.Role_ID FROM Users_mst U JOIN Roles_mst R ON U.Role_ID = R.Role_ID WHERE U.User_Email = '{user.User_Email}' AND U.User_Password='{hashedPassword}'";
                string query = $"SELECT * FROM Users_mst  WHERE User_Email = '{user.User_Email}' AND User_Password='{hashedPassword}'";

                var connection = new LkDataConnection.Connection();

                // DataTable result = _connection.ExecuteQueryWithResult(query);
              
            var result = connection.bindmethod(query);

                DataTable Table = result._DataTable;
                DataRow userData = Table.Rows.Count > 0 ? Table.Rows[0] : null;

                
        
                if (userData == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }

    
                if (userData["Status"].ToString()!="1")
                {
                    return Unauthorized(new { message = "User is not active. Please contact the administrator." });
                }



                if (hashedPassword != userData["User_Password"].ToString())
                {
                    return Unauthorized(new { message = "Password not matched" });
                }


                if (userData["Role_ID"].ToString() != user.Role_ID.ToString())
                {
                    return Unauthorized(new { message = "Role not matched" });
                }

         
                string token = GenerateToken(new UsersModel
                {
                    User_ID = Convert.ToInt32(userData["User_ID"]),
                    User_Name = userData["User_Name"].ToString(),
                  //  userRole = userData["RoleName"].ToString()
                });
                ExtractTokenInformation(token);
                Console.WriteLine($"Here is the token {token}");
                
                response = Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }

            return response;
        }

        private void ExtractTokenInformation(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            DateTime expires = DateTimeOffset.FromUnixTimeSeconds(jsonToken.Payload.Exp ?? 0).LocalDateTime;
            Console.WriteLine($"Token Expires: {expires}");

            DateTime issuedAt = DateTimeOffset.FromUnixTimeSeconds(jsonToken.Payload.Iat ?? 0).LocalDateTime;
            Console.WriteLine($"Token Issued At: {issuedAt}");

            var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
            {
                string userId = userIdClaim.Value;
                Console.WriteLine($"User ID: {userId}");
            }

            var usernameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            if (usernameClaim != null)
            {
                string username = usernameClaim.Value;
                Console.WriteLine($"Username: {username}");
            }
        }
        [HttpPut]
        [Route("updateUsers/{User_ID}")]
        public IActionResult updateUsers(int User_ID, [FromBody] UsersModel user)
        {
            try
            {

                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Users_mst",
                 new[] { "User_Name" },
                 new[] { user.User_Name },
                 "User_ID", User_ID.ToString());

                if (isDuplicate)
                {
                    return Ok("Duplicate ! Users exists.");
                }
                if (user.User_Password != null)
                {
                    string hashedPassword = HashedPassword.HashPassword(user.User_Password);
                    //string hashedPassword = PasswordUtility.EncryptPassword(user.User_Password);

                    user.User_Password = hashedPassword;
                }
                if (String.IsNullOrEmpty(user.User_Email) || String.IsNullOrEmpty(user.User_Name) || String.IsNullOrEmpty(user.User_Password))
                {
                    return Ok("Email,Username,Password can't be blank");
                }
                _query = _dc.InsertOrUpdateEntity(user, "Users_mst", User_ID, "User_ID");
                return Ok("Users Updated Successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }
        [HttpDelete]
        [Route("deleteUser/{User_ID}")]
        public IActionResult DeleteUserName(int User_ID)
        {
            string deleteUserQuery = $"Delete from Users_mst where User_ID='{User_ID}'";
            try
            {
               LkDataConnection.Connection.ExecuteNonQuery(deleteUserQuery);
            //  _connection.ExecuteQueryWithoutResult(deleteUserQuery);
                return Ok("User Deleted successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }
    }
}