using System;
using System.Collections.Generic;
using System.Text;

namespace common_google.inbox_state {
    public interface IRegisteredMailSource {
        bool IsRegistered(string mail);
    }
}
