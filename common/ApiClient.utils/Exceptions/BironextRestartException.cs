using System;

namespace BirokratNext.Exceptions
{
    public class BironextRestartException : Exception
    {
        public BironextRestartException(string message) : base(message) { }
    }
}
