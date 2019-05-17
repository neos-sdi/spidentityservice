//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //
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
using System.Security.Cryptography.X509Certificates;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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

    /// <summary>
    /// PasswordManager2 class implmentation
    /// </summary>
    public class PasswordManager : IDisposable
    {
        private X509Certificate2 _cert = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordManager()
        {
            _cert = GetSharePointCertificate();
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public PasswordManager(string thumbprint)
        {
            _cert = GetCertificate(thumbprint, StoreLocation.LocalMachine);
        }

        /// <summary>
        /// Certificate property
        /// </summary>
        public X509Certificate2 Certificate
        {
            get { return _cert; }
            set { _cert = value; }
        }

        /// <summary>
        /// Encrypt method
        /// </summary>
        public string Encrypt(string plainText)
        {
            try
            {
                if (_cert == null)
                    throw new Exception("Invalid encryption certificate !");
                byte[] plainBytes = SerializeToStream(plainText).ToArray();
                byte[] encryptedBytes = null;
                var key = _cert.GetRSAPublicKey();
                if (key == null)
                    throw new CryptographicException("Invalid public Key !");

                if (key is RSACng)
                    encryptedBytes = ((RSACng)key).Encrypt(plainBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    encryptedBytes = ((RSACryptoServiceProvider)key).Encrypt(plainBytes, true);
                return System.Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                throw new CryptographicException(ex.Message);
            }
        }

        /// <summary>
        /// Decrypt method
        /// </summary>
        public string Decrypt(string encryptedText)
        {
            try
            {
                if (_cert == null)
                    throw new Exception("Invalid decryption certificate !");
                byte[] encryptedBytes = System.Convert.FromBase64CharArray(encryptedText.ToCharArray(), 0, encryptedText.Length);
                byte[] decryptedBytes = null;
                var key = _cert.GetRSAPrivateKey();
                if (key == null)
                    throw new CryptographicException("Invalid private Key !");

                if (key is RSACng)
                    decryptedBytes = ((RSACng)key).Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);
                else
                    decryptedBytes = ((RSACryptoServiceProvider)key).Decrypt(encryptedBytes, true);

                MemoryStream mem = new MemoryStream(decryptedBytes);
                return DeserializeFromStream(mem);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return default(string);
            }
            catch (Exception ex)
            {
                throw new CryptographicException(ex.Message);
            }
        }

        /// <summary>
        /// Encrypt encript method implementation
        /// </summary>
        public string SymetricEncrypt(string plainStr)
        {
            if (_cert == null)
                throw new Exception("Invalid decryption certificate !");
            if (plainStr.StartsWith("0x01"))
                return plainStr;
            string cipherText = CipherUtility.Encrypt<AesManaged>(plainStr, _cert.Thumbprint, "BABE");
            return "0x01" + cipherText;
        }

        /// <summary>
        /// Decrypt method implementation
        /// </summary>
        public string SymetricDecrypt(string encryptedText)
        {
            if (_cert == null)
                throw new Exception("Invalid decryption certificate !");
            if (!encryptedText.StartsWith("0x01"))
                throw new SharePointIdentityCryptographicException("Message Unknown ! or never never encrypted by SharePoint Indentity Service !");
            encryptedText = encryptedText.Substring(4);
            string cipherText = CipherUtility.Decrypt<AesManaged>(encryptedText, _cert.Thumbprint, "BABE");
            return cipherText;
        }

        /// <summary>
        /// SerializeToStream
        /// </summary>
        private MemoryStream SerializeToStream(string objectType)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, objectType);
            return stream;
        }

        /// <summary>
        /// DeserializeFromStream
        /// </summary>
        private string DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object objectType = formatter.Deserialize(stream);
            return (string)objectType;
        }

        /// <summary>
        /// GetCertificate method implementation
        /// </summary>
        private X509Certificate2 GetCertificate(string thumprint, StoreLocation location)
        {
            X509Certificate2 data = null;
            // X509Store store = new X509Store(location);
            X509Store store = new X509Store("SharePoint", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindByThumbprint, thumprint, false);

                foreach (X509Certificate2 x509 in findCollection)
                {
                    data = x509;
                    break;
                }
            }
            catch
            {
                data = null;
            }
            finally
            {
                store.Close();
            }
            return data;
        }

        /// <summary>
        /// This method is used to fetch certificate details insatalled on the machine
        /// using Cryptography 
        /// </summary>
        public static X509Certificate2 GetSharePointCertificate()
        {
            X509Certificate2 data = null;
            //Create certificate store object and open the same
            X509Store store = new X509Store("SharePoint", StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            try
            {
                //Open certificate collection
                X509Certificate2Collection collection = (X509Certificate2Collection)store.Certificates;
                X509Certificate2Collection findCollection = (X509Certificate2Collection)collection.Find(X509FindType.FindBySubjectName, "SharePoint Security Token Service", false);

                //Iterate through all certificates in the collection
                foreach (X509Certificate2 x509 in findCollection)
                {
                    //Fetch the raw Data from certificate object
                    data = x509;
                    break;
                }
            }
            catch
            {
                data = null;
            }
            finally
            {
                store.Close();
            }
            return data;
        }

        /// <summary>
        /// Dispose IDispose method implementation
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose method implementation
        /// </summary>
        internal virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cert != null)
                    _cert.Reset();
            }
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


