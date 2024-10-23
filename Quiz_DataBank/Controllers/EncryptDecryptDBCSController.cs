using Azure.Core;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Quiz_DataBank.Model;

namespace Quiz_DataBank.Controllers
{
    [ApiController]
    public class EncryptDecryptDBCSController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        LkDataConnection.EncryptDecrypt _lkencr = new LkDataConnection.EncryptDecrypt();


        public EncryptDecryptDBCSController(IConfiguration configuration)
        {
            _configuration = configuration;


        }
      
        [HttpGet]
        [Route("decrypt")]
        public IActionResult GetEncryptString()
        {
            string Server = _configuration["StringData:server"];
            string DataBase = _configuration["StringData:database"];
            string User = _configuration["StringData:user"];
            string Password = _configuration["StringData:password"];

            string DecrServer = _lkencr.Decrypt("ABC", Server);
            string DecrDataBase = _lkencr.Decrypt("ABC", DataBase);
            string DecrUser = _lkencr.Decrypt("ABC", User);
            string DecrPassword = _lkencr.Decrypt("ABC", Password);
            return Ok(
new
{
    DecryptedServer = DecrServer,
    DecryptedUser = DecrUser,
    DecryptedDataBase = DecrDataBase,
    DecryptedPassword = DecrPassword

});


}
        [HttpPost]
        [Route("/encrypt")]
        public IActionResult GetEncrypted([FromBody] EncryptDecryptDBCSModel _Encrypted)
        {

            string EncryptedDataBase= _lkencr.Encrypt("ABC",
                 _Encrypted.EncryptedDatabase
            );
            string EncryptedServer = _lkencr.Encrypt("ABC",
               _Encrypted.EncryptedServer
          );
            string EncryptedPassword = _lkencr.Encrypt("ABC",
           _Encrypted.EncryptedPassword
      ); string EncryptedUser = _lkencr.Encrypt("ABC",
               _Encrypted.EncryptedUser
          );
            return Ok(new { EncryptServer = EncryptedServer, EncryptDataBase= EncryptedDataBase, EncryptPassword = EncryptedPassword , EncryptUser = EncryptedUser });


        }
        public class EncryptDecryptDBCSModel
        {
            public string EncryptedServer { get; set; }
            public string EncryptedUser { get; set; }
            public string EncryptedPassword { get; set; }
            public string EncryptedDatabase { get; set; }
        }

    } }
