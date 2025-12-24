using System;

namespace emCastle.Cmp
{
    public class CmpException
        : Exception
    {
        public CmpException()
        {
        }

        public CmpException(string message)
            : base(message)
        {
        }

        public CmpException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
