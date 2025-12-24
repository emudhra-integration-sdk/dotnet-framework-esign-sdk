using System;

using emCastle.Utilities.IO;

namespace emCastle.Crypto.IO
{
    public class MacSink
        : BaseOutputStream
    {
        private readonly IMac mMac;

        public MacSink(IMac mac)
        {
            this.mMac = mac;
        }

        public virtual IMac Mac
        {
            get { return mMac; }
        }

        public override void WriteByte(byte b)
        {
            mMac.Update(b);
        }

        public override void Write(byte[] buf, int off, int len)
        {
            if (len > 0)
            {
                mMac.BlockUpdate(buf, off, len);
            }
        }
    }
}
