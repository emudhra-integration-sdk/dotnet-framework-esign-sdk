using System;

using emCastle.Utilities.IO;

namespace emCastle.Crypto.IO
{
    public class SignerSink
		: BaseOutputStream
	{
		private readonly ISigner mSigner;

        public SignerSink(ISigner signer)
		{
            this.mSigner = signer;
		}

        public virtual ISigner Signer
        {
            get { return mSigner; }
        }

		public override void WriteByte(byte b)
		{
            mSigner.Update(b);
		}

		public override void Write(byte[] buf, int off, int len)
		{
			if (len > 0)
			{
				mSigner.BlockUpdate(buf, off, len);
			}
		}
	}
}
