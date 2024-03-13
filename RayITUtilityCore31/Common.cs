using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RayITUtilityCore31
{
    public static class Common
    {
        /// <summary>
        /// 取得隨機密碼
        /// 12碼/英文大寫/英文小寫/數字/特殊符號!#$%&*@
        /// </summary>
        /// <returns></returns>
        public static string GetRandomPassword()
        {
            bool includeLowercase = true;
            bool includeUppercase = true;
            bool includeNumeric = true;
            bool includeSpecial = true;
            bool includeSpaces = false;
            int lengthOfPassword = 12;

            string password = PasswordGenerator.GeneratePassword(includeLowercase, includeUppercase, includeNumeric, includeSpecial, includeSpaces, lengthOfPassword);

            while (!PasswordGenerator.PasswordIsValid(includeLowercase, includeUppercase, includeNumeric, includeSpecial, includeSpaces, password))
            {
                password = PasswordGenerator.GeneratePassword(includeLowercase, includeUppercase, includeNumeric, includeSpecial, includeSpaces, lengthOfPassword);
            }

            return password;
        }

        /// <summary>
        /// 取得長度16的Guid()
        /// </summary>
        /// <returns>字串(char(16)</returns>
        /// https://www.itread01.com/content/1549376313.html
        public static string GetGuid16()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        /// <summary>
        /// 取得加鹽雜湊碼(SHA512取部分32碼)
        /// SHA512的Base64字串原始長度為88碼
        /// </summary>
        /// <param name="orgStr">原始字串</param>
        /// <param name="saltStr">鹽字串</param>
        /// <returns>Base64字串(char(32))</returns>
        public static string GetSaltedHashCode(string orgStr, string saltStr)
        {
            byte[] passwordAndSaltBytes = Encoding.UTF8.GetBytes(orgStr + saltStr);
            byte[] hashBytes = new SHA512Managed().ComputeHash(passwordAndSaltBytes);
            string hashString = Convert.ToBase64String(hashBytes);

            return hashString.Substring(0, 32);
        }

        /// <summary>
        /// 取得驗證碼物件
        /// </summary>
        /// <returns></returns>
        public static ValidateCode GetValidateCode()
        {
            string code = ValidateCodeHelper.CreateValidateCode(5);
            byte[] bytes = ValidateCodeHelper.CreateValidateGraphic(code);
            var vCode = new ValidateCode()
            {
                Code = code,
                Bytes = bytes
            };
            return vCode;
        }

        /// <summary>
        /// 移除檔名中不合法字元
        /// </summary>
        /// <param name="input">原始字串</param>
        /// <param name="replacement">不合法字元取代字串</param>
        /// <returns></returns>
        public static string RemoveIllegalFileNameChars(string input, string replacement = "")
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(input, replacement);
        }

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
    }
}