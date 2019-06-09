namespace Scaffold.Application.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public abstract class NotFoundException : ApplicationException
    {
        protected NotFoundException(string message)
            : base(message)
        {
        }

        protected NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected NotFoundException(string title, string message)
            : base(title, message)
        {
        }

        protected NotFoundException(string title, string message, Exception innerException)
            : base(title, message, innerException)
        {
        }

        protected NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
