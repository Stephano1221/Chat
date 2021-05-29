using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    public class Encryption
    {
        int rsaKeySize = 2048;
        public string keyContainerName;

        public RSAParameters rsaParametersPublicKey;
        public RSAParameters rsaParametersPrivateAndPublicKey;

        public byte[] aesEncryptedKey;
        public byte[] aesEncryptedIv;
        private byte[] aesUnencryptedKey;
        private byte[] aesUnencryptedIv;

        public Encryption()
        {

        }

        #region RSA
        public byte[] RsaEncryptDecrypt(byte[] data, string keyContainerName, RSAParameters rsaParameters, bool oAEPPadding, bool encrypt)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize, cspParameters))
                    {
                        if (encrypt)
                        {
                            return rsaCryptoServiceProvider.Encrypt(data, oAEPPadding);
                        }
                        else
                        {
                            return rsaCryptoServiceProvider.Decrypt(data, oAEPPadding);
                        }
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize))
                    {
                        rsaCryptoServiceProvider.ImportParameters(rsaParameters);
                        if (encrypt)
                        {
                            return rsaCryptoServiceProvider.Encrypt(data, oAEPPadding);
                        }
                        else
                        {
                            return rsaCryptoServiceProvider.Decrypt(data, oAEPPadding);
                        }
                    }
                }
            }
            catch(CryptographicException)
            {
                return null;
            }
        }

        public void RsaGenerateKey(string keyContainerName)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize, cspParameters);
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize))
                    {
                        rsaParametersPublicKey = rsaCryptoServiceProvider.ExportParameters(false);
                        rsaParametersPrivateAndPublicKey = rsaCryptoServiceProvider.ExportParameters(true);
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public void RsaImportXmlKey(string keyContainerName, string xmlKey)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize, cspParameters))
                    {
                        rsaCryptoServiceProvider.FromXmlString(xmlKey);
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize))
                    {
                        rsaCryptoServiceProvider.FromXmlString(xmlKey);
                        rsaParametersPublicKey = rsaCryptoServiceProvider.ExportParameters(false);
                        rsaParametersPrivateAndPublicKey = rsaCryptoServiceProvider.ExportParameters(true);
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public string RsaExportXmlKey(string keyContainerName, bool includePrivateParameters)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize, cspParameters))
                    {
                        return rsaCryptoServiceProvider.ToXmlString(includePrivateParameters);
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize))
                    {
                        if (includePrivateParameters)
                        {
                            rsaCryptoServiceProvider.ImportParameters(rsaParametersPrivateAndPublicKey);
                        }
                        else
                        {
                            rsaCryptoServiceProvider.ImportParameters(rsaParametersPublicKey);
                        }
                        return rsaCryptoServiceProvider.ToXmlString(includePrivateParameters);
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public void RsaDeleteKey(string keyContainerName)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize, cspParameters))
                    {
                        rsaCryptoServiceProvider.PersistKeyInCsp = false;
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize))
                    {
                        rsaParametersPublicKey = new RSAParameters();
                        rsaParametersPrivateAndPublicKey = new RSAParameters();
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }
        #endregion

        #region AES
        public byte[] AesEncryptDecrypt(byte[] data, (byte[] key, byte[] iv) keyandIv, bool encrypt)
        {
            byte[] changedData;
            using (Aes aes = Aes.Create())
            {
                aes.Key = keyandIv.key;
                aes.IV = keyandIv.iv;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cryptoTransform;
                if (encrypt)
                {
                    cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);
                }
                else
                {
                    cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);
                }
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(data);
                        /*Use only if input data is a string
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(data);
                        }*/
                    }
                    changedData = memoryStream.ToArray();
                }
            }
            return changedData;
        }

        public void AesGenerateKey()
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        aesEncryptedKey = ProtectedData.Protect(aes.Key, null, DataProtectionScope.CurrentUser);
                        aesEncryptedIv = ProtectedData.Protect(aes.IV, null, DataProtectionScope.CurrentUser);
                    }
                    else
                    {
                        aesUnencryptedKey = aes.Key;
                        aesUnencryptedIv = aes.IV;
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public void AesImportKeyAndIv(string xmlKey)
        {
            try
            {
                string[] parts = xmlKey.Split(' ', 2);
                byte[] key = Convert.FromBase64String(parts[0]);
                byte[] iv = Convert.FromBase64String(parts[1]);
                //TODO: Extract key parts from xmlKey for use below
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    aesEncryptedKey = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
                    aesEncryptedIv = ProtectedData.Protect(iv, null, DataProtectionScope.CurrentUser);
                }
                else
                {
                    aesUnencryptedKey = key;
                    aesUnencryptedIv = iv;
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public (byte[] aesDecryptedKey, byte[] aesDecryptedIv) AesGetKeyAndIv(byte[] encryptedKey, byte[] encryptedIv)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return (ProtectedData.Unprotect(encryptedKey, null, DataProtectionScope.CurrentUser), ProtectedData.Unprotect(encryptedIv, null, DataProtectionScope.CurrentUser));
                }
                else
                {
                    return (aesUnencryptedKey, aesUnencryptedIv);
                }
            }
            catch (CryptographicException)
            {
                throw;
            }
        }

        public string AesExportKeyAndIv()
        {
            try
            {
                //TODO: Put key and IV into an XML string
                (byte[] key, byte[] iv) keyAndIv = AesGetKeyAndIv(aesEncryptedKey, aesEncryptedIv);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return $"{Convert.ToBase64String(keyAndIv.key)} {Convert.ToBase64String(keyAndIv.iv)}";
                    //aesEncryptedKey = ProtectedData.Unprotect(keyandIv.key, null, DataProtectionScope.CurrentUser);
                    //aesEncryptedIv = ProtectedData.Unprotect(keyandIv.iv, null, DataProtectionScope.CurrentUser);
                }
                else
                {
                    return $"{Convert.ToBase64String(keyAndIv.key)} {Convert.ToBase64String(keyAndIv.iv)}";
                    //aesUnencryptedKey = keyandIv.key;
                    //aesUnencryptedIv = keyandIv.iv;
                }
            }
            catch (CryptographicException)
            {
                throw;
            }
        }
        #endregion
    }
}
