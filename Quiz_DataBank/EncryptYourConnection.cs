using LkDataConnection;
namespace Quiz_DataBank
{

    public class EncryptYourConnection
    {
        public void EncryptAndDisplayConnectionString()
        {
            //string connectionString = "Server=192.168.1.53;Database=QuizBank;User Id=sa;Password='1'; max pool size = 20000000;TrustServerCertificate=True;Connect Timeout=30000;";
           string Server = "192.168.1.50";

            LkDataConnection.EncryptDecrypt _lkencr = new LkDataConnection.EncryptDecrypt();
            // string encrstr = _lkencr.Encrypt("ABC", connectionString);
            string encrstr = _lkencr.Encrypt("ABC", Server);
            //string encryptedConnectionString = EncryptionHelper.Encrypt(connectionString);

            Console.WriteLine(encrstr);
        }
    }
}
