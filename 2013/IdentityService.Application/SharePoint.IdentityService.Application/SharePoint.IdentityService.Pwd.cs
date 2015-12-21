//******************************************************************************************************************************************************************************************//
// Copyright (c) 2015 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Reflection;

namespace SharePoint.IdentityService
{
    // Fake Class, used to Know if password is always encrypted
    public class SharePointIdentityCryptographicException : CryptographicException
    {
        public SharePointIdentityCryptographicException(string text)
            : base(text)
        {
        }
    }

    internal class CipherUtility
    {
        public static string Encrypt<T>(string value, string password, string salt)  where T : SymmetricAlgorithm, new()
        {
           // DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            DeriveBytes rgb = new PBKDF2(Encoding.Unicode.GetBytes(password), Encoding.Unicode.GetBytes(salt), 1);
            SymmetricAlgorithm algorithm = new T();
            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateEncryptor(rgbKey, rgbIV);

            using (MemoryStream buffer = new MemoryStream())
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    using (StreamWriter writer = new StreamWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(value, 0, value.Length);
                    }
                }
                return Convert.ToBase64String(buffer.ToArray());
            }
        }

        public static string Decrypt<T>(string text, string password, string salt) where T : SymmetricAlgorithm, new()
        {
           // DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            DeriveBytes rgb = new PBKDF2(Encoding.Unicode.GetBytes(password), Encoding.Unicode.GetBytes(salt), 1);

            SymmetricAlgorithm algorithm = new T();

            byte[] rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            byte[] rgbIV = rgb.GetBytes(algorithm.BlockSize >> 3);

            ICryptoTransform transform = algorithm.CreateDecryptor(rgbKey, rgbIV);

            using (MemoryStream buffer = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (CryptoStream stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }

    public static class PasswordManager
    {
        /// <summary>
        /// Encrypt encript method implementation
        /// </summary>
        public static string Encrypt(string plainStr, string KeyString)         
        {    
            if (plainStr.StartsWith("0x01"))
                return plainStr;
            string cipherText = CipherUtility.Encrypt<AesManaged>(plainStr, KeyString, "BABE");
            return "0x01"+cipherText; 
        } 
 
        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public static string Decrypt(string encryptedText, string KeyString)  
        {
            if (!encryptedText.StartsWith("0x01"))
                throw new SharePointIdentityCryptographicException("Message Unknown ! or never never encrypted by SharePoint Indentity Service !");
            encryptedText = encryptedText.Substring(4);
            string cipherText = CipherUtility.Decrypt<AesManaged>(encryptedText, KeyString, "BABE");
            return cipherText;  
        } 
     }

    /// <summary>         
    /// Provided an implementation of Rfc2898DeriveBytes accessable via the IPasswordDerivedBytes         
    /// interface.  One primary difference in GetBytes() ensures that the number of bytes         
    /// generated are always rounded to hash size, thus GetBytes(4) + GetBytes(4) != GetBytes(8)         
    /// </summary>         
    public class PBKDF2 : System.Security.Cryptography.Rfc2898DeriveBytes, IPasswordDerivedBytes         
    {                 
        /// <summary>                 
        /// Constructs the Rfc2898DeriveBytes implementation.                 
        /// </summary>                 
        public PBKDF2(byte[] password, byte[] salt, int iterations): base(password, salt.ToArray(), iterations)                 
        { 
        }                  
        
        /// <summary>                 
        /// Overloaded, The base implementation is broken for length > 20, further the RFC doesnt                  
        /// support lenght > 20 and stipulates that the operation should fail.                 
        /// </summary>                 
        public override byte[] GetBytes(int cb)                 
        {                         
            byte[] buffer = new byte[cb];                         
            for (int i = 0; i < cb; i += 20)                         
            {                                 
                int step = Math.Min(20, cb - i);                                 
                Array.Copy(base.GetBytes(20), 0, buffer, i, step);                         
            }                         
            return buffer;                 
        }  

#if NET20 || NET35 
        // NOTE: .NET 4.0 finally implemented                 
        /// <summary>                 
        /// Disposes of the object                 
        /// </summary>                 
        public void Dispose()                 
        {                         
            base.Salt = new byte[8];                         
            base.IterationCount = 1;                          
            //The base doesn't clear the key'd hash, which contains the password in clear text when < 20 bytes                         
            FieldInfo f_hmacsha1 = GetType().BaseType.GetField("m_hmacsha1", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);                         
            if (f_hmacsha1 != null)                         
            {                                 
                HMACSHA1 m_hmacsha1 = f_hmacsha1.GetValue(this) as HMACSHA1;                                 
                m_hmacsha1.Clear();                         
            }                 
        } 
#endif     
    } 

    public interface IPasswordDerivedBytes : IDisposable         
    {                 
        ///<summary>                 
        ///     Gets or sets the number of iterations for the operation.                 
        ///</summary>                 
        int IterationCount { get; set; }                 

        ///                 
        ///<summary>                 
        ///     Gets or sets the key salt value for the operation.                 
        ///</summary>                 
        byte[] Salt { get; set; }                  
        
        ///<summary>                 
        ///     Returns a pseudo-random key from a password, salt and iteration count.                 
        ///</summary>                 
        byte[] GetBytes(int cb);     
            
        ///<summary>                 
        ///     Resets the state of the operation.                 
        ///</summary>                 
        void Reset();         
    }  
}


