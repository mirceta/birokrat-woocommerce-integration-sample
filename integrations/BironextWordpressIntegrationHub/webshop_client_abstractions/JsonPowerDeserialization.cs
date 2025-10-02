using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace core.tools.wooops
{

    public interface IJsonConvert {
        T Deserialize<T>(string some);
        string Serialize(object some);
    }

    public class JsonPowerDeserialization2 : IJsonConvert
    {

        public JsonPowerDeserialization2() { }

        #region [IJsonConvert]
        public T Deserialize<T>(string some) {
            return DeserializeObjectImmuneToBadJSONEscapeSequenece<T>(some);
        }

        public string Serialize(object some) {
            return JsonConvert.SerializeObject(some);
        }
        #endregion


        // throws JsonReaderException
        public T DeserializeObjectImmuneToBadJSONEscapeSequenece<T>(string some) {
            while (true) {
                var chome = JsonConvert.DeserializeObject<T>(some, new JsonSerializerSettings {
                    Error = HandleDeserializationError
                });
                if (chome == null && !string.IsNullOrEmpty(ProblemEscapeSequenece)) {
                    some = some.Replace(ProblemEscapeSequenece, "");
                    ProblemEscapeSequenece = "";
                } else {
                    return chome;
                }

            }
        }

        string ProblemEscapeSequenece = "";
        public void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs) {
            try {
                if (errorArgs.ErrorContext.Error.Message.Contains("Bad JSON escape sequence")) {
                    string tmp = errorArgs.ErrorContext.Error.Message;
                    int start = tmp.IndexOf("Bad JSON escape sequence:") + "Bad JSON escape sequence:".Length;
                    int end = tmp.IndexOf(".", start);
                    string fin = tmp.Substring(start, end - start).Trim();
                    ProblemEscapeSequenece = fin;
                    errorArgs.ErrorContext.Handled = true;
                } else if (errorArgs.ErrorContext.Error.Message.Contains("Unexpected character encountered while parsing value")) {
                    errorArgs.ErrorContext.Handled = true;
                }
            } catch (Exception ex) { }

        }

    }
}
