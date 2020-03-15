using System;
using System.Collections.Generic;
using System.Text;

namespace Lastic.Core.Exceptions
{
    [Serializable]
    public class ConnectionException : Exception
    {
        public ConnectionException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ConnectionException(string uri, Exception innerException) : base(string.Format("Unable to connect to {0}", uri), innerException)
        {
        }

        public ConnectionException(string message) : base(message)
        {
        }

        protected ConnectionException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
