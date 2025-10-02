namespace common_ops.diagnostics.Checks.Location.Utils
{
    public interface IReadonlySetter
    {
        void MakeFileReadonly(string file);
        void RemoveReadonlyFromFile(string file);
    }
}