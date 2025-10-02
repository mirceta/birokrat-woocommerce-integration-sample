using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace common_python
{
    public class PythonInstallationChecker
    {
        public static bool IsLibraryInstalled(string pythonPath, string libraryName)
        {
            try
            {
                // Prepare the python command to run your script
                string cmd = $"{pythonPath} -c \"import {libraryName}\"";

                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = pythonPath; // python executable path e.g., "python" or full path "C:\\Python39\\python.exe"
                start.Arguments = $"-c \"import {libraryName}\""; // Arguments to run the script
                start.UseShellExecute = false; // Do not use OS shell
                start.RedirectStandardOutput = true; // Redirect output so we can read it
                start.RedirectStandardError = true; // Redirect error output so we can read it

                using (Process process = Process.Start(start))
                {
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string result = reader.ReadToEnd(); // Read the output
                        process.WaitForExit(); // Wait for the script to finish

                        if (process.ExitCode == 0)
                        {
                            Console.WriteLine($"{libraryName} is installed");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine($"{libraryName} is not installed");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if any version of Python is installed on the system.
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static bool IsPythonInstalled()
        {
            try
            {
                var result = FetchPythonVersions();

                // Check if the output contains 'Python' which is typical in Python version output
                return result.Contains("Python");
            }
            catch (Exception)
            {
                // Python not installed or not in PATH
                return false;
            }
        }

        /// <summary>
        /// <summary>
        /// Returns <c>true</c> if the specified Python version is installed on the system.  
        /// The version should be provided in the format <c>major.minor.patch</c> (e.g., 3.12.0).  
        /// Partial versions checks such as <c>3.12</c> or <c>3</c> are also supported.
        /// </summary>
        /// </summary>
        /// <param name="version"></param>
        /// <returns></returns>
        public static (bool isInstaled, string fullVersion) IsPythonInstalled(string version)
        {
            var pattern =  BuildVersionSearchPattern(version);

            var installed = FetchPythonVersions();

            if (Regex.IsMatch(installed, pattern))
                return (true, installed);

            return (false, string.Empty);
        }

        private static string BuildVersionSearchPattern(string version)
        {
            var versions = version.Trim().Split('.');

            if (versions.Length == 1)
                return $@"{Regex.Escape(versions[0])}\.\d+\.\d+";
            if (versions.Length == 2)
                return $@"{Regex.Escape(versions[0])}\.{Regex.Escape(versions[1])}\.\d+";
            if (versions.Length == 3)
                return $@"{Regex.Escape(versions[0])}\.{Regex.Escape(versions[1])}\.{Regex.Escape(versions[2])}";

            throw new Exception("Not valid version format");
        }

        private static string FetchPythonVersions()
        {
            // Start the Python process with --version argument
            ProcessStartInfo procStartInfo = new ProcessStartInfo("python", "--version")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proc = new Process() { StartInfo = procStartInfo })
            {
                proc.Start(); // Start the process
                var result =  proc.StandardOutput.ReadToEnd(); // Read the output
                return result;
            }
        }
    }
}
