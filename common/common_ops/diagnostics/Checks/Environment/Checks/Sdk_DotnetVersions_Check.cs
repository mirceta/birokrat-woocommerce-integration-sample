using common_ops.diagnostics.Checks.Environment.Utils;
using common_ops.diagnostics.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace common_ops.diagnostics.Checks.Environment.Checks
{
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

    public class Sdk_DotnetVersions_Check : ICheck
    {
        private readonly IInstalledFrameworkReader _dotNetVersionReader;
        private readonly string[] _dotnetVersions;

        /// <summary>
        /// <inheritdoc cref="Sdk_DotnetVersions_Check"/>
        /// </summary>
        /// <param name="dotnetVersions">by default it will search for 2.1 and 3.1. You can provide more versions. Example: "2.1", "3.1", "8.0". Default "2.1", "3.1" search is ONLY
        /// done if there are no arguments. For custom search you need to provide all required</param>
        public Sdk_DotnetVersions_Check(IInstalledFrameworkReader dotNetVersionReader, params string[] dotnetVersions)
        {
            _dotNetVersionReader = dotNetVersionReader;
            _dotnetVersions = dotnetVersions;

            if (dotnetVersions.Length == 0)
                _dotnetVersions = new string[] { "2.1", "3.1" };
        }

        public async Task<ResultRecord> Run()
        {
            try
            {
                return await Work();
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private async Task<ResultRecord> Work()
        {
            try
            {
                string output = await _dotNetVersionReader.FetchSDKs();
                var results = new List<string>();

                if (DoesContainAllVersions(output, results))
                    results.Add($"All .NET versions installed: " + _dotnetVersions.Aggregate((x, next) => x + ", " + next) + $". {TextConstants.POSTFIX_OK}");
                else
                    results.Add($"This is only necessary for building and running next from code. {TextConstants.POSTFIX_WARNING}");

                return new ResultRecord(true, GetType().Name, results.ToArray());
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_WARNING);
            }
        }

        private bool DoesContainAllVersions(string output, List<string> results)
        {
            var result = true;
            foreach (var item in _dotnetVersions)
            {
                string pattern = @"\b" + Regex.Escape(item);
                if (!Regex.IsMatch(output, pattern))
                {
                    result = false;
                    results.Add(".NET SDK version '" + item + "' not installed! " + TextConstants.POSTFIX_WARNING);
                }
                else
                    results.Add(".NET SDK version '" + item + "' installed. " + TextConstants.POSTFIX_OK);
            }
            return result;
        }
    }
}
