using BironextWordpressIntegrationHub.structs;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace tests.tests.estrada
{
    public class FolderOrderStore : IOrderStore
    {
        string folderpath;
        public FolderOrderStore(string folderpath) {
            this.folderpath = folderpath;
        }

        public async Task<List<string>> GetOrders() {
            EnsureFolderExists();

            List<string> all = Directory.GetFiles(folderpath).ToList();
            List<string> results = all.Select(x => File.ReadAllText(x)).ToList();
            return results;
        }

        public void SaveOrder(WoocommerceOrder order) {
            EnsureFolderExists();
            string path = Path.Combine(folderpath, $"{order.Data.Id}.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(order));
        }

        private void EnsureFolderExists() {
            if (!Directory.Exists(folderpath)) Directory.CreateDirectory(folderpath);
        }
    }
}
