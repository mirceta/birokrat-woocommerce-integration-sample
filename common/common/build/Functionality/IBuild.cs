namespace si.birokrat.next.common.build.Functionality
{
    public interface IBuild
    {
        string GetConfiguration();
        string GetFramework();
        string GetProjectPath();
        string GetSolutionPath();
    }
}
