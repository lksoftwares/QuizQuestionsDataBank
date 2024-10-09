namespace Quiz_DataBank
{
    public class EncryptYourConnection
    {
        public void EncryptAndDisplayConnectionString()
        {
            string connectionString = "Server=192.168.1.60;Database=Quiz_Databank;User Id=sa;Password='1';max pool size=20000000;TrustServerCertificate=True;Connect Timeout=30000;";

            string encryptedConnectionString = EncryptionHelper.Encrypt(connectionString);

            Console.WriteLine(encryptedConnectionString);
        }
    }
}
