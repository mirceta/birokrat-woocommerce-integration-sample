using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiroWoocommerceHub.flows
{
    public class Tools
    {
        public static bool IsEUWooCountry(string country_code) {
            string[] codes = EUCountryCodes();
            return codes.Contains(country_code);
        }

        public static string[] EUCountryCodes() {
            string[] codes = { "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "EL",
            "ES", "FI", "FR", "GR", "HR", "HU", "IE", "IT", "LT", "LU", "LV",
            "MT", "NL", "PL", "PT", "RO", "SE", "SI", "SK"};
            return codes;
        }

        public static double ParseDoubleBigBrainTime(string number) {
            int cnt = number.Where(x => x == ',' || x == '.').ToList().Count;

            if (string.IsNullOrEmpty(number)) {
                return 0;
            } else if (cnt == 0) {
                return double.Parse(number);
            } else if (cnt == 1) {
                string some = number.Replace(".", ",");
                CultureInfo culture = new CultureInfo("de"); // de culture means '.' is thousands sep, ',' is decimal sep
                return double.Parse(some, culture);
            } else {
                char decimalsep = number.Where(x => x == ',' || x == '.').Last();

                if (decimalsep == ',') {
                    number = number.Replace(".", "");
                } else {
                    number = number.Replace(",", "");
                }
                number = number.Replace(".", ",");
                CultureInfo culture = new CultureInfo("de"); // de culture means '.' is thousands sep, ',' is decimal sep
                return double.Parse(number, culture);
            }
        }

        public static string SerializeDoubleToBirokratFormat(double number) {
            string some = number.ToString("0.00000000", System.Globalization.CultureInfo.InvariantCulture);
            return some.Replace(",", ".");
        }

        public static string SerializeDoubleToWooType(double number) {
            return null;
        }

        public static string GetHashCode(string theString)
        {
            string hash;
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                hash = BitConverter.ToString(
                  md5.ComputeHash(Encoding.UTF8.GetBytes(theString))
                ).Replace("-", String.Empty);
            }
            return hash.Substring(0, 14);
        }


        public static int RetardedDynamicLength(dynamic dyn)
        {
            int cnt = 0;
            foreach (var package in dyn)
            {
                cnt++;
            }
            return cnt;
        }

        public static async Task SavePdf(string pdf, string name, string folder = "")
        {
            var bytes = Convert.FromBase64String(pdf);
            int b = pdf.Length;
            string path = $"{name}.pdf";
            if (!string.IsNullOrEmpty(folder)) {
                path = Path.Combine(folder, path);
            }
            var file = new FileStream(path, FileMode.OpenOrCreate);
            var srm = new MemoryStream(bytes);
            await srm.CopyToAsync(file);
            srm.Close();
            file.Close();
        }
    }
}
