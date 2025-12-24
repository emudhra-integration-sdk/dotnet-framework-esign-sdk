using System;
using System.IO;

namespace emCastle.Crypto.Tls
{
    public interface TlsSignerCredentials
        :   TlsCredentials
    {
        /// <exception cref="IOException"></exception>
        byte[] GenerateCertificateSignature(byte[] hash);

        SignatureAndHashAlgorithm SignatureAndHashAlgorithm { get; }
    }
}
