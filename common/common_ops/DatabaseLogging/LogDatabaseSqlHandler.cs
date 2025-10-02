using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace common_ops.DatabaseLogging
{
    public struct SAttribute
    {
        public string Name;
        public string Type;
    }

    internal class LogDatabaseSqlHandler
    {
        private const string TABLE_NAME = "Logs";
        private readonly DatabaseName _databaseName;
        private readonly SAttribute[] _attributes;
        private readonly string _attributeNames;

        internal LogDatabaseSqlHandler(DatabaseName databaseName, params SAttribute[] attributes)
        {
            _databaseName = databaseName;
            _attributes = attributes;
            _attributeNames = _attributes.Aggregate(new StringBuilder(), (sb, next) => sb.Append($"{next.Name}, ")).ToString();
            _attributeNames = TrimAndEnsureNoTrailingComma(_attributeNames);
        }

        internal string CreateDatabaseQuery()
        {
            return $@"
                IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = '{_databaseName.Name}')
                BEGIN
                    CREATE DATABASE {_databaseName.Name};
                END";
        }

        internal string CreateTableQuery()
        {
            var insert = _attributes.Aggregate(new StringBuilder(), (sb, next) => sb.Append($"{next.Name} {next.Type}, ")).ToString();
            insert = TrimAndEnsureNoTrailingComma(insert);

            return $@"
                USE {_databaseName.Name};
                IF NOT EXISTS(SELECT * FROM sys.tables WHERE name = '{TABLE_NAME}')
                BEGIN
                    CREATE TABLE {TABLE_NAME}
                    (
                       {insert}
                    );
                END";
        }

        internal string GetInsertLogQuery(params string[] data)
        {
            if (_attributes.Length != data.Length)
                throw new ArgumentException("Columns and data must have same length.");

            var values = data.Aggregate(new StringBuilder(), (sb, next) =>
            {
                if (int.TryParse(next, NumberStyles.Integer, CultureInfo.InvariantCulture, out var pint))
                    return sb.Append($"{pint}, ");

                if (double.TryParse(next, NumberStyles.Float, CultureInfo.InvariantCulture, out var pdob))
                    return sb.Append($"{pdob.ToString(CultureInfo.InvariantCulture)}, ");

                if (DateTime.TryParse(next, CultureInfo.CurrentCulture, DateTimeStyles.None, out var pdate))
                    return sb.Append($"'{pdate.ToString("yyyy-MM-dd HH:mm:ss.fff")}', ");

                // fallback = Unicode string
                return sb.Append($"N'{next.Replace("'", "''")}', ");

            }).ToString();

            values = TrimAndEnsureNoTrailingComma(values);


            return $@"
                 INSERT INTO {_databaseName.Name}.dbo.{TABLE_NAME} ({_attributeNames})
                 VALUES ({values});";
        }

        public string GetSqlFriendlyDateTimeNow()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); // SQL-friendly format
        }

        private string TrimAndEnsureNoTrailingComma(string text)
        {
            text = text.Trim();
            if (text.EndsWith(","))
                text = text.Substring(0, text.Length - 1);

            return text;
        }
    }
}
