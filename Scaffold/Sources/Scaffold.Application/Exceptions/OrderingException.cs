namespace Scaffold.Application.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public abstract class OrderingException : ApplicationException
    {
        protected OrderingException(string message)
            : base(message)
        {
        }

        protected OrderingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected OrderingException(string title, string message)
            : base(title, message)
        {
        }

        protected OrderingException(string title, string message, Exception innerException)
            : base(title, message, innerException)
        {
        }

        protected OrderingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
