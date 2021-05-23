using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    public class Encryption
    {
        RSAParameters nonWindowsRSAPublicKey;
        RSAParameters nonWindowsRSAPrivatePublicKey;

        public Encryption()
        {

        }

        public byte[] RSAEncryptDecrypt(byte[] data, RSAParameters rSAParameters, bool oAEPPadding, bool encrypt)
        {
            try
            {
                using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
                {
                    rSACryptoServiceProvider.ImportParameters(rSAParameters);
                    byte[] changedData;
                    if (encrypt)
                    {
                        changedData = rSACryptoServiceProvider.Encrypt(data, oAEPPadding);
                    }
                    else
                    {
                        changedData = rSACryptoServiceProvider.Decrypt(data, oAEPPadding);
                    }
                    return changedData;
                }
            }
            catch(CryptographicException)
            {
                return null;
            }
        }

        public void RSAGenerateKey(string keyContainerName)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(cspParameters);
                }
                else
                {
                    using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
                    {
                        nonWindowsRSAPublicKey = rSACryptoServiceProvider.ExportParameters(false);
                        nonWindowsRSAPrivatePublicKey = rSACryptoServiceProvider.ExportParameters(true);
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public void RSAXmlToKey(string keyContainerName, string xmlKey)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(cspParameters))
                    {
                        rSACryptoServiceProvider.FromXmlString(xmlKey);
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
                    {
                        rSACryptoServiceProvider.FromXmlString(xmlKey);
                        nonWindowsRSAPublicKey = rSACryptoServiceProvider.ExportParameters(false);
                        nonWindowsRSAPrivatePublicKey = rSACryptoServiceProvider.ExportParameters(true);
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public string RSAGetKeyAsXml(string keyContainerName, bool includePrivateParameters)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(cspParameters))
                    {
                        return rSACryptoServiceProvider.ToXmlString(includePrivateParameters);
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
                    {
                        if (includePrivateParameters)
                        {
                            rSACryptoServiceProvider.ImportParameters(nonWindowsRSAPrivatePublicKey);
                        }
                        else
                        {
                            rSACryptoServiceProvider.ImportParameters(nonWindowsRSAPublicKey);
                        }
                        return rSACryptoServiceProvider.ToXmlString(includePrivateParameters);
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }

        public void RSADeleteKey(string keyContainerName)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    CspParameters cspParameters = new CspParameters()
                    {
                        KeyContainerName = keyContainerName
                    };
                    using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider(cspParameters))
                    {
                        rSACryptoServiceProvider.PersistKeyInCsp = false;
                    }
                }
                else
                {
                    using (RSACryptoServiceProvider rSACryptoServiceProvider = new RSACryptoServiceProvider())
                    {
                        nonWindowsRSAPublicKey = new RSAParameters();
                        nonWindowsRSAPrivatePublicKey = new RSAParameters();
                    }
                }
            }
            catch(CryptographicException)
            {
                throw;
            }
        }
    }
}
