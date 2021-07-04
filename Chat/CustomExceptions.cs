using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat
{
    class CustomExceptions
    {
    }

    public class CertificateNotFoundException : Exception
    {
        public CertificateNotFoundException()
        {

        }

        public CertificateNotFoundException(string message) : base(message)
        {

        }

        public CertificateNotFoundException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
