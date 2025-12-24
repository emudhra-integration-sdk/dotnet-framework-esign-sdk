using System;
using System.IO;

namespace emCastle.Crypto.Tls
{
    public class TlsException
        : IOException
    {
        public TlsException(string message, Exception cause)
            : base(message, cause)
        {
        }
    }
}
