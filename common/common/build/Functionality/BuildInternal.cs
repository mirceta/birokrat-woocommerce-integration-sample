using si.birokrat.next.common.logging;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace si.birokrat.next.common.build.Functionality
{
    /// <summary>
    /// USE Build IN PROJECTS! 
    /// </summary>
    public class BuildInternal : IBuild
    {
        private readonly IBuildDependencyProvider _dependencies;

        public BuildInternal(IBuildDependencyProvider buildDependencyProvider)
        {
            _dependencies = buildDependencyProvider;
        }

        public string GetConfiguration()
        {
            var configuration = "Release";
#if DEBUG
            configuration = "Debug";
#endif
            return configuration;
        }

        public string GetFramework()
        {
            var framework = RuntimeInformation.FrameworkDescription;
            if (framework.Contains(".NET Framework"))
            {
                return "Framework";
            }
            if (framework.Contains(".NET") && (framework.Contains("Core") || framework.Contains("6") ||
                framework.Contains("7") || framework.Contains("8") || framework.Contains("9")))
            {
                return "Core";
            }
            else
            {
                string error = $"Framework {framework} is not supported.";
                Logger.Log("Exception", error);
                throw new NotSupportedException(error);
            }
        }

        public string GetProjectPath()
        {
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (currentDirectory.Contains(@"\bin\Debug") || currentDirectory.Contains(@"\bin\Release"))
            {
                switch (GetFramework())
                {
                    case "Core":
                        return Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\.."));
                    case "Framework":
                        return Path.GetFullPath(Path.Combine(currentDirectory, @"..\.."));
                    default:
                        return Path.GetFullPath(currentDirectory);
                }
            }
            else
            {
                return Path.GetFullPath(currentDirectory);
            }
        }

        public string GetSolutionPath()
        {
            return Path.GetFullPath(Path.Combine(GetProjectPath(), ".."));
        }
    }
}
