using common_ops.diagnostics.Constants;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace common_ops.Executors.Sql
{
    internal class WrappedSqlReader
    {
        private readonly StringBuilder _sb = new StringBuilder();

        internal async Task<List<string>> ReadAllReaderRows(SqlDataReader reader)
        {
            var values = new List<string>();
            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    _sb.Clear();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var value = CheckForNullOrEmpty(reader[i]);
                        if (i < reader.FieldCount - 1)
                            _sb.Append($"{value}{TextConstants.DELIMITER}");
                        else
                            _sb.Append($"{value}");
                    }
                    values.Add(_sb.ToString());
                }
            }
            return values;
        }

        private string CheckForNullOrEmpty(object entry)
        {
            var result = entry?.ToString()?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(result))
                return TextConstants.NULL_FIELD;
            return result;
        }
    }
}
