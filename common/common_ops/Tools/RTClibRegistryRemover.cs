using common_ops.Executors.Shell;
using System.Threading.Tasks;

namespace common_ops.Tools
{
    public class RTClibRegistryRemover
    {
        private readonly IShellExecutor _shellExecutor;
        private readonly string BASE_KEY = "HKCR";
        private readonly string SEARCH_TERM = "libBiroRtc";

        internal RTClibRegistryRemover(IShellExecutor shellExecutor)
        {
            _shellExecutor = shellExecutor;
        }

        public async Task<string> DeleteEntries()
        {
            string command = $@"
            $matchedKeys = @();
            $currentKey = '';
            reg query '{BASE_KEY}' /s | ForEach-Object {{
                if ($_ -match '^HKEY_CLASSES_ROOT\\') {{ $currentKey = $_.Trim() }}
                if ($_ -like '*{SEARCH_TERM}*') {{ if ($currentKey -ne '') {{ $matchedKeys += $currentKey }} }}
            }};
            $matchedKeys = $matchedKeys | Sort-Object -Unique;
            foreach ($key in $matchedKeys) {{
                Write-Host 'DELETING: ' $key;
                reg delete ""$key"" /f
            }}";

            var result = await _shellExecutor.ExecuteInBackgroundAsync(command, true);
            return result;
        }

        public async Task<string> FetchEntries()
        {
            string command = "reg query '" + BASE_KEY + "' /s | ForEach-Object { if ($_ -like '*" + SEARCH_TERM + "*') { Write-Host $_ } }";
            var result = await _shellExecutor.ExecuteInBackgroundAsync(command, true);
            return result;
        }
    }
}
