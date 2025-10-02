using Newtonsoft.Json.Linq;

namespace order_mapping
{
    public class JObjectDifferenceFinder
    {
        public JObject FindDifference(JObject X, JObject Y)
        {
            JObject Z = new JObject();

            foreach (var property in X.Properties())
            {
                Z = HandleProperty(Y, Z, property);
            }

            return Z;
        }

        private JObject HandleProperty(JObject Y, JObject Z, JProperty property)
        {
            JToken YProperty;
            bool foundInY = Y.TryGetValue(property.Name, out YProperty);

            if (foundInY)
            {
                if (property.Value.Type == JTokenType.Object && YProperty.Type == JTokenType.Object)
                {
                    JObject nestedDifference = FindDifference((JObject)property.Value, (JObject)YProperty);

                    if (nestedDifference.HasValues)
                    {
                        Z.Add(property.Name, nestedDifference);
                    }
                }
                else if (property.Value.Type == JTokenType.Array && YProperty.Type == JTokenType.Array)
                {
                    var arrayDifferenceFinder = new JArrayDifferenceFinder();
                    JArray arrayZ = arrayDifferenceFinder.FindDifference((JArray)property.Value, (JArray)YProperty);

                    Z.Add(property.Name, arrayZ);
                }
                else if (property.Value.Type != YProperty.Type)
                {
                    Z.Add(property.Name, property.Value.DeepClone());
                }
            }
            else
            {
                Z.Add(property.Name, property.Value.DeepClone());
            }

            return Z;
        }
    }
}
