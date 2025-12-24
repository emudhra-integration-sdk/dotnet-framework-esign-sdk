using System;

namespace emCastle.Crypto.Tls
{
    public abstract class AbstractTlsCredentials
        :   TlsCredentials
    {
        public abstract Certificate Certificate { get; }
    }
}
