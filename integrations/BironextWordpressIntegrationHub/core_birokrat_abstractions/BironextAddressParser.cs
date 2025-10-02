using System;
using System.Collections.Generic;
using System.Text;

namespace core.logic.common_birokrat {
    public class BironextAddressParser {

        public static string CorrectAddress(string bironextAddress) {
            if (bironextAddress[bironextAddress.Length - 1] == '/') {
                bironextAddress = bironextAddress.Substring(0, bironextAddress.Length - 1);
            }
            if (bironextAddress.Substring(bironextAddress.Length - 4) == "/api") {
                bironextAddress = bironextAddress.Substring(0, bironextAddress.Length - 4);
            }
            bironextAddress = bironextAddress + "/api/";
            return bironextAddress;
        }
    }
}
