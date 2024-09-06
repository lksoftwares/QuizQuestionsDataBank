using System.Security.Cryptography;

public class GenerateEncryptionKey
{
    public static byte[] GenerateEncryptKey()
    {
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] key = new byte[32]; 
            rng.GetBytes(key);
            return key; 
        }
    }
}
