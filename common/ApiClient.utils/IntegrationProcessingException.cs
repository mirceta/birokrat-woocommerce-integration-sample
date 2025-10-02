using System;
using System.Runtime.Serialization;

namespace ApiClient.utils
{
    [Serializable]
    internal class IntegrationProcessingException : Exception
    {
        public IntegrationProcessingException()
        {
        }

        public IntegrationProcessingException(string message) : base(message)
        {
        }

        public IntegrationProcessingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected IntegrationProcessingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}