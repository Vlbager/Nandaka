using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class ConfigurationException : NandakaBaseException
    {
        public ConfigurationException()
        {
        }

        public ConfigurationException(string message) : base(message)
        {
        }

        public ConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}