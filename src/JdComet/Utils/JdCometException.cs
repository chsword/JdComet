using System;

namespace JdComet
{
    public class JdCometException : Exception
    {
        public JdCometException()
            : base()
        {
        }

        public JdCometException(string message, Exception cause)
            : base(message, cause)
        {
        }

        public JdCometException(string message)
            : base(message)
        {
        }

    }
}
