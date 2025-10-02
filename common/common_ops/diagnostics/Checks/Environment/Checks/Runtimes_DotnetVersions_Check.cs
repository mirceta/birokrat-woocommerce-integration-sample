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
    ///     <item><description>Warnings for any missing required Runtime versions (e.g., .NET Core 3.1 or 2.1).</description></item>
    /// </list>
    /// <para> <see cref="ResultRecord.AdditionalInfo"/> postfixes: OK, WARNING, ERROR </para>
    /// </summary>
    public class Runtimes_DotnetVersions_Check : ICheck
    {
        private readonly IInstalledFrameworkReader _dotNetVersionReader;
        private readonly string[] _dotnetVersions;

        private readonly string ASP_NET = "AspNetCore.";
        private readonly string NET_CORE = "NETCore.";
        private readonly string NET_DESKTOP = "WindowsDesktop.";

        /// <summary>
        /// <inheritdoc cref="Sdk_DotnetVersions_Check"/>
        /// </summary>
        /// <param name="dotnetVersions">by default it will search for versions 3.1, 2.1, 6, 7, 8, 9. You can provide more versions.
        /// Example: "2.1", "3.1", "7", "8". Default search is ONLY done if there are no arguments. For custom search you need to provide
        /// all required versions. </param>
        public Runtimes_DotnetVersions_Check(IInstalledFrameworkReader dotNetVersionReader, params string[] dotnetVersions)
        {
            _dotNetVersionReader = dotNetVersionReader;
            _dotnetVersions = dotnetVersions;

            if (dotnetVersions.Length == 0)
                _dotnetVersions = new string[] { "2.1", "3.1", "6.0", "7.0", "8.0", "9.0" };
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
                string output = await _dotNetVersionReader.FetchRuntimes();

                if (DoesContainAllVersions(output, out var additionalInfo))
                {
                    additionalInfo.Add($"All .NET versions installed. OK");
                    return new ResultRecord(true, GetType().Name, additionalInfo.ToArray());
                }
                else
                {
                    additionalInfo.Add("All missing runtimes MUST be installed or the code won't work!");
                    return new ResultRecord(false, GetType().Name, additionalInfo.ToArray());
                }
            }
            catch (Exception ex)
            {
                return new ResultRecord(false, GetType().Name, "Error: " + ex.Message + " " + TextConstants.POSTFIX_ERROR);
            }
        }

        private bool DoesContainAllVersions(string input, out List<string> additionalInfo)
        {
            additionalInfo = new List<string>();

            var result = true;
            var inputLines = input.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var item in _dotnetVersions)
            {
                var pattern = BuildPattern(item);
                var runtimes = inputLines.Where(x => Regex.IsMatch(x, pattern)).ToArray();

                if (runtimes.Length == 0)
                {
                    result = false;
                    additionalInfo.Add(".NET Runtime version '" + item + "' not installed! ");
                }
                else
                {
                    if (!runtimes.Any(x => x.IndexOf(ASP_NET, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        additionalInfo.Add(".NET Runtime AspNetCore version '" + item + "' missing!" + AttachEnding(TextConstants.POSTFIX_ERROR));
                        result = false;
                    }
                    else
                        additionalInfo.Add(".NET Runtime AspNetCore version '" + item + "' installed." + AttachEnding(TextConstants.POSTFIX_OK));

                    if (!runtimes.Any(x => x.IndexOf(NET_CORE, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        additionalInfo.Add(".NET Runtime Core Framework version '" + item + "' missing!" + AttachEnding(TextConstants.POSTFIX_ERROR));
                        result = false;
                    }
                    else
                        additionalInfo.Add(".NET Runtime Core Framework version '" + item + "' installed." + AttachEnding(TextConstants.POSTFIX_OK));

                    if (item.StartsWith("2."))
                    {
                        additionalInfo.Add(".NET version " + item + " does not have WindowsDesktop Runtime." + AttachEnding(TextConstants.POSTFIX_OK));
                        continue; // 2. net version does not have desktop runtime
                    }

                    if (!runtimes.Any(x => x.IndexOf(NET_DESKTOP, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        additionalInfo.Add(".NET Runtime WindowsDesktop version '" + item + "' missing!" + AttachEnding(TextConstants.POSTFIX_ERROR));
                        result = false;
                    }
                    else
                        additionalInfo.Add(".NET Runtime WindowsDesktop version '" + item + "' installed." + AttachEnding(TextConstants.POSTFIX_OK));
                }
            }
            return result;
        }

        private string AttachEnding(string ending)
        {
            return $"{TextConstants.DELIMITER}{ending}";
        }

        private string BuildPattern(string item)
        {
            var versions = item.Trim().Split('.');

            if (versions.Length == 1)
                return $@"{Regex.Escape(versions[0])}\.\d+\.\d+";
            if (versions.Length == 2)
                return $@"{Regex.Escape(versions[0])}\.{Regex.Escape(versions[1])}\.\d+";
            if (versions.Length == 3)
                return $@"{Regex.Escape(versions[0])}\.{Regex.Escape(versions[1])}\.{Regex.Escape(versions[2])}";

            throw new Exception("Not valid version format");
        }
    }
}
