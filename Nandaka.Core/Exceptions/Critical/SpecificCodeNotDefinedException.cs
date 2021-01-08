using System;
using System.Runtime.Serialization;

namespace Nandaka.Core.Exceptions
{
    [Serializable]
    public class SpecificCodeNotDefinedException : NandakaBaseException
    {
        public int SpecificCode { get; }
        
        public SpecificCodeNotDefinedException(int specificCode)
        {
            SpecificCode = specificCode;
        }
        
        public SpecificCodeNotDefinedException(string message, int specificCode) : base(message)
        {
            SpecificCode = specificCode;
        }

        public SpecificCodeNotDefinedException(string message, Exception innerException, int specificCode) : base(message, innerException)
        {
            SpecificCode = specificCode;
        }

        protected SpecificCodeNotDefinedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}