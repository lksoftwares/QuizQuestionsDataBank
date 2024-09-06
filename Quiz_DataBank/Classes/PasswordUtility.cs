using System;
using System.Security.Cryptography;
using System.Text;

namespace Quiz_DataBank.Classes
{
    public class PasswordUtility
    {
        private static readonly byte[] encryptionKey = GenerateEncryptionKey.GenerateEncryptKey(); // Use byte array

        public static string EncryptPassword(string password)
        {
            using (Aes aes = Aes.Create())
            {
                Console.WriteLine(encryptionKey);

                aes.Key = encryptionKey; // Use the byte array
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7; // Ensure padding is set
                aes.GenerateIV(); // Generates a new IV for encryption
                byte[] iv = aes.IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, iv);
                using (var ms = new System.IO.MemoryStream())
                {
                    ms.Write(iv, 0, iv.Length);  // Store the IV at the beginning of the encrypted data
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        byte[] inputBytes = Encoding.UTF8.GetBytes(password);
                        cs.Write(inputBytes, 0, inputBytes.Length);
                    }
                    return Convert.ToBase64String(ms.ToArray()); // Combine IV + Encrypted data
                }
            }
        }

        public static string DecryptPassword(string encryptedPassword)
        {
            try
            {
                byte[] fullCipher = Convert.FromBase64String(encryptedPassword);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = encryptionKey; // Use the byte array
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7; // Ensure padding is set

                    // Extract the IV from the encrypted data (first 16 bytes)
                    byte[] iv = new byte[16];
                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);

                    // Extract the actual encrypted data (after the IV)
                    byte[] cipherBytes = new byte[fullCipher.Length - iv.Length];
                    Array.Copy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, iv);
                    using (var ms = new System.IO.MemoryStream(cipherBytes))
                    {
                        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (var reader = new System.IO.StreamReader(cs))
                            {
                                return reader.ReadToEnd(); // Return decrypted password
                            }
                        }
                    }
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("Decryption failed: " + e.Message);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: " + e.Message);
                return null;
            }
        }
        private static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length) return false;
            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i]) return false;
            }
            return true;
        }
    }
}
