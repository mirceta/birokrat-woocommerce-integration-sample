using si.birokrat.next.common.registration;
using System;
using System.Collections.Generic;
using System.Text;

namespace si.birokrat.next.common.biro {
    public class BiroRegistry {

        static string key = Environment.Is64BitOperatingSystem? @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Andersen\Birokrat" : @"HKEY_LOCAL_MACHINE\SOFTWARE\Andersen\Birokrat";

        public static string Get(string value) {
            return RegistryUtils.GetRegistryValue(key, value);
        }

    }
}
