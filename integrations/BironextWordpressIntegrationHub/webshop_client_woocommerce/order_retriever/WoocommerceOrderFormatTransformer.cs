
using BiroWoocommerceHubTests;
using core.tools.wooops;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using webshop_client_woocommerce;

namespace order_mapping
{
    public class WoocommerceOrderFormatTransformer
    {

        CouponGetter couponGetter;
        public WoocommerceOrderFormatTransformer(CouponGetter couponGetter)
        {
            this.couponGetter = couponGetter;
        }

        bool noCoupons = false;

        public async Task<JObject> TransformStageOne_InternetDependent(IOutApiClient x, JObject obj1)
        {
            var ids = GetProductIds(obj1);
            JArray products = new JArray();
            foreach (var id in ids)
            {
                string productJson = await x.Get($"products/{id}");
                JObject product = new JsonPowerDeserialization2().Deserialize<JObject>(productJson);

                VariationGetter getter = new VariationGetter((WooApiClient)x, id);
                //string variations = await x.Get($"products/{id}/variations?per_page=100&page=2");
                var variations = (await getter.Get()).ToString();
                JArray vars = buildVariations(variations);

                product["variations"] = vars;
                products.Add(product);
            }



            obj1["line_items"] = SetDestinationProperty((JArray)obj1.SelectToken("line_items"), products, "origin_product");
            obj1 = PutToDataAndItems(obj1);


            try
            {
                var pr = await couponGetter.Get();
                obj1["coupons"] = pr;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("\"code\":\"woocommerce_rest_cannot_view\""))
                    noCoupons = true;
                else
                    throw ex;
            }


            return obj1;
        }



        public async Task<JObject> TransformStageTwo_InternetIndependent(JObject obj)
        {
            CorrectDate((JToken)obj);

            if (!noCoupons)
            {
                JArray rel = (JArray)obj["data"]["coupon_lines"];
                JArray result = new JArray();
                foreach (var coup in rel)
                {
                    result.Add(coup["code"]);
                }
                obj["used_coupons_codes"] = result;
            }
            else
            {
                obj["coupons"] = new JArray();
                obj["used_coupons_codes"] = new JArray();
            }


            obj["shipping_method"] = "Poštnina";

            return obj;
        }

        #region [auxiliary]
        private static JArray buildVariations(string variations)
        {
            var vars = JArray.Parse(variations);
            for (int i = 0; i < vars.Count; i++)
            {

                ///////////////////////////////
                // handle variation id mapping
                JObject curr = (JObject)vars[i];
                curr["variation_id"] = curr["id"];


                ///////////////////////////////
                // handle attributes mapping
                if (((JArray)curr["attributes"]).Count == 0)
                {
                    // handle in a way that will not cause an exception if we iterate over something that is not a JArray.
                }

                var newAttrs = new JObject();
                foreach (var attr in (JArray)curr["attributes"])
                {
                    string name = ((string)attr["name"]).ToLower();
                    string value = ((string)attr["option"]).ToLower();

                    newAttrs[$"attribute_pa_{name}"] = value;
                }
                curr["attributes"] = newAttrs;
            }

            return vars;
        }

        private string[] GetProductIds(JObject jObject)
        {
            JArray lineItems = (JArray)jObject.SelectToken("line_items");

            if (lineItems == null)
                return new string[] { };  // Return empty array if "line_items" is not found

            return lineItems.Select(item => item["product_id"].ToString()).ToArray();
        }

        private JArray SetDestinationProperty(JArray X, JArray Y, string destination)
        {
            // Check that X and Y have the same length
            if (X.Count != Y.Count)
            {
                throw new Exception("The arrays X and Y do not have the same length.");
            }

            // Iterate over the items in X
            for (int i = 0; i < X.Count; i++)
            {
                // Get the current JObject
                JObject itemX = (JObject)X[i];

                // Set the destination property to the value of the corresponding item in Y
                itemX[destination] = Y[i];
            }
            return X;
        }

        private JObject PutToDataAndItems(JObject input)
        {
            JObject output = new JObject();
            JObject data = new JObject();
            JArray items = new JArray();

            foreach (var property in input.Properties())
            {
                if (property.Name == "line_items")
                {
                    items = (JArray)property.Value;
                }
                else
                {
                    data.Add(property.Name, property.Value);
                }
            }

            output["data"] = data;

            if (items.HasValues)
            {
                output["items"] = items;
            }

            return output;
        }

