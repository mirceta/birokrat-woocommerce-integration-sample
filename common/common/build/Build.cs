using si.birokrat.next.common.build.Functionality;

namespace si.birokrat.next.common.build
{
    public static class Build 
    {
        private static IBuild _build;

        static Build()
        {
            _build = new BuildInternal(new BuildDependencyProvider());
        }

        public static string Configuration => _build.GetConfiguration();

        public static string Framework => _build.GetFramework();

        public static string ProjectPath => _build.GetProjectPath();

        public static string SolutionPath => _build.GetSolutionPath();
    }

}
