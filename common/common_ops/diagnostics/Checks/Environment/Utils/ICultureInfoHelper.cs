using System.Globalization;

namespace common_ops.diagnostics.Checks.Environment.Utils
{
    public interface ICultureInfoHelper
    {
        CultureInfo GetCurrentCulture();
        DateTimeFormatInfo GetDateTimeFormat();
        CultureInfo BuildCultureInfoFromString(string info);
        string ParseCulture(string localeCode);
    }
}
