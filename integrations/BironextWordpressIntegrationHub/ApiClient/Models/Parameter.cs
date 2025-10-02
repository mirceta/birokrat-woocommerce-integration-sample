using Newtonsoft.Json;
using System.Collections.Generic;

namespace BirokratNext.Models
{
    public class Parameter
    {
        public string Label { internal get; set; }
        public string Type { get; set; }
        public string Code { get; set; }
        public string Value { get; set; }
        public List<string> DataSet { internal get; set; }

        public override string ToString()
        {
            var output = $"Parameter '{Label}': type = {Type} | code = {Code}";

            if (!string.IsNullOrEmpty(Value))
            {
                output += $" | value = {Value}";
            }

            if (Type == "Select" && DataSet.Count > 0)
            {
                output += $" | data set = [{string.Join(", ", DataSet)}]";
            }

            return output;
        }
    }
}
