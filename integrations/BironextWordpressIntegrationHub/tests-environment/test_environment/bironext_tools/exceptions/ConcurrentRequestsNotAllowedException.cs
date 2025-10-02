using System;
using System.Runtime.Serialization;

namespace tests.tests.estrada {
    [Serializable]
    internal class ConcurrentRequestsNotAllowedException : Exception {
        public ConcurrentRequestsNotAllowedException() {
        }

        public ConcurrentRequestsNotAllowedException(string message) : base(message) {
        }

        public ConcurrentRequestsNotAllowedException(string message, Exception innerException) : base(message, innerException) {
        }

        protected ConcurrentRequestsNotAllowedException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}