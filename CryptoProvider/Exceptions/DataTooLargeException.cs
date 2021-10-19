namespace EdgeSecrets.CryptoProvider.Exceptions
{
    using System;

    public class DataTooLargeException : Exception
    {
        public DataTooLargeException()
        {
        }

        public DataTooLargeException(string message)
            : base(message)
        {
        }

        public DataTooLargeException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
