using Newtonsoft.Json.Linq;
using System;

namespace order_mapping
{
    public class JArrayDifferenceFinder
    {
        public JArray FindDifference(JArray X, JArray Y)
        {
            if (X.Count != Y.Count)
            {
                throw new Exception("Array sizes do not match");
            }

            JArray Z = new JArray();
            var objectDifferenceFinder = new JObjectDifferenceFinder();

            for (int i = 0; i < X.Count; i++)
            {
                if (X[i].Type == JTokenType.Object && Y[i].Type == JTokenType.Object)
                {
                    JObject nestedDifference = objectDifferenceFinder.FindDifference((JObject)X[i], (JObject)Y[i]);

                    if (nestedDifference.HasValues)
                    {
                        Z.Add(nestedDifference);
                    }
                    else
                    {
                        Z.Add(new JObject());
                    }
                }
                else
                {
                    Z.Add(X[i].DeepClone());
                }
            }

            return Z;
        }
    }
}
