using common_ops.diagnostics.Checks.Environment.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Environment.Checks
{
    /// <summary>
    /// Checks if date and time are in correct format. Checks if system language for unicode programs is set to Slovenian. Wit repair option it will also fix settings
    /// in registry. If locale language is changed (WARNING postfix) system will need a reboot
    ///
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: date format, time format and system locale language for unicode programs</para>
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR, WARNING</para>
    /// </summary>

    public class Region_DateTimeLocale_CheckAndRepair : ICheck
    {
        private readonly string EXPECTED_DATE_FORMAT = "dd.MM.yyyy";
        private readonly string EXPECTED_TIME_FORMAT = "HH:mm:ss";
        private readonly string EXPECTED_LOCALE = "Slovenian";
        private readonly string EXPECTED_LOCALE_CODE = "0424";

        private readonly string LOCALE_REGISTRY_KEY = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Nls\Language";

        private readonly ICultureInfoHelper _cultureInfoHelper;
        private readonly IRegistryUtils _registryUtils;
        private bool _repair;
        private readonly Func<string, bool> _waitForConfirmation;

        /// <summary>
        /// <inheritdoc cref="Region_DateTimeLocale_CheckAndRepair"/>
        /// </summary>

        public Region_DateTimeLocale_CheckAndRepair(
            ICultureInfoHelper cultureInfoHelper,
            IRegistryUtils registryUtils,
            bool doRepair = false,
            Func<string, bool> waitForConfirmation = null)
        {
            _cultureInfoHelper = cultureInfoHelper;
            _registryUtils = registryUtils;
            _repair = doRepair;
            _waitForConfirmation = waitForConfirmation;
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                var results = new List<string>();

                results.AddRange(CheckAndFixLocaleName());
                results.AddRange(CheckAndFixDateAndTimeFormats());
                results.AddRange(CheckAndFixLocaleForUnicodePrograms());

                return new ResultRecord(!results.Any(x => x.EndsWith(TextConstants.POSTFIX_ERROR)), GetType().Name, results.ToArray());
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
            }
        }

        private List<string> CheckAndFixDateAndTimeFormats()
        {

            var results = new List<string>();
            var currentCulture = _cultureInfoHelper.GetCurrentCulture();
            var dateTimeFormat = _cultureInfoHelper.GetDateTimeFormat();

            string shortDatePattern = dateTimeFormat.ShortDatePattern;
            string longTimePattern = dateTimeFormat.LongTimePattern;

            if (shortDatePattern != EXPECTED_DATE_FORMAT)
            {
                if (_repair && FixDateFormat(EXPECTED_DATE_FORMAT))
                    results.Add("System date format repaired to " + EXPECTED_DATE_FORMAT + TextConstants.DELIMITER + "Previous Format: " + shortDatePattern + TextConstants.DELIMITER + TextConstants.POSTFIX_REPAIR);
                else
                    results.Add("Wrong System date format. Expected Format: " + EXPECTED_DATE_FORMAT + TextConstants.DELIMITER + "Current Format: " + shortDatePattern + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
            }
            else
            {
                results.Add("The system date format is correct: " + EXPECTED_DATE_FORMAT + "" + TextConstants.DELIMITER + TextConstants.POSTFIX_OK);
            }


            if (longTimePattern != EXPECTED_TIME_FORMAT)
            {
                if (_repair && FixTimeFormat(EXPECTED_TIME_FORMAT))
                    results.Add("System time format repaired to " + EXPECTED_TIME_FORMAT + TextConstants.DELIMITER + "Previous Format: " + longTimePattern + TextConstants.DELIMITER + TextConstants.POSTFIX_REPAIR);
                else
                    results.Add("Wrong System time format. Expected Format: " + EXPECTED_TIME_FORMAT + TextConstants.DELIMITER + "Current Format: " + longTimePattern + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
            }
            else
            {
                results.Add("The system time format is correct: " + EXPECTED_TIME_FORMAT + "'" + TextConstants.DELIMITER + TextConstants.POSTFIX_OK);
            }
            return results;
        }

        private bool FixDateFormat(string expectedFormat)
        {
            try
            {
                const string dateKey = @"HKEY_CURRENT_USER\Control Panel\International";
                const string dateValueName = "sShortDate";
                FixRegistryValue(dateKey, dateValueName, expectedFormat, "Change date format to: " + expectedFormat + "?");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to set date format: " + ex.Message);
                return false;
            }
        }

        private bool FixTimeFormat(string expectedFormat)
        {
            try
            {
                const string timeKey = @"HKEY_CURRENT_USER\Control Panel\International";
                const string timeValueName = "sTimeFormat";
                FixRegistryValue(timeKey, timeValueName, expectedFormat, "Change time format to: " + expectedFormat + "?");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to set time format: " + ex.Message);
                return false;
            }
        }

        private List<string> CheckAndFixLocaleName()
        {
            var results = new List<string>();

            const string registryKey = @"HKEY_CURRENT_USER\Control Panel\International";
            const string registryValueName_LocaleName = "LocaleName";
            const string registryValueName_Locale = "Locale";

            string currentLocaleName = _registryUtils.GetRegistryValue(registryKey, registryValueName_LocaleName);
            string currentLocale = _registryUtils.GetRegistryValue(registryKey, registryValueName_Locale);

            bool localeNameCorrect = currentLocaleName != null && currentLocaleName.Equals("sl-SI", StringComparison.OrdinalIgnoreCase);
            bool localeCorrect = currentLocale != null && currentLocale.EndsWith(EXPECTED_LOCALE_CODE, StringComparison.OrdinalIgnoreCase);

            if (!localeNameCorrect || !localeCorrect)
            {
                if (_repair)
                {
                    bool repaired = true;
                    try
                    {
                        FixLocaleNameAndLocale(registryKey, registryValueName_LocaleName, registryValueName_Locale);
                    }
                    catch (Exception ex)
                    {
                        results.Add("Failed to set Region LocaleName or Locale: " + ex.Message + TextConstants.DELIMITER + TextConstants.POSTFIX_REPAIR);
                        repaired = false;
                    }

                    if (repaired)
                    {
                        results.Add("Region LocaleName and Locale repaired to sl-SI / 0424" + TextConstants.DELIMITER + TextConstants.POSTFIX_REPAIR);
                    }
                    else
                    {
                        results.Add("Failed to repair Region LocaleName or Locale" + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
                    }
                }
                else
                {
                    results.Add("Wrong Region LocaleName or Locale. Expected: sl-SI / 0424"
                        + TextConstants.DELIMITER + "Current: " + currentLocaleName + " / " + currentLocale
                        + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
                }
            }
            else
            {
                results.Add("Region LocaleName and Locale are correct: sl-SI / 0424" + TextConstants.DELIMITER + TextConstants.POSTFIX_OK);
            }

            return results;
        }

        private void FixLocaleNameAndLocale(string registryKey, string valueNameLocaleName, string valueNameLocale)
        {
            if (!DoFixOption("Change Region LocaleName and Locale to Slovenian (sl-SI / 0424)?"))
                throw new Exception("Registry change cancelled!");

            _registryUtils.FixRegistryValue(registryKey, valueNameLocaleName, "sl-SI");
            _registryUtils.FixRegistryValue(registryKey, valueNameLocale, EXPECTED_LOCALE_CODE);
        }


        private List<string> CheckAndFixLocaleForUnicodePrograms()
        {
            var results = new List<string>();
            const string registryValueName = "Default";

            string systemLocale = _registryUtils.GetRegistryValue(LOCALE_REGISTRY_KEY, registryValueName);

            if (!string.IsNullOrEmpty(systemLocale))
            {
                var localeName = _cultureInfoHelper.ParseCulture(systemLocale);

                if (!systemLocale.StartsWith(EXPECTED_LOCALE_CODE, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (_repair && FixLocaleForUnicodePrograms(EXPECTED_LOCALE))
                    {
                        results.Add("System Locale repaired to " + EXPECTED_LOCALE + TextConstants.DELIMITER + "Previous Locale: " + systemLocale + TextConstants.DELIMITER + TextConstants.POSTFIX_REPAIR);
                        results.Add("System needs a restart for changes to take effect" + TextConstants.DELIMITER + TextConstants.POSTFIX_WARNING);
                    }
                    else
                    {
                        results.Add("Wrong System Locale. Expected: " + EXPECTED_LOCALE + TextConstants.DELIMITER + "Current Locale: " + systemLocale + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
                    }
                }
                else
                {
                    results.Add("System Locale (for non-Unicode programs): " + localeName + TextConstants.DELIMITER + TextConstants.POSTFIX_OK);
                }
            }
            else
            {
                results.Add("System Locale could not be determined" + TextConstants.DELIMITER + TextConstants.POSTFIX_ERROR);
            }
            return results;
        }

        private bool FixLocaleForUnicodePrograms(string expectedLocale)
        {
            try
            {
                const string registryKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Nls\Language";
                const string registryValueName = "Default";

                // Convert locale name to LCID (Locale ID)
                var culture = _cultureInfoHelper.BuildCultureInfoFromString(expectedLocale);

                string lcid = culture.LCID.ToString("X4"); // Convert LCID to hexadecimal string

                FixRegistryValue(registryKey, registryValueName, lcid, "Change system locale to: " + expectedLocale + "?");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to set system locale: " + ex.Message);
                return false;
            }
        }

        private void FixRegistryValue(string registryKey, string registryValueName, string value, string message)
        {
            if (!DoFixOption(message))
                throw new Exception("Registry change cancelled!");
            _registryUtils.FixRegistryValue(registryKey, registryValueName, value);
        }

        bool DoFixOption(string message)
        {
            if (_waitForConfirmation != null)
            {
                if (_waitForConfirmation.Invoke(message))
                    return true;
            }
            return false;
        }
    }
}
