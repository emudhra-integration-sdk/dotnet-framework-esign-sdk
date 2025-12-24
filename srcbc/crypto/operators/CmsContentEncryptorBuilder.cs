using System;
using System.Collections;
using System.IO;

using emCastle.Asn1;
using emCastle.Asn1.Nist;
using emCastle.Asn1.Ntt;
using emCastle.Asn1.X509;
using emCastle.Cms;
using emCastle.Crypto.IO;
using emCastle.Crypto.Parameters;
using emCastle.Security;
using emCastle.Utilities;
using emCastle.Crypto;
using emCastle.Crypto.Operators;

namespace emCastle.Operators
{
    public class CmsContentEncryptorBuilder
    {
        private static readonly IDictionary KeySizes = Platform.CreateHashtable();

        static CmsContentEncryptorBuilder()
        {
            KeySizes[NistObjectIdentifiers.IdAes128Cbc] = 128;
            KeySizes[NistObjectIdentifiers.IdAes192Cbc] = 192;
            KeySizes[NistObjectIdentifiers.IdAes256Cbc] = 256;

            KeySizes[NttObjectIdentifiers.IdCamellia128Cbc] = 128;
            KeySizes[NttObjectIdentifiers.IdCamellia192Cbc] = 192;
            KeySizes[NttObjectIdentifiers.IdCamellia256Cbc] = 256;
        }

        private static int GetKeySize(DerObjectIdentifier oid)
        {
            if (KeySizes.Contains(oid))
            {
                return (int)KeySizes[oid];
            }

            return -1;
        }

        private readonly DerObjectIdentifier encryptionOID;
        private readonly int keySize;

        private readonly EnvelopedDataHelper helper = new EnvelopedDataHelper();
        //private SecureRandom random;

        public CmsContentEncryptorBuilder(DerObjectIdentifier encryptionOID)
            : this(encryptionOID, GetKeySize(encryptionOID))
        {
        }

        public CmsContentEncryptorBuilder(DerObjectIdentifier encryptionOID, int keySize)
        {
            this.encryptionOID = encryptionOID;
            this.keySize = keySize;
        }

        public ICipherBuilderWithKey Build()
        {
            //return new Asn1CipherBuilderWithKey(encryptionOID, keySize, random);
            return new Asn1CipherBuilderWithKey(encryptionOID, keySize, null);
        }
    }
}
