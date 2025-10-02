using common_ops.Abstractions;
using common_ops.diagnostics.Checks.Location.Checks;
using common_ops.diagnostics.Checks.Location.Utils;

namespace common_ops.diagnostics.Checks.Dll
{
    public class DllChecksFactory
    {
        public Dll_Version_Check Build_DllVersionCheck(string localDllFolder = "", bool doRepair = false)
        {
            return new Dll_Version_Check(
                new BirokratDll_Location_Check(
                    new LocationHelper(),
                    new DirectorySystem()),
                new DirectorySystem(),
                localDllFolder,
                doRepair);
        }
    }
}
