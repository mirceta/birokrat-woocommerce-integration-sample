using Newtonsoft.Json;

namespace si.birokrat.next.common_dll.models {
    public class ExecutorResponse {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;

        public string ErrorMessage { get; set; } = string.Empty;

        public object Result { get; set; } = null;

        public static ExecutorResponse FromJson(string data) {
            return JsonConvert.DeserializeObject<ExecutorResponse>(data);
        }

        public string ToJson(bool indented = false) {
            return JsonConvert.SerializeObject(this, indented ? Formatting.Indented : Formatting.None);
        }
    }
}
