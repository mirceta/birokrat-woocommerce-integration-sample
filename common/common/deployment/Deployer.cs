using si.birokrat.next.common.build;
using si.birokrat.next.common.logging;
using si.birokrat.next.common.shell;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace si.birokrat.next.common.deployment {
    public static class Deployer
    {
        private const string DEFAULT_DESTINATION_PATH = "deploy";

        public static bool PublishCoreProject(
            string solutionPath,
            string netVersion, // now takes version like "6.0", "7.0", "8.0", "9.0"
            string projectName,
            string destinationFullPath = DEFAULT_DESTINATION_PATH)
        {
            Console.Write($"Publishing {projectName} project");

            string projectPath = Path.Combine(solutionPath, projectName);
            string projectDeployPath = Path.Combine(destinationFullPath, projectName);

            if (Directory.Exists(projectDeployPath))
            {
                Directory.Delete(projectDeployPath, recursive: true);
            }

            // Validate and map to proper TFMs
            string framework = MapVersionToTFM(netVersion); // e.g., "net8.0"

            string command = $"dotnet publish \"{projectPath}\" " +
                             $"--configuration Release " +
                             $"--framework {framework} " +
                             $"--output \"{projectDeployPath}\" " +
                             $"--verbosity normal";

            string result = CommandPrompt.Execute(command);

            bool success = result.Contains("Build succeeded");
            if (success)
            {
                CopyAndDeleteSettings(projectPath, projectDeployPath);
                CreateCleanLog(projectDeployPath);
                Console.WriteLine("SUCCESS");
            }
            else
            {
                Console.WriteLine("...ERROR\n\nPress any key to exit.");
                Console.ReadLine();
                throw new Exception("Exit environment!");
            }

            return success;
        }

        private static string MapVersionToTFM(string version)
        {
            version = version.Trim().ToLowerInvariant();

            if (version.StartsWith("netcoreapp") || version.StartsWith("net"))
                return version; // allow full TFMs like "net8.0-windows"

            if (version.StartsWith("2.") || version.StartsWith("3."))
                return $"netcoreapp{version}";

            if (version.StartsWith("5.") || version.StartsWith("6.") ||
                version.StartsWith("7.") || version.StartsWith("8.") || version.StartsWith("9."))
                return $"net{version}-windows"; // force windows for desktop

            throw new ArgumentException($"Unsupported .NET version format: '{version}'");
        }

        public static bool PublishFrameworkProject(string commonSolutionPath, string solutionPath, string projectName, string destinationPath = DEFAULT_DESTINATION_PATH)
        {
            string msBuildPath = GetMSBuildPath(commonSolutionPath);

            Console.Write($"Publishing {projectName} project");

            string projectPath = Path.Combine(solutionPath, projectName);
            string projectDeployPath = Path.Combine(solutionPath, destinationPath, projectName);

            if (Directory.Exists(projectDeployPath))
            {
                Directory.Delete(projectDeployPath, recursive: true);
            }

            string command = $"\"{msBuildPath}\" " +
                $"/target:Publish " +
                $"/p:Configuration=Release " +
                $"/p:OutDir={Path.Combine(projectDeployPath, @"bin\Release")} " +
                $"\"{projectPath}\"";
            string result = CommandPrompt.Execute(command);

            var success = result.Contains("Build succeeded");
            if (success)
            {
                CreateCleanLog(projectDeployPath);
                Console.WriteLine("SUCCESS");
            }
            else
            {
                Console.WriteLine("...ERROR\n\nPress any key to exit.");
                Console.ReadLine();
                throw new Exception("Exit environemnt!");
            }

            return success;
        }

        private static string GetMSBuildPath(string commonSolutionPath)
        {
            var vsWherePath = Path.Combine(commonSolutionPath, @"common\deployment\tools\vswhere.exe");
            string result = CommandPrompt.Execute(vsWherePath);

            var match = Regex.Match(result, $"installationPath:.*{Environment.NewLine}");
            if (match.Success)
            {
                var visualStudioInstallPath = match.Value.Trim().Split(new[] { ": " }, StringSplitOptions.None)[1];
                return Path.Combine(visualStudioInstallPath, @"MSBuild\Current\Bin\MSBuild.exe");
            }
            else
            {
                Console.WriteLine("Could not determine 'MSBuild.exe' path.");
                Console.ReadLine();
                throw new Exception("Exit environemnt!");
            }

            return null;
        }

        private static void CopyAndDeleteSettings(string projectPath, string projectDeployPath)
        {
            var secretsProductionPath = Path.Combine(projectPath, "appsettings.Secrets.Production.json");
            var secretsExamplePath = Path.Combine(projectPath, "appsettings.Secrets.Example.json");
            var secretsDeployPath = Path.Combine(projectDeployPath, "appsettings.Secrets.json");
            var bakSecretsDeployPath = Path.Combine(projectDeployPath, "bakappsettings.Secrets.json");

            if (File.Exists(secretsProductionPath))
            {
                File.Copy(secretsProductionPath, secretsDeployPath, overwrite: true);
                File.Copy(secretsProductionPath, bakSecretsDeployPath, overwrite: true);
            }
            else
            {
                File.Copy(secretsExamplePath, secretsDeployPath, overwrite: true);
                File.Copy(secretsExamplePath, bakSecretsDeployPath, overwrite: true);
            }

            var developmentDeployPath = Path.Combine(projectDeployPath, "appsettings.Development.json");
            File.Delete(developmentDeployPath);

            var secretsProductionDeployPath = Path.Combine(projectDeployPath, "appsettings.Secrets.Production.json");
            if (File.Exists(secretsProductionDeployPath))
            {
                File.Delete(secretsProductionDeployPath);
            }

            var secretsExampleDeployPath = Path.Combine(projectDeployPath, "appsettings.Secrets.Example.json");
            File.Delete(secretsExampleDeployPath);
        }

        private static void CreateCleanLog(string projectDeployPath)
        {
            var logPath = Path.Combine(projectDeployPath, Logger.FILE);
            File.WriteAllText(logPath, string.Empty);
        }
    }
}
