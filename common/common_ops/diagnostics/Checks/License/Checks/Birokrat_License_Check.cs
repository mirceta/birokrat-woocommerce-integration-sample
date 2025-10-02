using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.License.Checks
{
    /// <summary>
    /// Checks license fields and dates if they are valid. In case of valid license it will return a
    /// single line in <see cref="ResultRecord.AdditionalInfo"/>: License OK
    ///
    /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: errors in format: description||OPERATION||Postfix.
    /// Separated with <c>||</c> </para>
    /// <para>Example for invalid license: 2023-01-30   Veljavnost licence||DATETIME||ERROR</para>
    /// <para>Example for required field not found: MSC||REQUIRED||ERROR</para>
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR</para>
    /// </summary>
    public class Birokrat_License_Check : ICheck
    {
        private readonly string[] _license;
        private readonly string[] REQUIRED = new string[] { "IME", "BIR", "VLC", "VND", "USN", "MSD" };

        /// <summary>
        /// <inheritdoc cref="Birokrat_License_Check"/>
        /// </summary>
        public Birokrat_License_Check(string[] licenseLines)
        {
            _license = licenseLines.Select(x => x.Trim()).ToArray();
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                if (!_license.Any())
                    throw new Exception("License has no lines!");

                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private async Task<ResultRecord> Work()
        {
            var info = AreRequiredPresent();

            foreach (var line in _license)
            {
                if (CanExtractDate(line))
                {
                    var date = ExtractDateFromLicense(line);
                    if (date == null)
                    {
                        info.Add(ExtractDescription(line) + "||DATETIME||" + TextConstants.POSTFIX_ERROR);
                        continue;
                    }
                    if (date.Value.Date < DateTime.Now)
                        info.Add(ExtractDescription(line) + "||DATETIME||" + TextConstants.POSTFIX_ERROR);
                }
                else if (line.StartsWith("USN", StringComparison.CurrentCultureIgnoreCase))
                {
                    var amount = new string(line.Where(x => char.IsDigit(x)).ToArray());
                    if (amount.Length == 0)
                    {
                        info.Add(ExtractDescription(line) + "||USERS||" + TextConstants.POSTFIX_ERROR);
                        continue;
                    }
                    if (int.TryParse(amount, out var result))
                    {
                        if (result == 0)
                            info.Add(ExtractDescription(line) + "||USERS||" + TextConstants.POSTFIX_ERROR);
                    }
                }
            }

            if (info.Count() > 0)
                return new ResultRecord(info.Count() == 0, GetType().Name, info.ToArray());
            return new ResultRecord(info.Count() == 0, GetType().Name, "License " + TextConstants.POSTFIX_OK);
        }

        private List<string> AreRequiredPresent()
        {
            var result = new List<string>();
            foreach (var req in REQUIRED)
            {
                var found = false;
                foreach (var line in _license)
                {
                    if (line.StartsWith(req, StringComparison.CurrentCultureIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    result.Add(req + "||REQUIRED||" + TextConstants.POSTFIX_ERROR);
            }
            return result;
        }

        private bool CanExtractDate(string line)
        {
            if (line.StartsWith("VLC", StringComparison.CurrentCultureIgnoreCase))
                return true;
            if (line.StartsWith("VND", StringComparison.CurrentCultureIgnoreCase))
                return true;
            return false;
        }

        private DateTime? ExtractDateFromLicense(string line)
        {
            // Regex pattern to match yyyy-MM-dd format
            string pattern = @"\b\d{4}-\d{2}-\d{2}\b";
            Match match = Regex.Match(line, pattern);
            try
            {
                return DateTime.Parse(match.Value);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string ExtractDescription(string line)
        {
            return new string(line.SkipWhile(x => x != ' ').SkipWhile(x => !char.IsLetterOrDigit(x)).ToArray()).Replace("  ", " ");
        }
    }
}
