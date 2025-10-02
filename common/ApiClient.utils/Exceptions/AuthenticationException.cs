using System;

namespace BirokratNext.Exceptions
{
    public class AuthenticationException : ApiException
    {
        public AuthenticationException(string message) : base(message) { }
    }
}
