namespace common_ops.Abstractions
{
    public interface IPathSystem
    {
        string GetDirectoryName(string path);
        string GetFileExtension(string file);
        string GetFileName(string path);
        string GetFileNameWithoutExtension(string path);
    }
}
