using System;
using System.Collections.Generic;
using System.Text;

namespace si.birokrat.next.common.exceptions {
    public class TravellingException {
        // LOGIN
        public static string LOGIN_ERROR = "LOGINERROR";
        public static string LOGIN_USER_DOESNT_EXIST = $"Uporabnik ne obstaja!";
        public static string LOGIN_WRONG_PASSWORD = $"Nepravilno geslo !";
        public static string LOGIN_UNKNOWN_ERROR = $"Neznana napaka";
        public static string LOGIN_INVITE_FAILED = $"Neznana napaka";

        public static bool IS_LOGIN_ERROR(string errstring) {
            return errstring.Contains(LOGIN_ERROR);
        }

        public static string GET_LOGIN_ERROR(string error) {
            return $"{LOGIN_ERROR}:{error}";
        }

        public static string GET_CONCRETE_ERROR(string error) {
            return error.Split(':')[1];
        }
    }
}
