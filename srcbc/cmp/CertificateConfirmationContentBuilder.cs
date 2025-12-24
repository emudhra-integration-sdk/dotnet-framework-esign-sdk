using System;
using System.Collections;

using emCastle.Asn1;
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
    public class CertificateConfirmationContentBuilder
    {
        private static readonly DefaultSignatureAlgorithmIdentifierFinder sigAlgFinder = new DefaultSignatureAlgorithmIdentifierFinder();

        private readonly DefaultDigestAlgorithmIdentifierFinder digestAlgFinder;
        private readonly IList acceptedCerts = Platform.CreateArrayList();
        private readonly IList acceptedReqIds = Platform.CreateArrayList();

        public CertificateConfirmationContentBuilder()
            : this(new DefaultDigestAlgorithmIdentifierFinder())
        {
        }

        public CertificateConfirmationContentBuilder(DefaultDigestAlgorithmIdentifierFinder digestAlgFinder)
        {
            this.digestAlgFinder = digestAlgFinder;
        }

        public CertificateConfirmationContentBuilder AddAcceptedCertificate(X509Certificate certHolder,
            BigInteger certReqId)
        {
            acceptedCerts.Add(certHolder);
            acceptedReqIds.Add(certReqId);
            return this;
        }

        public CertificateConfirmationContent Build()
        {
            Asn1EncodableVector v = new Asn1EncodableVector();
            for (int i = 0; i != acceptedCerts.Count; i++)
            {
                X509Certificate cert = (X509Certificate)acceptedCerts[i];
                BigInteger reqId = (BigInteger)acceptedReqIds[i];


                AlgorithmIdentifier algorithmIdentifier = sigAlgFinder.Find(cert.SigAlgName);

                AlgorithmIdentifier digAlg = digestAlgFinder.find(algorithmIdentifier);
                if (null == digAlg)
                    throw new CmpException("cannot find algorithm for digest from signature");

                byte[] digest = DigestUtilities.CalculateDigest(digAlg.Algorithm, cert.GetEncoded());

                v.Add(new CertStatus(digest, reqId));
            }

            return new CertificateConfirmationContent(CertConfirmContent.GetInstance(new DerSequence(v)),
                digestAlgFinder);
        }
    }
}
