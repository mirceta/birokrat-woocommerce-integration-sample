using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace si.birokrat.next.common.build.Functionality
{
    public class BuildDependencyProvider : IBuildDependencyProvider
    {
        public string GetFrameworkDescription()
        {
            return RuntimeInformation.FrameworkDescription;
        }

        public string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public string GetExecutingAssemblyLocation()
        {
            return Assembly.GetExecutingAssembly().Location;
        }
    }
}
