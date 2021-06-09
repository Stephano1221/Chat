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
        int rsaKeySize = 2048;// Recommended: 2048. Minimum (Prior to Windows 8.1): 384. Minimum (Windows 8.1+): 512. Maximum (Microsoft Enhanced Cyprographic Provider): 16384. Maximum (Microsoft Base Cryptographic Provider): 512. Must be divisable by 8. https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsacryptoserviceprovider.keysize?view=net-5.0
        public string keyContainerName;
        private bool useKeyContainers = false;
        private bool isWindows = false;

        public RSAParameters rsaParametersPublicKey;
        public RSAParameters rsaParametersPrivateAndPublicKey;

        public byte[] aesEncryptedKey;
        public byte[] aesEncryptedIv;
        private byte[] aesUnencryptedKey;
        private byte[] aesUnencryptedIv;

        public Encryption()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                isWindows = true;
            }
        }

        #region RSA
        public byte[] RsaEncryptDecrypt(byte[] data, string keyContainerName, RSAParameters rsaParameters, bool oAEPPadding, bool encrypt)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null && useKeyContainers)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName,
                        Flags = CspProviderFlags.UseExistingKey
                        //cspParameters.Flags = CspProviderFlags.UseArchivableKey;
                    };

                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize, cspParameters))
                    {
                        if (encrypt && rsaParameters.Modulus != null && rsaParameters.Modulus.Count() > 0)
                        {
                            rsaCryptoServiceProvider.ImportParameters(rsaParameters);
                        }
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
                            return rsaCryptoServiceProvider.Encrypt(data, isWindows);
                        }
                        else
                        {
                            return rsaCryptoServiceProvider.Decrypt(data, isWindows);
                        }
                    }
                }
            }
            catch(CryptographicException e)
            {
                return null;
            }
        }

        public void RsaGenerateKey(string keyContainerName)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null && useKeyContainers)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName,
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null && useKeyContainers)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                        //cspParameters.Flags = CspProviderFlags.UseArchivableKey;
                    };

                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize, cspParameters))
                    {
                        rsaCryptoServiceProvider.FromXmlString(xmlKey);
                        if (rsaCryptoServiceProvider.PublicOnly)
                        {
                            rsaParametersPublicKey = rsaCryptoServiceProvider.ExportParameters(false);
                        }
                        else
                        {
                            rsaParametersPrivateAndPublicKey = rsaCryptoServiceProvider.ExportParameters(true);
                        }
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize))
                    {
                        rsaCryptoServiceProvider.FromXmlString(xmlKey);
                        if (rsaCryptoServiceProvider.PublicOnly)
                        {
                            rsaParametersPublicKey = rsaCryptoServiceProvider.ExportParameters(false);
                        }
                        else
                        {
                            rsaParametersPrivateAndPublicKey = rsaCryptoServiceProvider.ExportParameters(true);
                        }
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null && useKeyContainers)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName,
                        Flags = CspProviderFlags.UseExistingKey
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && keyContainerName != null && useKeyContainers)
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName,
                        Flags = CspProviderFlags.UseExistingKey
                    };

                    using (RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(rsaKeySize, cspParameters))
                    {
                        rsaParametersPublicKey = new RSAParameters();
                        rsaParametersPrivateAndPublicKey = new RSAParameters();
                        rsaCryptoServiceProvider.PersistKeyInCsp = false;
                        rsaCryptoServiceProvider.Clear();
                    }
                }
                else
                {
                    rsaParametersPublicKey = new RSAParameters();
                    rsaParametersPrivateAndPublicKey = new RSAParameters();
                }
            }
            catch(CryptographicException)
            {
                //Do nothing
            }
        }
        #endregion

        #region AES
        public byte[] AesEncryptDecrypt(byte[] data, (byte[] key, byte[] iv) keyandIv, bool encrypt)
        {
            try
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
            catch(CryptographicException)
            {
                throw;
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

        public void AesImportKeyOrIv(byte[] keyOrIv, bool isKey)
        {
            try
            {
                //string[] parts = xmlKey.Split(' ', 2);
                //TODO: Extract key parts from xmlKey for use below
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (isKey)
                    {
                        aesEncryptedKey = ProtectedData.Protect(keyOrIv, null, DataProtectionScope.CurrentUser);
                    }
                    else
                    {
                        aesEncryptedIv = ProtectedData.Protect(keyOrIv, null, DataProtectionScope.CurrentUser);
                    }
                }
                else
                {
                    if (isKey)
                    {
                        aesUnencryptedKey = keyOrIv;
                    }
                    else
                    {
                        aesUnencryptedIv = keyOrIv;
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public (byte[] aesDecryptedKey, byte[] aesDecryptedIv) AesExportKeyAndIv(byte[] encryptedKey, byte[] encryptedIv)
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
        #endregion
    }
}
