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
using Microsoft.Data.SqlClient;
using System.Net.Mail;
using System.Net;
namespace Quiz_DataBank.Controllers
{
    [Authorize]
    [Route("/[controller]")]
    [ApiController]

    public class UsersController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly IConfiguration _config;

        private readonly Quiz_DataBank.Classes.Connection _connection;
        private LkDataConnection.DataAccess _dc;
        private LkDataConnection.SqlQueryResult _query;
        public UsersController(IConfiguration configuration, Quiz_DataBank.Classes.Connection connection, IWebHostEnvironment hostingEnvironment)
        {
            _config = configuration;
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
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

        public IActionResult GetAllUSers([FromQuery] IDictionary<string, string> param)
        {
            var query = $"select U.*,R.RoleName From Users_mst U join Roles_mst R ON R.Role_ID=U.Role_ID";

            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();
            if (param.TryGetValue("User_ID", out string User_ID))
            {
                filter.Add("  U.User_ID = @User_ID");
                sqlparams.Add("@User_ID", User_ID);
            }
            if (param.TryGetValue("Role_ID", out string Role_ID)) 
            {
                filter.Add("  U.Role_ID = @Role_ID");
                sqlparams.Add("@Role_ID", Role_ID);
            }

            if (filter.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filter);
            }


            DataTable UserTable = _connection.ExecuteQueryWithResults(query, sqlparams);
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
                    User_Password=  HashedPassword.DecryptPassword(row["User_Password"].ToString()),
                   // User_Password = row["User_Password"].ToString(),

                    userRole = row["RoleName"].ToString(),

                    Status = Convert.ToInt32(row["Status"])


                }); ;



            }
            string TotalUsers = $"select Count(*) as totalUsers from Users_mst where Role_ID=3";
            DataTable Table = _connection.ExecuteQueryWithResult(TotalUsers);
            int TotalUser = 0;
            if (Table.Rows.Count > 0)
            {
                TotalUser = Table.Rows[0]["totalUsers"] != DBNull.Value
                 ? Convert.ToInt32(Table.Rows[0]["totalUsers"])
                 : 0;
            }
            string TotalAdmin = $"select Count(*) as totalUsers from Users_mst where Role_ID=5";
            DataTable AdminTable = _connection.ExecuteQueryWithResult(TotalAdmin);
            int Admins = 0;
            if (AdminTable.Rows.Count > 0)
            {
                Admins = AdminTable.Rows[0]["totalUsers"] != DBNull.Value
                 ? Convert.ToInt32(AdminTable.Rows[0]["totalUsers"])
                 : 0;
            }

            return Ok(new
            {
                TotalAdmin = Admins,
                UsersLists = UsersList,
                TotalUsers = TotalUser
            });


        }


        [HttpGet]
        [Route("GetAllParticipent")]

        public IActionResult GetAllParticipent([FromQuery] IDictionary<string, string> param)
        {
            //var query = $"  select Q.*,DISTINCT(U.User_Name) from [Quiz_Transaction_mst] Q  join Users_mst U ON  Q.User_ID=U.User_ID";
            var query = $"SELECT U.USer_Name,U.User_ID, MIN(Q.Quiz_Date) AS Quiz_Date FROM [Quiz_Transaction_mst] Q JOIN [Users_mst] U ON Q.User_ID = U.User_ID";

            List<string> filter = new List<string>();
            Dictionary<string, object> sqlparams = new Dictionary<string, object>();
            if (param.TryGetValue("quizDate", out string QuizDate) && DateTime.TryParse(QuizDate, out DateTime quizDate))
            {
                filter.Add("Q.Quiz_Date = @quizDate");
                sqlparams.Add("@quizDate", quizDate);
            }
            if (filter.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", filter);
            }
            query += " GROUP BY U.User_Name,U.User_ID";

            DataTable UserTable = _connection.ExecuteQueryWithResults(query, sqlparams);
            var UsersList = new List<UsersModel>();
           

            foreach (DataRow row in UserTable.Rows)
            {
            

                UsersList.Add(new UsersModel
                {


                    User_Name = row["User_Name"].ToString(),
                    User_ID = Convert.ToInt32(row["User_ID"]),




                }); ;



            }
          

            return Ok(new
            { UsersList
               
            });


        }

        [HttpGet]
        [Route("participation")]
        public IActionResult GetCardsApi()
        {
            try
            {


                string participatedQuery = $" select Count(DISTINCT [User_ID]) as Participation from [Quiz_AnsTransaction_mst]  ";

                DataTable result = _connection.ExecuteQueryWithResult(participatedQuery);
                int participation = 0;
                if (result.Rows.Count > 0)
                {
                    participation += Convert.ToInt32(result.Rows[0]["Participation"]);
                }



                return Ok(new
                {
                    No_Of_Participated_User = participation
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IActionResult Register([FromForm] UsersModel newUser)
        {

            if (newUser.User_Password != null)
            {
                //string encryptedPassword = PasswordUtility.EncryptPassword(newUser.User_Password);
                //Console.WriteLine(encryptedPassword);
                string hashedPassword = HashedPassword.EncryptPassword(newUser.User_Password);

               // string hashedPassword = HashedPassword.HashPassword(newUser.User_Password);
                newUser.User_Password = hashedPassword;
                //  newUser.User_Password = encryptedPassword;
            }
            newUser.Status = 1;
            newUser.Role_ID = 3;
            try
            {
                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Users_mst",
                 new[] { "User_Email", "User_Name" },
                 new[] { newUser.User_Email, newUser.User_Name }
                );

                if (isDuplicate)
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Duplicate ! Users exists.", DUP = true });
                }
                if (String.IsNullOrEmpty(newUser.User_Email) || String.IsNullOrEmpty(newUser.User_Name) || String.IsNullOrEmpty(newUser.User_Password))
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "User Email ,Name,Password can't be blank " });

                }
                if (newUser.User_Name != null || !newUser.User_Name.IsNullOrEmpty())
                {
                    var username = new System.Globalization.CultureInfo("en-US", false).TextInfo.ToTitleCase(newUser.User_Name.ToLower());
                    newUser.User_Name = username;

                }

                //SendEmail sendEmail = new SendEmail();
                //sendEmail.SendConfirmationEmail(newUser.User_Email, newUser.User_Name,[ "F:\\vs2022\\sqlAuthJWt\\Quiz_DataBank\\Quiz_DataBank\\wwwroot\\Public\\images\\03c1dafe-0382-4300-b6c1-e7566a383444.png", "F:\\vs2022\\sqlAuthJWt\\Quiz_DataBank\\Quiz_DataBank\\wwwroot\\Public\\images\\0d5ef12c-7256-4cc3-b7eb-981e6765de28.jpeg"]);
                LkDataConnection.EmailMessages _lkemail = new LkDataConnection.EmailMessages();
                LkDataConnection.EmailConfiguration _emailConfig = new LkDataConnection.EmailConfiguration();
                LkDataConnection.EmailAttributesDet _attribute = new LkDataConnection.EmailAttributesDet();
                _emailConfig.EmailUserName = "haseenrajput012@gmail.com";
                _emailConfig.EmailPassword = "ngmp xpqr jcas hjrd";
                _emailConfig.SSL = true;

                _emailConfig.SmtpHost = "smtp.gmail.com";
                _emailConfig.SmtpPort = 587;
                _attribute.Mailto = ["1506shreya@gmail.com", "haseencomputer016@gmail.com"];
                _attribute.Subject = "hello";
                _attribute.Mailbody = " hello from body";
                _attribute.IsBodyHtml = false;
                _attribute.Mailbcc = ["1506shreya@gmail.com", "haseencomputer016@gmail.com"];
                _attribute.Mailcc = ["1506shreya@gmail.com", "haseencomputer016@gmail.com"];
                _attribute.AttachmentFiles = ["F:\\vs2022\\sqlAuthJWt\\Quiz_DataBank\\Quiz_DataBank\\wwwroot\\Public\\images\\03c1dafe-0382-4300-b6c1-e7566a383444.png", "F:\\vs2022\\sqlAuthJWt\\Quiz_DataBank\\Quiz_DataBank\\wwwroot\\Public\\images\\0d5ef12c-7256-4cc3-b7eb-981e6765de28.jpeg"];



                _attribute = _lkemail.SendEmail(_emailConfig, _attribute);


                _query = _dc.InsertOrUpdateEntity(newUser, "Users_mst", -1);
                return StatusCode(StatusCodes.Status200OK, new { message = "USer Register successfully"});
                
         

           //     return Ok("USer Register successfully");

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error:{ex.Message}");
            }

        }
        private string GenerateToken(LoginModel users)
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
              new Claim("Role_ID", users.Role_ID.ToString()),


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

        public IActionResult Login(LoginModel user)
        {

            IActionResult response = Unauthorized();
            try
            {
                // string hashedPassword = HashedPassword.HashPassword(user.User_Password);
                string hashedPassword = HashedPassword.EncryptPassword(user.User_Password);

                //string hashedPassword = PasswordUtility.EncryptPassword(user.User_Password);
                // string realp = PasswordUtility.DecryptPassword(hashedPassword);

                //         string query = $"SELECT U.*, R.RoleName, R.Role_ID FROM Users_mst U JOIN Roles_mst R ON U.Role_ID = R.Role_ID WHERE U.User_Email = '{user.User_Email}' AND U.User_Password='{hashedPassword}'";
                string query = $"SELECT U.*,R.* FROM Users_mst U  Join Roles_mst R on U.Role_ID=R.Role_ID WHERE  U.User_Email = '{user.User_Email}' AND U.User_Password='{hashedPassword}'";

                var connection = new LkDataConnection.Connection();

                // DataTable result = _connection.ExecuteQueryWithResult(query);

                var result = connection.bindmethod(query);

                DataTable Table = result._DataTable;
                DataRow userData = Table.Rows.Count > 0 ? Table.Rows[0] : null;



                if (userData == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }


                if (userData["Status"].ToString() != "1")
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


                //string token = GenerateToken(new LoginModel
                //{
                //User_ID = Convert.ToInt32(userData["User_ID"]),
                //    User_Name = userData["User_Name"].ToString(),
                //    Role_ID = Convert.ToInt32(userData["Role_ID"]),

                //    //  userRole = userData["RoleName"].ToString()
                //});
                WebToken _web = new WebToken();
                UserDetails _userdetails = new UserDetails();
                List<KeyDetails> lst = new List<KeyDetails>();
                List<KeyDetails> lst1 = new List<KeyDetails>
                {
                    new KeyDetails
                { KeyName = "User_ID", KeyValue = userData["User_ID"].ToString() },

                                        new KeyDetails
{ KeyName = "Role_ID", KeyValue = userData["Role_ID"].ToString() },


                    new KeyDetails
                { KeyName = "User_Name", KeyValue = userData["User_Name"].ToString() }
                };

                lst.Add(new KeyDetails
                { KeyName = "User_ID", KeyValue = userData["User_ID"].ToString() });
                    lst.Add(new KeyDetails
                    { KeyName = "User_Name", KeyValue = userData["User_Name"].ToString() });

                lst.Add(new KeyDetails
                { KeyName = "Role_ID", KeyValue = userData["Role_ID"].ToString() });
                _userdetails.ListKeydetails = lst1;
                //string[] _Keynames = ["User_ID", "User_Name", "Role_ID"];
                //string[] _Keyvalues = [userData["User_ID"].ToString(), userData["User_Name"].ToString(), userData["Role_ID"].ToString()];
                string token = _web.GenerateToken(new LkDataConnection.WebTokenValidationParameters
                { ValidIssuer = "http://localhost:7242/",
                    ValidAudience = "http://localhost:7242/",
                    IssuerSigningKey = "2Fsk5LBU5j1DrPldtFmLWeO8uZ8skUzwhe3ktVimUE8l=",

                }, new LkDataConnection.UserDetails
                {

                    ListKeydetails = _userdetails.ListKeydetails


                    //  ListKeydetails = _userdetails.ListKeydetails

                    //User_Id = Convert.ToInt32(userData["User_ID"]),
                    //User_Name = userData["User_Name"].ToString(),
                    //Role_Id = Convert.ToInt32(userData["Role_ID"]),
                }); ; ; 
                WebTokenDetails _tokendetails = new WebTokenDetails();
                _tokendetails.Token = token;
                _tokendetails.TokenKeyName = "Role_ID";
                _tokendetails = _web.ExtractTokenInformation(_tokendetails);

                //ExtractTokenInformation(token);
                Console.WriteLine($"Here is the token {token}");

                response = Ok(new
                {
                    token,
                    user_id = userData["User_ID"],
                    Role_ID = userData["Role_ID"],
                    RoleName= userData["RoleName"]

                });

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
            var roleClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "Role_ID");
            if (roleClaim != null)
            {
                string roleId = roleClaim.Value;
                Console.WriteLine($"Role ID: {roleId}");
            }
            var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "User_ID");

          //  var userIdClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
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
      //  [RoleAuthorize("Admin")]

        public IActionResult updateUsers(int User_ID, [FromForm] UsersModel user)
        {
            try
            {
                string wwwRootPath = _hostingEnvironment.WebRootPath;

                string oldImagePath = _connection.GetOldImagePathFromDatabase(User_ID);

                if (!string.IsNullOrEmpty(oldImagePath))
                {
                    string imagePath = Path.Combine(wwwRootPath, "Public/Images", oldImagePath);
                  //  DataAccess.DeleteImage("/Public/Images", oldImagePath);
                }
                var duplicacyChecker = new CheckDuplicacy(_connection);

                bool isDuplicate = duplicacyChecker.CheckDuplicate("Users_mst",
                 new[] { "User_Name" },
                 new[] { user.User_Name },
                 "User_ID", User_ID.ToString());

                if (isDuplicate)
                {
                    return StatusCode(StatusCodes.Status208AlreadyReported, new { message = "Duplicate ! Users exists.", DUP = true });

                }
                if (user.User_Password != null)
                {
                    string hashedPassword = HashedPassword.EncryptPassword(user.User_Password);
                    //string hashedPassword = PasswordUtility.EncryptPassword(user.User_Password);

                    user.User_Password = hashedPassword;
                }
                if(user.User_Name!=null || !user.User_Name.IsNullOrEmpty())
                {
                    var username = new System.Globalization.CultureInfo("en-US", false).TextInfo.ToTitleCase(user.User_Name.ToLower());
                    user.User_Name = username;

                }

                _query = _dc.InsertOrUpdateEntity(user, "Users_mst", User_ID, "User_ID", "wwwroot/Public/Images");
                return StatusCode(StatusCodes.Status200OK, new { message = "Users Updated Successfully", DUP = false });

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
            }
        }
        //[HttpDelete]
        //[Route("deleteUser/{User_ID}")]
        //public IActionResult DeleteUserName(int User_ID)
        //{
        //    try
        //    {

        //        string checkQuery = $"SELECT COUNT(*) AS recordCount FROM Quiz_Transaction_mst WHERE User_ID = {User_ID}";




        //        int result = Convert.ToInt32(_connection.ExecuteScalar(checkQuery));
        //        if (result > 0)
        //        {
        //            return Ok("Can't delete Exists in another table  ");
        //        }
        //        string deleteUserQuery = $"Delete from Users_mst where User_ID='{User_ID}'";

        //        LkDataConnection.Connection.ExecuteNonQuery(deleteUserQuery);
        //        //  _connection.ExecuteQueryWithoutResult(deleteUserQuery);
        //        return Ok("User Deleted successfully");

        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, $"Error{ex.Message}");
        //    }
        //}





        [HttpDelete]
        [Route("deleteUser/{User_ID}")]
        public IActionResult DeleteUserName(int User_ID)
        {
            try
            {
                string checkQuery = $"SELECT COUNT(*) AS recordCount FROM Quiz_Transaction_mst WHERE User_ID = {User_ID}";
                int result = Convert.ToInt32(_connection.ExecuteScalar(checkQuery));

                if (result > 0)
                {
                    return Ok("Can't delete. Exists in another table.");
                }
                _connection.GetSqlConnection().Close();

                string roleCheckQuery = $"SELECT COUNT(*) AS roleCount FROM Users_mst WHERE Role_ID = 5";
                int roleCount = Convert.ToInt32(_connection.ExecuteScalar(roleCheckQuery));
                _connection.GetSqlConnection().Close();

                string currentUserRoleQuery = $"SELECT COUNT(*) AS currentUserRoleCount FROM Users_mst WHERE User_ID = {User_ID} AND Role_ID = 5";
                int currentUserRoleCount = Convert.ToInt32(_connection.ExecuteScalar(currentUserRoleQuery));

                if (roleCount == 1 && currentUserRoleCount == 1)
                {
                    return Ok("Can't delete. This is the only user with Role_ID = 5.");
                }
                _connection.GetSqlConnection().Close();


                string deleteUserQuery = $"DELETE FROM Users_mst WHERE User_ID = {User_ID}";
                LkDataConnection.Connection.ExecuteNonQuery(deleteUserQuery);

                return Ok("User deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        //[HttpGet]
        //[Route("TopUser")]
        //public IActionResult GetTopUser()
        //{
        //    string query = @"
        //SELECT 
        //    u.User_Name,
        //    COUNT(CASE WHEN qat.Answer = q.Correct_Answer THEN 1 END) AS CorrectAnswersCount
        //FROM 
        //    Quiz_AnsTransaction_mst qat
        //INNER JOIN 
        //    Questions_mst q ON qat.Ques_ID = q.Ques_ID0
        //INNER JOIN 
        //    Users_mst u ON qat.User_ID = u.User_ID
        //GROUP BY 
        //    u.User_Name
        //ORDER BY 
        //    CorrectAnswersCount DESC";

        //    DataTable table = _connection.ExecuteQueryWithResult(query);

        //    var topUsers = new List<UsersModel>();
        //    foreach (DataRow row in table.Rows)
        //    {
        //        topUsers.Add(new UsersModel
        //        {
        //            User_Name = row["User_Name"].ToString(),
        //            Is_Correct = Convert.ToInt32(row["CorrectAnswersCount"])
        //        });
        //    }

        //    return Ok(topUsers);
        //}
        [HttpGet]
        [Route("TopUser")]
        public IActionResult GetTopUser()
        {
            string query = @"
    SELECT 
        u.User_Name,
        COUNT(qat.Ques_ID) AS TotalQuestions,
        COUNT(CASE WHEN qat.Answer = q.Correct_Answer THEN 1 END) AS CorrectAnswersCount,
        (ROUND(CAST(COUNT(CASE WHEN qat.Answer = q.Correct_Answer THEN 1 END) AS Float ),1 )/ COUNT(qat.Ques_ID)) * 100 AS AverageCorrectAnswers
    FROM 
        Quiz_AnsTransaction_mst qat
    INNER JOIN 
        Questions_mst q ON qat.Ques_ID = q.Ques_ID
    INNER JOIN 
        Users_mst u ON qat.User_ID = u.User_ID
    GROUP BY 
        u.User_Name
    ORDER BY 
        CorrectAnswersCount DESC";

            DataTable table = _connection.ExecuteQueryWithResult(query);

            var topUsers = new List<UsersModel>();
            foreach (DataRow row in table.Rows)
            {
                topUsers.Add(new UsersModel
                {
                    User_Name = row["User_Name"].ToString(),
                    TotalQuestions = Convert.ToInt32(row["TotalQuestions"]),
                    Is_Correct = Convert.ToInt32(row["CorrectAnswersCount"]),
                    Avg_Correct = Convert.ToDouble(row["AverageCorrectAnswers"])
                });
            }

            return Ok(topUsers);
        }

        [HttpGet]
        [Route("ProfileImage/{User_ID}")]
        public IActionResult GetProfileImage(int User_ID)
        {
            string query = $"SELECT Image FROM Users_mst WHERE User_ID = {User_ID}";
            DataTable Table = _connection.ExecuteQueryWithResult(query);

            if (Table.Rows.Count > 0)
            {
                string imageName = Table.Rows[0]["Image"]?.ToString();

                if (!string.IsNullOrEmpty(imageName))
                {
                    var imageUrl = $"http://192.168.1.54:7243/public/images/{imageName}";

                    return Ok(new { ImageUrl = imageUrl });
                }
                else
                {
                    return NotFound(new { Message = "No image found for this user." });
                }
            }
            else
            {
                return NotFound(new { Message = "User not found." });
            }
        }
        [HttpGet]
        [Route("GetBestAndWorstQuizes/{User_ID}")]
        public IActionResult GetBestAndWorstQuizes(int User_ID)
        {
            string bestQuizzesQuery = $@"
    SELECT TOP 3 
     qt.Quiz_Name,
     qt.Quiz_Date,
     COUNT(qat.Ques_ID) AS TotalQuestions,
     SUM(CASE WHEN qat.Answer = q.Correct_Answer THEN 1 ELSE 0 END) AS CorrectAnswersCount,
     (CAST(SUM(CASE WHEN qat.Answer = q.Correct_Answer THEN 1 ELSE 0 END) AS FLOAT) / COUNT(qat.Ques_ID)) * 100 AS ScorePercentage
 FROM 
     Quiz_AnsTransaction_mst qat
 INNER JOIN 
     Questions_mst q ON qat.Ques_ID = q.Ques_ID
 INNER JOIN 
     Quiz_Transaction_mst qt ON qat.Ques_ID = qt.Ques_ID AND qat.User_ID = qt.User_ID
 WHERE 
     qat.User_ID = {User_ID} AND qat.Answer_ID IS NOT NULL 
  AND qat.Quiz_DateTime IS NOT NULL
  AND qt.Quiz_Name = qat.Quiz_Name  
  AND qat.User_ID = qt.User_ID     
  AND qat.Ques_ID = qt.Ques_ID  
 GROUP BY 
     qt.Quiz_Name, qt.Quiz_Date
 ORDER BY 
     ScorePercentage DESC;
    ";

            string worstQuizzesQuery = $@"
    SELECT TOP 3 
     qt.Quiz_Name,
     qt.Quiz_Date,
     COUNT(qat.Ques_ID) AS TotalQuestions,
     SUM(CASE WHEN qat.Answer = q.Correct_Answer THEN 1 ELSE 0 END) AS CorrectAnswersCount,
     (CAST(SUM(CASE WHEN qat.Answer = q.Correct_Answer THEN 1 ELSE 0 END) AS FLOAT) / COUNT(qat.Ques_ID)) * 100 AS ScorePercentage
 FROM 
     Quiz_AnsTransaction_mst qat
 INNER JOIN 
     Questions_mst q ON qat.Ques_ID = q.Ques_ID
 INNER JOIN 
     Quiz_Transaction_mst qt ON qat.Ques_ID = qt.Ques_ID AND qat.User_ID = qt.User_ID
 WHERE 
     qat.User_ID = {User_ID} AND qat.Answer_ID IS NOT NULL 
  AND qat.Quiz_DateTime IS NOT NULL
  AND qt.Quiz_Name = qat.Quiz_Name  
  AND qat.User_ID = qt.User_ID     
  AND qat.Ques_ID = qt.Ques_ID  
 GROUP BY 
     qt.Quiz_Name, qt.Quiz_Date
 ORDER BY 
     ScorePercentage ASC;
    ";

            DataTable bestTable = _connection.ExecuteQueryWithResult(bestQuizzesQuery);
            DataTable worstTable = _connection.ExecuteQueryWithResult(worstQuizzesQuery);

            
            var bestQuizResults = new List<QuizTransactionModel>();
            foreach (DataRow row in bestTable.Rows)
            {
                bestQuizResults.Add(new QuizTransactionModel
                {
                    Quiz_Date = Convert.ToDateTime(row["Quiz_Date"]),
                    Total_Questions = Convert.ToInt32(row["TotalQuestions"]),
                    CorrectAnswers = Convert.ToInt32(row["CorrectAnswersCount"]),
                    Score_Percentage = Convert.ToDouble(row["ScorePercentage"])
                });
            }

            var worstQuizResults = new List<QuizTransactionModel>();
            foreach (DataRow row in worstTable.Rows)
            {
                worstQuizResults.Add(new QuizTransactionModel
                {
                    Quiz_Date = Convert.ToDateTime(row["Quiz_Date"]),
                    Total_Questions = Convert.ToInt32(row["TotalQuestions"]),
                    CorrectAnswers = Convert.ToInt32(row["CorrectAnswersCount"]),
                    Score_Percentage = Convert.ToDouble(row["ScorePercentage"])
                });
            }

            return Ok(new
            {
                BestQuizes = bestQuizResults,
                WorstQuizes = worstQuizResults
            });
        }



    }


}
