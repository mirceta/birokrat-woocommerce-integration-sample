using System;

namespace BirokratNext.Exceptions
{
    public class ConcurrentRequestsNotAllowedException : Exception
    {
        public ConcurrentRequestsNotAllowedException() : base("") { }
    }
}
