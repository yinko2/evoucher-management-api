using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace eVoucherAPI.Util
{
    public class Encryption
    {
        private static string _ClientEncryptionKey = Startup.StaticConfig.GetSection("Encryption:ClientEncryptionKey").Value;
        private static string _ClientEncryptionSalt = Startup.StaticConfig.GetSection("Encryption:ClientEncryptionSalt").Value;
        public static string DecryptClient_String(string cipherText) {
            return DecryptClient_CBC_256(cipherText);
        }

        private static string EncryptClient_CBC_256(string PlainText, string EncryptionKey = "")
        {
            
            if(PlainText == "") 
                return "";

            string encryptString = "";
            try
            {
                if (EncryptionKey.Trim() == "") EncryptionKey = _ClientEncryptionKey;
                var bsaltkey = Encoding.UTF8.GetBytes(_ClientEncryptionSalt);
                byte[] clearBytes = Encoding.UTF8.GetBytes(PlainText);
                
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, bsaltkey, 1000, HashAlgorithmName.SHA256);
                    encryptor.Key = pdb.GetBytes(32);  //256 bit Key
                    encryptor.IV = GenerateRandomBytes(16);
                    encryptor.Mode = CipherMode.CBC;
                    encryptor.Padding = PaddingMode.PKCS7;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(clearBytes, 0, clearBytes.Length);
                        }
                        byte[] result = MergeArrays(encryptor.IV, ms.ToArray());  //append IV to cipher, so cipher length will longer
                        encryptString = Convert.ToBase64String(result);
                    }
                }
                return encryptString;
            }
            catch (Exception ex)
            {
                Globalfunction.WriteSystemLog("Encrypt_CBC_256: " + ex.Message);
            }
            return encryptString;
        }
        private static string DecryptClient_CBC_256(string cipherText)
        {
            if(cipherText == "")
                return "";

            string plainText = "";
            try
            {
                
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                using (Aes encryptor = Aes.Create())
                {
                    //byte[] ClientKey = Encoding.UTF8.GetBytes(_ClientEncryptionKey);
                    byte[] ClientSalt = Encoding.UTF8.GetBytes(_ClientEncryptionSalt);

                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(_ClientEncryptionKey, ClientSalt, 1000, HashAlgorithmName.SHA256);
                    encryptor.Mode = CipherMode.CBC;
                    encryptor.Padding = PaddingMode.PKCS7;
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = cipherBytes.Take(16).ToArray();
                    cipherBytes = cipherBytes.Skip(16).ToArray();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                        }
                        plainText = Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
                return plainText;
            }
            catch (Exception ex)
            {
                Globalfunction.WriteSystemLog("DecryptClient_CBC_256: " + ex.Message);
            }
            return plainText;
        }

        private static byte[] GenerateRandomBytes(int numberOfBytes)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            var randomBytes = new byte[numberOfBytes];
            rng.GetBytes(randomBytes);
            return randomBytes;
        }

        private static byte[] MergeArrays(params byte[][] arrays)
        {
            var merged = new byte[arrays.Sum(a => a.Length)];
            var mergeIndex = 0;
            for (int i = 0; i < arrays.GetLength(0); i++)
            {
                arrays[i].CopyTo(merged, mergeIndex);
                mergeIndex += arrays[i].Length;
            }

            return merged;
        }
    }
}