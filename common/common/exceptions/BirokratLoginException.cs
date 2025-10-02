using System;
using System.Collections.Generic;
using System.Text;

namespace common.exceptions {
    public class BirokratLoginException : Exception {
        public BirokratLoginException(string message) : base(message) {
        }
    }
}
