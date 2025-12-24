using System;

using emCastle.Asn1.X509;
using emCastle.Crypto;
using emCastle.Security;

namespace emCastle.Crmf
{
    public class DefaultPKMacPrimitivesProvider
        : IPKMacPrimitivesProvider
    {
        public IDigest CreateDigest(AlgorithmIdentifier digestAlg)
        {
            return DigestUtilities.GetDigest(digestAlg.Algorithm);
        }

        public IMac CreateMac(AlgorithmIdentifier macAlg)
        {
            return MacUtilities.GetMac(macAlg.Algorithm);
        }
    }
}
