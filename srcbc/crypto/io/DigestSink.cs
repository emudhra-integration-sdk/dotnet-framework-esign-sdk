using System;

using emCastle.Utilities.IO;

namespace emCastle.Crypto.IO
{
    public class DigestSink
        : BaseOutputStream
    {
        private readonly IDigest mDigest;

        public DigestSink(IDigest digest)
        {
            this.mDigest = digest;
        }

        public virtual IDigest Digest
        {
            get { return mDigest; }
        }

        public override void WriteByte(byte b)
        {
            mDigest.Update(b);
        }

        public override void Write(byte[] buf, int off, int len)
        {
            if (len > 0)
            {
                mDigest.BlockUpdate(buf, off, len);
            }
        }
    }
}
