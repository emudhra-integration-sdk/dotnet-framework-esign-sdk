using System;
using System.Collections;
using System.IO;
using System.Text;

using emCastle.Asn1;
using emCastle.Asn1.X509;
using emCastle.Crypto.Agreement;
using emCastle.Crypto.Agreement.Srp;
using emCastle.Crypto.Digests;
using emCastle.Crypto.Encodings;
using emCastle.Crypto.Engines;
using emCastle.Crypto.Generators;
using emCastle.Crypto.IO;
using emCastle.Crypto.Parameters;
using emCastle.Crypto.Prng;
using emCastle.Math;
using emCastle.Security;
using emCastle.Utilities;
using emCastle.Utilities.Date;

namespace emCastle.Crypto.Tls
{
    [Obsolete("Use 'TlsClientProtocol' instead")]
    public class TlsProtocolHandler
        :   TlsClientProtocol
    {
        public TlsProtocolHandler(Stream stream, SecureRandom secureRandom)
            :   base(stream, stream, secureRandom)
        {
        }

        /// <remarks>Both streams can be the same object</remarks>
        public TlsProtocolHandler(Stream input, Stream output, SecureRandom	secureRandom)
            :   base(input, output, secureRandom)
        {
        }
    }
}
