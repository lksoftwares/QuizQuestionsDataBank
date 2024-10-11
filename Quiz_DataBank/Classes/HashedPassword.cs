//using System.Security.Cryptography;
//using System.Text;

//namespace Quiz_DataBank.Classes
//{
//    public class HashedPassword
//    {
//        public static string HashPassword(string password)
//        {
//            using (MD5 md5 = MD5.Create())
//            {
//                byte[] inputBytes = Encoding.UTF8.GetBytes(password);
//                byte[] hashedBytes = md5.ComputeHash(inputBytes);

//                StringBuilder builder = new StringBuilder();
//                for (int i = 0; i < hashedBytes.Length; i++)
//                {
//                    builder.Append(hashedBytes[i].ToString("x2"));
//                }

//                return builder.ToString();
//            }
//        }
//    }
//}

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class HashedPassword
{
    private static readonly string key = "1234567890abcdef"; 
    private static readonly string iv = "abcdef1234567890";  


    public static string EncryptPassword(string plainTextPassword)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter sw = new StreamWriter(cs))
                    {
                        sw.Write(plainTextPassword);
                    }
                }

                byte[] encrypted = ms.ToArray();
                return Convert.ToBase64String(encrypted);
            }
        }
    }

    public static string DecryptPassword(string encryptedPassword)
    {
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(encryptedPassword)))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }
    }
}
