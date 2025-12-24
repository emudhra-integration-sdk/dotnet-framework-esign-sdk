using System;

namespace emCastle.Crypto.Tls
{
    public interface TlsPskIdentityManager
    {
        byte[] GetHint();

        byte[] GetPsk(byte[] identity);
    }
}
