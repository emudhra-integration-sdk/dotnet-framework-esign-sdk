using System;

namespace emCastle.Crypto.Tls
{
    public interface TlsSession
    {
        SessionParameters ExportSessionParameters();

        byte[] SessionID { get; }

        void Invalidate();

        bool IsResumable { get; }
    }
}
