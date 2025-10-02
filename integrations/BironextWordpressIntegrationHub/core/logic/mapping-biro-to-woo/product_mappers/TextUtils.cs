using System.Text;

namespace BiroWoocommerceHubTests.tools
{
    public static class TextUtils
    {
        public static string ApplyTextCorrections(string text) =>
            text
            .Replace("{{S}}", "Š")
            .Replace("{{s}}", "š")
            .Replace("{{C}}", "Č")
            .Replace("{{c}}", "č")
            .Replace("{{Z}}", "Ž")
            .Replace("{{z}}", "ž")
            .Replace("\n", " ");

        public static char toSumnik(char a) {
            a = a.ToString().ToLower()[0];
            if (a == 'z') {
                return 'ž';
            } else if (a == 's') {
                return 'š';
            } else if (a == 'c') {
                return 'č';
            }
            return a;
        }

        public static char toSicnik(char a) {
            a = a.ToString().ToLower()[0];
            if (a == 'ž') {
                return 'z';
            } else if (a == 'š') {
                return 's';
            } else if (a == 'č') {
                return 'c';
            }
            return a;
        }

        public static string RemoveSumniks(string word) {
            StringBuilder bild = new StringBuilder("");
            for (int i = 0; i < word.Length; i++) {
                bild.Append(toSicnik(word[i]));
            }
            return bild.ToString();
        }
    }
}