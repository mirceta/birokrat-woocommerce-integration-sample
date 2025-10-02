using System;
using System.Collections.Generic;
using System.Text;

namespace core.logic.common_exceptions
{
    public class Nothing : Exception
    {
        public Nothing(string message) : base(message) { }
    }
}
