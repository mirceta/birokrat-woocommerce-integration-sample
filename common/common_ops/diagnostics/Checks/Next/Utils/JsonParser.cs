using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Text.RegularExpressions;

namespace common_ops.diagnostics.Checks.Next.Utils
{
    public class JsonParser : IJsonParser
    {
        private readonly char KEY_SEPARATOR = ':';

        public Dictionary<string, string> BuildConfig(string item)
        {
            try
            {
                var file = File.ReadAllText(item);
                var cleaned = RemoveComments(file);
                var config = FlattenJson(cleaned);

                return config;
            }
            catch (Exception ex)
            {
                return new Dictionary<string, string>();
            }
        }

        private string RemoveComments(string file)
        {
            string pattern = @"//.*?$";
            string cleanedJson = Regex.Replace(file, pattern, "", RegexOptions.Multiline);
            return cleanedJson;
        }

        public Dictionary<string, string> FlattenJson(string json)
        {
            var dictionary = new Dictionary<string, string>();
            try
            {
                var jObject = JObject.Parse(json);
                FillDictionaryFromJToken(dictionary, jObject, "");
            }
            catch (Exception e) { }

            return dictionary;
        }

        private void FillDictionaryFromJToken(Dictionary<string, string> dict, JToken token, string prefix)
        {
            foreach (var property in token.Children<JProperty>())
            {
                string key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}{KEY_SEPARATOR}{property.Name}";
                if (property.Value.Type == JTokenType.Object)
                {
                    FillDictionaryFromJToken(dict, property.Value, key);
                }
                else
                {
                    dict[key] = property.Value.ToString();
                }
            }
        }

        public string SaveConfig(Dictionary<string, string> config, string filePath)
        {
            var errors = string.Empty;
            try
            {
                var json = UnflattenJson(config);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                errors = $"Failed to write config '{filePath}' to file: {ex.Message}";
            }
            return errors;
        }

        public string UnflattenJson(Dictionary<string, string> flatConfig)
        {
            var nestedObject = new ExpandoObject() as IDictionary<string, object>;

            foreach (var kvp in flatConfig)
            {
                var keys = kvp.Key.Split(KEY_SEPARATOR);
                IDictionary<string, object> current = nestedObject;

                for (int i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];

                    if (i == keys.Length - 1) // Last key
                    {
                        if (int.TryParse(kvp.Value, out int result))
                            current[key] = result;
                        else
                            current[key] = kvp.Value;
                    }
                    else
                    {
                        if (!current.ContainsKey(key))
                        {
                            current[key] = new ExpandoObject();
                        }
                        current = current[key] as IDictionary<string, object>;
                    }
                }
            }

            return JsonConvert.SerializeObject(nestedObject, Formatting.Indented);
        }
    }

}
