using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace common_ops.SharedSettings
{
    public class CredentialsSqlServer
    {
        private readonly string FILE_NAME = "\\\\sqlbirokrat\\Birokrat ni za distribucijo\\Bironext\\delivery\\SharedSettings\\credentialsSqlServer.json";

        public string FetchSqlServerName()
        {
            var json = File.ReadAllText(FILE_NAME);

            var config = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

            if (config.TryGetValue("Sql", out var server))
            {
                if (!string.IsNullOrEmpty(server))
                    return server;
            }

            throw new KeyNotFoundException("Could not found sql credentials server location");
        }
    }
}