        private void CorrectDate(JToken token)
        {
            if (token is JObject jsonObject)
            {
                CorrectDateSub(jsonObject);
            }
            else if (token is JArray jsonArray)
            {
                foreach (var child in jsonArray)
                {
                    CorrectDate(child);
                }
            }
        }

        private void CorrectDateSub(JObject jsonObject)
        {
            Dictionary<string, JObject> newVals = new Dictionary<string, JObject>();

            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.StartsWith("date"))
                {

                    if (property.Value.Type == JTokenType.Null)
                        continue;


                    DateTime date = DateTime.MinValue;

                    if (property.Value.Type == JTokenType.Object)
                    {
                        JToken dateToken;
                        if (((JObject)property.Value).TryGetValue("date", out dateToken))
                        {
                            date =ParseOnlyExactDateTimeFormats(dateToken.ToString());
                        }
                        else
                        {
                            throw new Exception("Json datetime parsing failed!");
                        }
                    }
                    else if (property.Value != null && property.Value.ToString() == "")
                    {
                        date = DateTime.MinValue;
                    }
                    else
                    {
                        try {
                            date = ParseOnlyExactDateTimeFormats(property.Value.ToString());
                        } catch (Exception ex) {
                            throw new Exception($"Unable to parse the value '{property.Value}' from property '{property.Name}' as a valid date. Please ensure the input is in a recognizable date format like: 'yyyy-MM-dd HH:mm:ss' or 'dd.MM.yyyy HH:mm:ss'.");
                        }
                    }


                    JObject newdate = new JObject();
                    newdate["timezone_type"] = 1;
                    newdate["timezone"] = "+00:00";
                    newdate["date"] = date.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
                    newVals[property.Name] = newdate;
                }
                else
                {
                    // If not, process the property value
                    CorrectDate(property.Value);
                }
            }

            // Replace marked properties with empty JObject
            foreach (var property in newVals)
            {
                jsonObject[property.Key] = property.Value;
            }
        }

        private DateTime ParseOnlyExactDateTimeFormats(string dateToBeParsed)
        {
            string[] acceptedFormats = new string[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "dd.MM.yyyy HH:mm:ss",
                "d.MM.yyyy HH:mm:ss",
                "d.M.yyyy HH:mm:ss",
                "dd.M.yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm:ss"
            };

            DateTime parsedDate = DateTime.MinValue;
            bool didParse = false;

            if (long.TryParse(dateToBeParsed, out long unixTime))
            {
                parsedDate = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
                didParse = true;
                Console.WriteLine(parsedDate);
            }
            else
            {
                try
                {
                    var parsed = DateTime.Parse(dateToBeParsed);
                    parsedDate = new DateTime(
                        parsed.Year,
                        parsed.Month,
                        parsed.Day,
                        parsed.Hour,
                        parsed.Minute,
                        parsed.Second);
                    didParse = true;
                    Console.WriteLine(parsedDate);
                }
                catch { }
            }

            if (!didParse)
            {
                var formats = acceptedFormats.Aggregate(new StringBuilder(), (sb, next) => sb.AppendLine(next)).ToString();
                throw new Exception($"DateTime not in correct format! Received DateTime: {dateToBeParsed}{Environment.NewLine}Expected formats: {Environment.NewLine}{formats}");
            }

            return parsedDate;
        }

        private string TrimAfterSeconds(string dateTime)
        {
            // Reverse the string
            char[] charArray = dateTime.ToCharArray();
            Array.Reverse(charArray);
            string reversedDateTime = new string(charArray);

            // Find the first non-alphanumeric character
            int dotIndex = -1;
            for (int i = 0; i < reversedDateTime.Length; i++)
            {
                if (reversedDateTime[i] == '.')
                {
                    dotIndex = i;
                    break;
                }
                else if (!char.IsLetterOrDigit(reversedDateTime[i]) && reversedDateTime[i] != ' ' && reversedDateTime[i] != ':' && reversedDateTime[i] != '/')
                {
                    break;
                }
            }

            // If a dot was found, trim everything after the dot
            if (dotIndex != -1)
            {
                reversedDateTime = reversedDateTime.Substring(dotIndex + 1);
            }

            // Reverse the string back to the original order
            charArray = reversedDateTime.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }




        #endregion
    }
}
