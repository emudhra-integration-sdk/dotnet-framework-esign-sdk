using System;

using emCastle.Asn1.Cmp;
using emCastle.Asn1.X509;
using emCastle.Cms;
using emCastle.Crypto.IO;
using emCastle.Math;
using emCastle.Security;
using emCastle.Utilities;
using emCastle.X509;

namespace emCastle.Cmp
{
    public class CertificateStatus
    {
        private static readonly DefaultSignatureAlgorithmIdentifierFinder sigAlgFinder = new DefaultSignatureAlgorithmIdentifierFinder();

        private readonly DefaultDigestAlgorithmIdentifierFinder digestAlgFinder;
        private readonly CertStatus certStatus;

        public CertificateStatus(DefaultDigestAlgorithmIdentifierFinder digestAlgFinder, CertStatus certStatus)
        {
            this.digestAlgFinder = digestAlgFinder;
            this.certStatus = certStatus;
        }

        public PkiStatusInfo PkiStatusInfo
        {
            get { return certStatus.StatusInfo; }
        }

        public BigInteger CertRequestId
        {
            get { return certStatus.CertReqID.Value; }
        }

        public bool IsVerified(X509Certificate cert)
        {
            AlgorithmIdentifier digAlg = digestAlgFinder.find(sigAlgFinder.Find(cert.SigAlgName));
            if (null == digAlg)
                throw new CmpException("cannot find algorithm for digest from signature " + cert.SigAlgName);

            byte[] digest = DigestUtilities.CalculateDigest(digAlg.Algorithm, cert.GetEncoded());

            return Arrays.ConstantTimeAreEqual(certStatus.CertHash.GetOctets(), digest);
        }
    }
}
