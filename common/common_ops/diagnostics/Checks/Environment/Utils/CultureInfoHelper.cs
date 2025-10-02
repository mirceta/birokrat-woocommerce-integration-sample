using System.Globalization;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public class CultureInfoHelper : ICultureInfoHelper
    {
        public CultureInfo GetCurrentCulture()
        {
            return CultureInfo.CurrentCulture;
        }

        public DateTimeFormatInfo GetDateTimeFormat()
        {
            return CultureInfo.CurrentCulture.DateTimeFormat;
        }

        public CultureInfo BuildCultureInfoFromString(string info)
        {
            return new CultureInfo(info);
        }

        public string ParseCulture(string localeCode)
        {
            if (int.TryParse(localeCode, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int localeInt))
            {
                return new CultureInfo(localeInt).DisplayName;
            }
            else
            {
                return "Unknown Locale";
            }
        }
    }
}
