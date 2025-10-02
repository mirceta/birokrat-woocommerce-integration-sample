using System;
using System.Collections.Generic;
using System.Text;

namespace si.birokrat.next.common {
    class MyGuid {
        public static string Get() {
            return Guid.NewGuid().ToString().Substring(0, 15);
        }
    }
}
