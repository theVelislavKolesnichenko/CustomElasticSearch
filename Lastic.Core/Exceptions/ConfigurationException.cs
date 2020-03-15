using System;
using System.Collections.Generic;
using System.Text;

namespace Lastic.Core.Exceptions
{
    [Serializable]
    public class ConfigurationException : Exception
    {
        public ConfigurationException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigurationException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
