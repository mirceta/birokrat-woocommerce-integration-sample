namespace si.birokrat.next.common.build.Functionality
{
    public interface IBuildDependencyProvider
    {
        string GetDirectoryName(string path);
        string GetExecutingAssemblyLocation();
        string GetFrameworkDescription();
    }
}
