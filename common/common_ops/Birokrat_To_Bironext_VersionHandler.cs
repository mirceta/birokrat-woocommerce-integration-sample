using si.birokrat.next.common.build;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace common_ops
{
    /*
    Class responsibility is joined versioning of Birokrat and Bironext

    Joined versioning: versioning for bironext will be identical to birokrat.exe.

    This means that if birokrat.exe is version 8.0.44.000, then bironext will be 8.0.44.000 .
    We will pack the only valid version of birokrat.exe along with the project and we have it like this:
    Put it into bironext/server/dependencies/Birokrat.exe
    On build: exception if the current version written in AssemblyInfo.cs is lower than Birokrat’s current version!

    We have the following versions:
    - We have CBirokrat - which is is the version in C:\Birokrat
    - We have ExtDepBirokrat - which is the version written in server\external_dependencies\Birokrat.exe
    - We have Next - which is written in AssemblyInfo.cs of runner_global and FileVersion of runner_global.exe

    The responsibility of this class is to execute all operations related to these rules of versioning
     */
    public class Birokrat_To_Bironext_VersionHandler : IBirokrat_To_Bironext_VersionHandler
    {

        public string GetBironextVersion(string bironextDeployFolder = "")
        {

            // Check if the current assembly is runner_global
            Assembly executingAssembly = Assembly.GetEntryAssembly();
            string executingAssemblyName = executingAssembly.GetName().Name; //this will get current executing assembly from which the code is running. so common_ops RACO 11.12.2024

            if (executingAssemblyName.Equals("runner_global", StringComparison.OrdinalIgnoreCase))
            {
                // If running from runner_global, get the version from the executing assembly
                var version = executingAssembly.GetName().Version;
                string versionWithoutRevision = version.Major + "." + version.Minor + "." + version.Build;
                return versionWithoutRevision.ToString();
            }
            else if (Debugger.IsAttached && string.IsNullOrEmpty(bironextDeployFolder))// !!! WILL ONLY WORK IN DEBUG MODE BECAUSE IT REFERENCES BuildClient.SolutionPath!!!
            {
                // If called from another assembly, read the version from AssemblyInfo.cs
                string assemblyInfoPath = Path.Combine(Build.SolutionPath, "runner_global", "Properties", "AssemblyInfo.cs");
                string assemblyInfoContent = File.ReadAllText(assemblyInfoPath);

                // Use regex to find the assembly version
                Match match = Regex.Match(assemblyInfoContent, @"\[assembly: AssemblyVersion\(""(.*?)""\)\]");
                if (match.Success)
                {
                    return match.Groups[1].Value;  // Return the version number as a string
                }

                throw new Exception("Fatal error: There should always be a version in bironext/runner_global but no version was found in the AssemblyInfo.cs!");
            }
            else
            {
                // its not executed from runner_global and is not in debug mode
                if (string.IsNullOrEmpty(bironextDeployFolder) || !Directory.Exists(bironextDeployFolder))
                {
                    throw new Exception("Not valid bironext deploy folder path" + bironextDeployFolder);
                }

                var runnerGlobal = Path.Combine(bironextDeployFolder, "runner_global", "bin", "Release", "runner_global.exe");
                try
                {
                    var info = FileVersionInfo.GetVersionInfo(runnerGlobal);
                    return info.ProductVersion;
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to retrieve the version of runner_global. " + ex);
                }

                throw new Exception("Fatal error: There should always be a version in bironext/runner_global but no version was found in the AssemblyInfo.cs!");
            }
        }

        public void Verify_CBirokrat_And_Bironext_VersionsAreSame(Action<string> logfunc, string bironextDeployFolder = "")
        {
            // Path to Birokrat.exe
            string birokratPath = @"C:\Birokrat\Birokrat.exe";

            // Get the version of Birokrat.exe
            FileVersionInfo birokratVersionInfo = FileVersionInfo.GetVersionInfo(birokratPath);
            Version birokratVersion = new Version(birokratVersionInfo.ProductVersion);

            // Get the version of runner_global using the GetBironextVersion method
            string runnerGlobalVersionString = GetBironextVersion(bironextDeployFolder);
            if (runnerGlobalVersionString == null)
            {
                logfunc("Unable to retrieve the version of runner_global.");
                return;
            }
            Version runnerGlobalVersion = new Version(runnerGlobalVersionString);

            // Normalize and compare the versions
            if (NormalizeVersion(birokratVersion) == NormalizeVersion(runnerGlobalVersion))
            {
                logfunc("The versions match: Birokrat.exe and runner_global.exe are the same version.");
            }
            else
            {
                string err = "";
                err += "Version mismatch detected.\n";
                err += $"Birokrat.exe version: {birokratVersion}\n";
                err += $"runner_global.exe version: {runnerGlobalVersion}\n";
                err += "Please ensure both are of the same version.\n";
                throw new Exception(err);
            }
        }

        private Version NormalizeVersion(Version version)
        {
            // Ensure components are non-negative and avoid uninitialized values
            int major = Math.Max(version.Major, 0);
            int minor = Math.Max(version.Minor, 0);
            int build = Math.Max(version.Build, 0);
            int revision = Math.Max(version.Revision, 0);

            // Strip trailing zero components from the version number
            int[] components = { major, minor, build, revision };
            int lastNonZeroIndex = components.Length - 1;

            // Find the last non-zero component
            while (lastNonZeroIndex >= 0 && components[lastNonZeroIndex] == 0)
            {
                lastNonZeroIndex--;
            }

            // Create a new version with only the significant components
            switch (lastNonZeroIndex)
            {
                case -1: return new Version(0, 0); // All components are zero
                case 0: return new Version(components[0], 0); // Only major component is significant
                case 1: return new Version(components[0], components[1]);
                case 2: return new Version(components[0], components[1], components[2]);
                default: return new Version(components[0], components[1], components[2], components[3]);
            }
        }





        /*
        Writes server/external_dependencies/Birokrat.exe 's version into server/runner_global 's AssemblyInfo.cs
        thereby copying Birokrat version to bironext version.
        */
        public void Copy_ExtDepBirokratVersion_To_BironextVersion()
        {
            Copy_ServerProj_ExtDepBirokratVersion_To_BironextVersion();
        }

        private void Copy_ServerProj_ExtDepBirokratVersion_To_BironextVersion()
        {
            Version birokratVersion = GetExternalDependencyBirokratVersion();
            string assemblyInfoPath = Path.Combine(Build.SolutionPath, "runner_global", "Properties", "AssemblyInfo.cs");
            string assemblyInfoContent = File.ReadAllText(assemblyInfoPath);

            ExitIf_ExtDepBirokratVersion_Is_TooOld(birokratVersion, assemblyInfoContent);
            Overwrite_RunnerGlobalAssemblyInfoVersion_With_BirokratVersion(birokratVersion, assemblyInfoPath, assemblyInfoContent);
        }


        private void Overwrite_RunnerGlobalAssemblyInfoVersion_With_BirokratVersion(Version birokratVersion, string assemblyInfoPath, string assemblyInfoContent)
        {

            string versionPattern = @"\d+(\.\d+){1,3}";

            assemblyInfoContent = Regex.Replace(assemblyInfoContent,
                                                $@"\[assembly: AssemblyVersion\(""{versionPattern}""\)\]",
                                                $"[assembly: AssemblyVersion(\"{birokratVersion}\")]");
            assemblyInfoContent = Regex.Replace(assemblyInfoContent,
                                                $@"\[assembly: AssemblyFileVersion\(""{versionPattern}""\)\]",
                                                $"[assembly: AssemblyFileVersion(\"{birokratVersion}\")]");
            File.WriteAllText(assemblyInfoPath, assemblyInfoContent);

        }

        private Version GetExternalDependencyBirokratVersion()
        {
            string biropath = Path.Combine(Build.SolutionPath, "external_dependencies", "Birokrat.exe");
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(biropath);
            Version birokratVersion = new Version(fileVersionInfo.ProductVersion);
            return birokratVersion;
        }

        private void ExitIf_ExtDepBirokratVersion_Is_TooOld(Version birokratVersion, string assemblyInfoContent)
        {
            Match match = Regex.Match(assemblyInfoContent, @"\[assembly: AssemblyVersion\(""(.*?)""\)\]");
            if (match.Success)
            {
                Version currentAssemblyVersion = new Version(match.Groups[1].Value);

                // Compare versions
                if (birokratVersion < currentAssemblyVersion)
                {
                    string err = "Fatal error. When building a new version of bironext server, the version for bironext will be ";
                    err += "copied from 'server/external_dependencies/Birokrat.exe', effecively making the versions the same. The ";
                    err += "version of Birokrat.exe found in 'server/external_dependencies/Birokrat.exe' was lower than the version ";
                    err += "of bironext (written in runner_global/Properties/AssemblyInfo.cs) meaning that bironext was build before ";
                    err += "with a newer version of Birokrat.exe . This would require us to lower the version of bironext which is not allowed!";
                    err += "To fix this issue, put a newer version of Birokrat.exe into 'server/external_dependencies'!";
                    throw new Exception(err);
                }
            }
        }
    }
}
