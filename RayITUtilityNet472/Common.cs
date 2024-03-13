using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace RayITUtilityNet472
{
    public static class Common
    {
        public static string AES_KEY = "RayIT@20150423";
        public static string AES_IV = "Netcast2MP4";

        #region AES function

        /* http://blog.wahahajk.com/2008/08/c-demo-aes-3des.html */

        /// <summary>
        /// AES加密函式
        /// </summary>
        /// <param name="string_secretContent">欲加密字串</param>
        /// <param name="string_key">密鑰(可使用Common.AES_KEY)</param>
        /// <param name="string_iv">初始向量(可使用Common.AES_IV)</param>
        /// <returns>Base64加密後字串</returns>
        public static string AESEncrypt(string string_secretContent, string string_key, string string_iv)
        {
            //密碼轉譯一定都是用byte[] 所以把string都換成byte[]
            byte[] byte_secretContent = Encoding.UTF8.GetBytes(string_secretContent);
            byte[] byte_key = Encoding.UTF8.GetBytes(string_key);
            byte[] byte_iv = Encoding.UTF8.GetBytes(string_iv);

            //加解密函數的key通常都會有固定的長度 而使用者輸入的key長度不定 因此用hash過後的值當做key
            SHA256CryptoServiceProvider provider_SHA256 = new SHA256CryptoServiceProvider();
            MD5CryptoServiceProvider provider_MD5 = new MD5CryptoServiceProvider();
            byte[] byte_key_SHA256 = provider_SHA256.ComputeHash(byte_key); //32B = 256b
            byte[] byte_iv_MD5 = provider_MD5.ComputeHash(byte_iv); //16B = 128b

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(byte_key_SHA256, byte_iv_MD5);

                byte[] output = encryptor.TransformFinalBlock(byte_secretContent, 0, byte_secretContent.Length);
                return Convert.ToBase64String(output);
            }
        }

        /// <summary>
        /// AES解密函式
        /// </summary>
        /// <param name="str_ciphertext">Base64加密後字串</param>
        /// <param name="string_key">密鑰(可使用Common.AES_KEY)</param>
        /// <param name="string_iv">初始向量(可使用Common.AES_IV)</param>
        /// <returns>原始字串</returns>
        public static string AESDecrypt(string str_ciphertext, string string_key, string string_iv)
        {
            byte[] byte_ciphertext = Convert.FromBase64String(str_ciphertext);

            //密碼轉譯一定都是用byte[] 所以把string都換成byte[]
            byte[] byte_key = Encoding.UTF8.GetBytes(string_key);
            byte[] byte_iv = Encoding.UTF8.GetBytes(string_iv);

            //加解密函數的key通常都會有固定的長度 而使用者輸入的key長度不定 因此用hash過後的值當做key
            SHA256CryptoServiceProvider provider_SHA256 = new SHA256CryptoServiceProvider();
            MD5CryptoServiceProvider provider_MD5 = new MD5CryptoServiceProvider();
            byte[] byte_key_SHA256 = provider_SHA256.ComputeHash(byte_key); //32B = 256b
            byte[] byte_iv_MD5 = provider_MD5.ComputeHash(byte_iv); //16B = 128b

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(byte_key_SHA256, byte_iv_MD5);

                byte[] byte_secretContent = decryptor.TransformFinalBlock(byte_ciphertext, 0, byte_ciphertext.Length);
                return Encoding.UTF8.GetString(byte_secretContent);
            }
        }

        #endregion AES function

        /// <summary>
        /// 取得檔案檢查碼-SHA256
        /// </summary>
        /// <param name="filePath">檔案路徑</param>
        /// <returns>Hex字串（小寫，長度64）</returns>
        public static string GetCheckSum_SHA256(string filePath)
        {
            using (SHA256 SHA256 = SHA256Managed.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return BitConverter.ToString(SHA256.ComputeHash(fileStream)).ToLower().Replace("-", "");
            }
        }
    }
}
