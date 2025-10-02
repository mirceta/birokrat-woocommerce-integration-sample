using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Environment.Checks;
using common_ops.diagnostics.Checks.Environment.Utils;
using common_ops.diagnostics.Checks.Location.Utils;
using System;

namespace common_ops.diagnostics.Checks.Environment
{
    public class EnvironmentChecksFactory
    {
        /// <summary>
        /// Will check if ini settings for 00000000 are filled correctly. Checks fields SQL, NEXT, Msgbox and RTC. If repair option is set
        /// it will fix or add fields with REPAIR postfix.
        /// Will not check customers inis since they will be created/updated automatically by birokrat
        /// 
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: All checked .ini field, value of the fields and postfixes</para>
        /// 
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR, REPAIR</para>
        /// </summary>
        public ICheck Build_Birokrat_IniSettings_CheckAndRepair(string sqlServerName, string location = "", bool doRepair = false)
        {
            var fileSystem = new FileSystem();
            return new Birokrat_IniSettings_CheckAndRepair(
                new IniSettingsHelper(fileSystem),
                new ReadonlySetterVoid(fileSystem),
                fileSystem,
                sqlServerName,
                location,
                doRepair);
        }

        /// <summary>
        /// Checks and optionally repairs registry values related to the Birokrat application.
        /// 
        /// <para>The class verifies registry values against expected values stored in a dictionary. If the `doRepair` flag is enabled, 
        /// incorrect registry values will be fixed automatically. Repair will also add record to registry if it is missing</para>
        ///
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: contains result for each registry record with postfix</para>
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR, REPAIR</para>
        /// </summary>
        public Birokrat_Registry_CheckAndRepair Build_Birokrat_Registry_CheckAndRepair(
            string sqlServerName,
            string biroExeLoc = "",
            bool doRepair = false,
            Func<string, bool> waitForConfirmation = null)
        {
            return new Birokrat_Registry_CheckAndRepair(
                new RegistrySettingsHelper(),
                new RegistryUtilsAccess(),
                sqlServerName,
                biroExeLoc,
                doRepair,
                waitForConfirmation);
        }

        /// <summary>
        /// Checks and compares if same values are in Birokrat.ini and registry. Will only return fields with the same name and corresponding value.
        /// if both values are the same output will be OK. Otherwise it will show and ERROR with corresponding value and value origin (registry and ini)
        ///
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: contains result for each field, origin and value with postfix</para>
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR</para>
        /// </summary>
        public Birokrat_RegistryAndIniComparer_Check Buid_Birokrat_RegistryAndIniComparer_Check(
            string sqlServerName,
            string biroExeLoc = "")
        {
            return new Birokrat_RegistryAndIniComparer_Check(
                new IniSettingsHelper(new FileSystem()),
                new RegistrySettingsHelper(),
                new RegistryUtilsAccess(),
                sqlServerName,
                biroExeLoc);
        }

        /// <summary>
        /// Checks if date and time are in correct format. Checks if system language for unicode programs is set to Slovenian. Wit repair option it will also fix settings
        /// in registry. If locale language is changed (WARNING postfix) system will need a reboot
        ///
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains: date format, time format and system locale language for unicode programs</para>
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, ERROR, WARNING</para>
        /// </summary>
        public Region_DateTimeLocale_CheckAndRepair Build_Region_DateTimeLocale_CheckAndRepair(bool doRepair = false, Func<string, bool> waitForConfirmation = null)
        {
            return new Region_DateTimeLocale_CheckAndRepair(
                new CultureInfoHelper(),
                new RegistryUtilsAccess(),
                doRepair,
                waitForConfirmation);
        }

        /// <summary>
        /// Checks if the .NET SDKs required for building and running the application are installed. 
        /// It runs the `dotnet --list-sdks` command to retrieve installed SDK versions and verifies if 
        /// specific SDK versions (e.g., .NET Core 3.1 and 2.1) are present.
        ///
        /// <para>If the `dotnet` command is unavailable, the result will indicate that the .NET CLI is not installed.</para>
        /// <para>If the required SDK versions are missing, the result will include warnings specifying which versions are absent.</para>
        ///
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains:</para>
        /// <list type="bullet">
        ///     <item><description>All detected SDK versions found using `dotnet --list-sdks`.</description></item>
        ///     <item><description>Warnings for any missing required SDK versions (e.g., .NET Core 3.1 or 2.1).</description></item>
        /// </list>
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, WARNING, ERROR </para>
        /// </summary>
        public ICheck Build_Sdk_DotNetVersions_Check(params string[] dotnetVersions)
        {
            return new Sdk_DotnetVersions_Check(new InstalledFrameworkReader(), dotnetVersions);
        }

        /// <summary>
        /// Checks if the required .NET runtimes for running the application are installed. 
        /// It runs the `dotnet --list-runtimes` command to retrieve installed runtime versions and verifies if 
        /// specific versions (e.g., .NET Core 3.1 and 2.1) are present.
        ///
        /// <para>If the `dotnet` command is unavailable, the result will indicate that the .NET CLI is not installed.</para>
        /// <para>If the required runtime versions are missing, the result will include warnings specifying which versions are absent.</para>
        ///
        /// <para>Returned <see cref="ResultRecord.AdditionalInfo"/> contains:</para>
        /// <list type="bullet">
        ///     <item><description>All detected SDK versions found using `dotnet --list-sdks`.</description></item>
        ///     <item><description>Warnings for any missing required SDK versions (e.g., .NET Core 3.1 or 2.1).</description></item>
        /// </list>
        /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, WARNING, ERROR </para>
        /// </summary>
        public ICheck Build_Runtimes_DotnetVersions_Check(params string[] dotnetVersions)
        {
            return new Runtimes_DotnetVersions_Check(new InstalledFrameworkReader(), dotnetVersions);
        }
    }
}
