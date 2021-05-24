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
        private string keyContainerName;
        private RSAParameters nonWindowsRsaPublicKey;
        private RSAParameters nonWindowsRsaPrivatePublicKey;
        private byte[] aesEncryptedKey;
        private byte[] aesEncryptedIv;
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
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(cspParameters))
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
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider())
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
                    using RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(cspParameters);
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider())
                    {
                        nonWindowsRsaPublicKey = rsaCryptoServiceProvider.ExportParameters(false);
                        nonWindowsRsaPrivatePublicKey = rsaCryptoServiceProvider.ExportParameters(true);
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
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(cspParameters))
                    {
                        rsaCryptoServiceProvider.FromXmlString(xmlKey);
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider())
                    {
                        rsaCryptoServiceProvider.FromXmlString(xmlKey);
                        nonWindowsRsaPublicKey = rsaCryptoServiceProvider.ExportParameters(false);
                        nonWindowsRsaPrivatePublicKey = rsaCryptoServiceProvider.ExportParameters(true);
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
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(cspParameters))
                    {
                        return rsaCryptoServiceProvider.ToXmlString(includePrivateParameters);
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider())
                    {
                        if (includePrivateParameters)
                        {
                            rsaCryptoServiceProvider.ImportParameters(nonWindowsRsaPrivatePublicKey);
                        }
                        else
                        {
                            rsaCryptoServiceProvider.ImportParameters(nonWindowsRsaPublicKey);
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
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(cspParameters))
                    {
                        rsaCryptoServiceProvider.PersistKeyInCsp = false;
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider())
                    {
                        nonWindowsRsaPublicKey = new RSAParameters();
                        nonWindowsRsaPrivatePublicKey = new RSAParameters();
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
        public void AesEncryptDecrypt(byte[] data, (byte[] key, byte[] iv) keyandIv, bool encrypt)
        {
            using (Aes aes = Aes.Create())
            {

            }
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
            catch(CryptographicException)
            {
                throw;
            }
        }
        #endregion
    }
}
