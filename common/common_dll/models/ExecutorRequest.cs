using Newtonsoft.Json;
using si.birokrat.next.common_proxy_standard.models;
using System.Collections.Generic;

namespace si.birokrat.next.common_dll.models {
    public class ExecutorRequest {
        public DllInfo Info { get; set; }

        public Method Method { get; set; }

        public string Language { get; set; } = string.Empty;

        public int Page { get; set; } = 1;

        public Dictionary<string, object> Content { get; set; }

        #region // json converting //
        public static ExecutorRequest FromJson(string data) {
            return JsonConvert.DeserializeObject<ExecutorRequest>(data);
        }

        public string ToJson(bool indented = false) {
            return JsonConvert.SerializeObject(this, indented ? Formatting.Indented : Formatting.None);
        }
        #endregion
    }
}
