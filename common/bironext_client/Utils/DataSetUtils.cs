using ExcelDataReader;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Birokrat.Next.ApiClient.Utils
{
    public static class DataSetUtils
    {
        public static DataSet CreateFromExcel(byte[] data, string name)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var stream = new MemoryStream(data);
            var reader = ExcelReaderFactory.CreateReader(stream);

            var dataSet = reader.AsDataSet();
            dataSet.DataSetName = name;

            foreach (DataTable table in dataSet.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    foreach (DataColumn column in table.Columns)
                    {
                        var value = row[column] == DBNull.Value ? string.Empty : Convert.ToString(row[column]);
                        row[column] = ApplyTextCorrections(value);
                    }
                }
            }

            return dataSet;
        }

        public static DataSet CreateFromJson(JToken data, string key)
        {
            var dataSet = new DataSet(dataSetName: key);

            var table = dataSet.Tables.Add("Sheet1");
            var rows = data.Value<JArray>("rows");
            if (rows.Count > 0)
            {
                var columns = rows[0].Value<JArray>("columns");
                var columnCollection = Enumerable
                    .Range(0, columns.Count)
                    .Select(i => new DataColumn($"Column{i}"))
                    .ToArray();
                table.Columns.AddRange(columnCollection);
            }

            foreach (var row in rows)
            {
                DataRow newRow = table.NewRow();

                int i = 0;
                foreach (var column in row["columns"])
                {
                    newRow[$"Column{i}"] = column.Value<string>("value");
                    i++;
                }

                table.Rows.Add(newRow);
            }

            dataSet.AcceptChanges();

            return dataSet;
        }

        private static string ApplyTextCorrections(string text) =>
            text
            .Replace("{{S}}", "Š")
            .Replace("{{s}}", "š")
            .Replace("{{C}}", "Č")
            .Replace("{{c}}", "č")
            .Replace("{{Z}}", "Ž")
            .Replace("{{z}}", "ž")
            .Replace("\n", " ");
    }
}
