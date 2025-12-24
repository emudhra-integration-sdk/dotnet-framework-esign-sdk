using System;
using System.IO;

using emCastle.Crypto.IO;

namespace emCastle.Crypto.Operators
{
    public class DefaultSignatureCalculator
        : IStreamCalculator
    {
        private readonly SignerSink mSignerSink;

        public DefaultSignatureCalculator(ISigner signer)
        {
            this.mSignerSink = new SignerSink(signer);
        }

        public Stream Stream
        {
            get { return mSignerSink; }
        }

        public object GetResult()
        {
            return new DefaultSignatureResult(mSignerSink.Signer);
        }
    }
}
