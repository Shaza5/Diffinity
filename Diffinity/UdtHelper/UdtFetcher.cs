using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Diffinity.UdtHelper
{
    public static class UdtFetcher
    {
        // Return all user-created types 
        public static List<(string schema, string name)> GetUdtNames(string connectionString)
        {
            const string sql = @"
SELECT s.name AS [schema], t.name AS [name]
FROM sys.types t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.is_user_defined = 1;
";

            using var con = new SqlConnection(connectionString);
            return con.Query<(string schema, string name)>(sql).ToList();
        }

        // Script CREATE TYPE for both source & destination
        public static (string sourceBody, string destBody) GetUdtBody(
            string sourceConn, string destConn, string schema, string name)
        {
            string src = ScriptUdt(sourceConn, schema, name);
            string dst = ScriptUdt(destConn, schema, name);
            return (src, dst);
        }

        private static string ScriptUdt(string connectionString, string schema, string name)
        {
            using var con = new SqlConnection(connectionString);

            // Is it a table type?
            const string isTableTypeSql = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1 FROM sys.table_types tt
    JOIN sys.schemas s ON s.schema_id = tt.schema_id
    WHERE s.name = @schema AND tt.name = @name
) THEN 1 ELSE 0 END AS bit);";

            bool isTableType = con.ExecuteScalar<bool>(isTableTypeSql, new { schema, name });

            if (isTableType)
                return ScriptTableType(con, schema, name);

            // Else: alias type
            const string aliasSql = @"
SELECT bt.name AS base_type,
       t.max_length,
       t.precision,
       t.scale,
       t.is_nullable
FROM sys.types t
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.types bt ON bt.user_type_id = t.system_type_id AND bt.user_type_id = bt.system_type_id
WHERE t.is_user_defined = 1 AND s.name = @schema AND t.name = @name;";

            var row = con.QuerySingleOrDefault(aliasSql, new { schema, name });
            if (row == null)
                return ""; // not found

            string baseType = row.base_type;
            short maxLen = (short)row.max_length;
            byte prec = (byte)row.precision;
            byte scale = (byte)row.scale;

            string typeWithParams = baseType.ToUpper() switch
            {
                "BINARY" or "VARBINARY" or "CHAR" or "NCHAR" or "VARCHAR" or "NVARCHAR" => $"{baseType}({LenTok(baseType, maxLen)})",
                "DECIMAL" or "NUMERIC" => $"{baseType}({prec},{scale})",
                _ => baseType
            };

            return $"CREATE TYPE [{schema}].[{name}] FROM {typeWithParams};";

            static string LenTok(string baseType, short maxLen)
            {
                if (baseType.ToUpper() is "NVARCHAR" or "NCHAR") // NVARCHAR length is in characters; sys.types stores bytes
                    maxLen = (short)(maxLen < 0 ? -1 : maxLen / 2);

                return maxLen == -1 ? "MAX" : maxLen.ToString();
            }
        }

        private static string ScriptTableType(SqlConnection con, string schema, string name)
        {
            const string colsSql = @"
SELECT c.name AS col_name,
       bt.name AS base_type,
       c.max_length,
       c.precision,
       c.scale,
       c.is_nullable
FROM sys.table_types tt
JOIN sys.schemas s ON s.schema_id = tt.schema_id
JOIN sys.columns c ON c.object_id = tt.type_table_object_id
JOIN sys.types bt ON bt.user_type_id = c.system_type_id AND bt.user_type_id = bt.system_type_id
WHERE s.name = @schema AND tt.name = @name
ORDER BY c.column_id;";

            var cols = con.Query(colsSql, new { schema, name }).ToList();
            if (cols.Count == 0)
                return ""; // not found

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TYPE [{schema}].[{name}] AS TABLE (");

            for (int i = 0; i < cols.Count; i++)
            {
                dynamic r = cols[i];
                string baseType = r.base_type;
                short maxLen = (short)r.max_length;
                byte prec = (byte)r.precision;
                byte scale = (byte)r.scale;
                bool isNullable = r.is_nullable is bool b ? b : Convert.ToInt32(r.is_nullable) != 0;

                string typeWithParams = baseType.ToUpper() switch
                {
                    "BINARY" or "VARBINARY" or "CHAR" or "NCHAR" or "VARCHAR" or "NVARCHAR" => $"{baseType}({LenTok(baseType, maxLen)})",
                    "DECIMAL" or "NUMERIC" => $"{baseType}({prec},{scale})",
                    _ => baseType
                };

                sb.Append($"    [{r.col_name}] {typeWithParams} {(isNullable ? "NULL" : "NOT NULL")}");
                sb.AppendLine(i < cols.Count - 1 ? "," : "");
            }

            sb.AppendLine(");");
            return sb.ToString();

            static string LenTok(string baseType, short maxLen)
            {
                if (baseType.ToUpper() is "NVARCHAR" or "NCHAR")
                    maxLen = (short)(maxLen < 0 ? -1 : maxLen / 2);
                return maxLen == -1 ? "MAX" : maxLen.ToString();
            }
        }
    }
}
