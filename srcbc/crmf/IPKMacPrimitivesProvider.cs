using System;

using emCastle.Asn1.X509;
using emCastle.Crypto;

namespace emCastle.Crmf
{
    public interface IPKMacPrimitivesProvider   
    {
	    IDigest CreateDigest(AlgorithmIdentifier digestAlg);

        IMac CreateMac(AlgorithmIdentifier macAlg);
    }
}
