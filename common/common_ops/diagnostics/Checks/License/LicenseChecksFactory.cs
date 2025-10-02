using common_ops.diagnostics.Checks.License.Checks;

namespace common_ops.diagnostics.Checks.License
{
    public class LicenseChecksFactory
    {
        /// <summary>
        /// Checks license fields and dates if they are valid. In case of valid license it will return a
        /// single line in <see cref="ResultRecord.AdditionalInfo"/>: License OK
        ///
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: errors in format: description||OPERATION||Postfix.
        /// Separated with <c>||</c> </para>
        /// <para>Example for invalid license: 2023-01-30   Veljavnost license||DATETIME||ERROR</para>
        /// <para>Example for required field not found: MSC||REQUIRED||ERROR</para>
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR</para>
        /// </summary>
        public Birokrat_License_Check Build_Birokrat_License_Check(string[] licenseLines)
        {
            return new Birokrat_License_Check(licenseLines);
        }
    }
}
