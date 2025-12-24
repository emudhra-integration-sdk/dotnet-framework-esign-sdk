using System;

using emCastle.Security;

namespace emCastle.Crypto.Tls
{
    internal class TlsClientContextImpl
        :   AbstractTlsContext, TlsClientContext
    {
        internal TlsClientContextImpl(SecureRandom secureRandom, SecurityParameters securityParameters)
            :   base(secureRandom, securityParameters)
        {
        }

        public override bool IsServer
        {
            get { return false; }
        }
    }
}
