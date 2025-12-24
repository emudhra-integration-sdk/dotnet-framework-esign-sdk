using System;

using emCastle.Asn1;
using emCastle.Asn1.Pkcs;
using emCastle.Asn1.X509;
using emCastle.Crypto;
using emCastle.Security;

namespace emCastle.Pkcs
{
    public sealed class EncryptedPrivateKeyInfoFactory
    {
        private EncryptedPrivateKeyInfoFactory()
        {
        }

        public static EncryptedPrivateKeyInfo CreateEncryptedPrivateKeyInfo(
            DerObjectIdentifier		algorithm,
            char[]					passPhrase,
            byte[]					salt,
            int						iterationCount,
            AsymmetricKeyParameter	key)
        {
            return CreateEncryptedPrivateKeyInfo(
                algorithm.Id, passPhrase, salt, iterationCount,
                PrivateKeyInfoFactory.CreatePrivateKeyInfo(key));
        }

        public static EncryptedPrivateKeyInfo CreateEncryptedPrivateKeyInfo(
            string					algorithm,
            char[]					passPhrase,
            byte[]					salt,
            int						iterationCount,
            AsymmetricKeyParameter	key)
        {
            return CreateEncryptedPrivateKeyInfo(
                algorithm, passPhrase, salt, iterationCount,
                PrivateKeyInfoFactory.CreatePrivateKeyInfo(key));
        }

        public static EncryptedPrivateKeyInfo CreateEncryptedPrivateKeyInfo(
            string			algorithm,
            char[]			passPhrase,
            byte[]			salt,
            int				iterationCount,
            PrivateKeyInfo	keyInfo)
        {
            IBufferedCipher cipher = PbeUtilities.CreateEngine(algorithm) as IBufferedCipher;
            if (cipher == null)
                throw new Exception("Unknown encryption algorithm: " + algorithm);

            Asn1Encodable pbeParameters = PbeUtilities.GenerateAlgorithmParameters(
                algorithm, salt, iterationCount);
            ICipherParameters cipherParameters = PbeUtilities.GenerateCipherParameters(
                algorithm, passPhrase, pbeParameters);
            cipher.Init(true, cipherParameters);
            byte[] encoding = cipher.DoFinal(keyInfo.GetEncoded());

            DerObjectIdentifier oid = PbeUtilities.GetObjectIdentifier(algorithm);
            AlgorithmIdentifier algID = new AlgorithmIdentifier(oid, pbeParameters);
            return new EncryptedPrivateKeyInfo(algID, encoding);
        }
    }
}
