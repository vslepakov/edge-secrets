using System;

namespace KeyManagement.Exceptions
{
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
