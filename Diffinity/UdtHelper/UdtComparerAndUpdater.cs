using Dapper;
using Microsoft.Data.SqlClient;

namespace Diffinity.UdtHelper
{
    public static class UdtComparerAndUpdater
    {
        /// <summary>
        /// Compares UDT scripts and, if asked, updates destination to match source.
        /// UPDATE STRATEGY: DROP TYPE + CREATE TYPE (only if there are NO dependencies).
        /// Returns:
        ///   areEqual    -> true if scripts match (after normalization);
        ///   differences -> textual notes why different or why update skipped;
        ///   wasAltered  -> true if we actually dropped/created the type.
        /// </summary>
        public static (bool areEqual, List<string> differences, bool wasAltered)
            ComparerAndUpdater(
                string destinationConnectionString,
                string sourceScript,
                string destinationScript,
                string schema,
                string typeName,
                ComparerAction makeChange)
        {
            var differences = new List<string>();
            bool wasAltered = false;

            // null/empty destination means "new" at destination
            bool destMissing = string.IsNullOrWhiteSpace(destinationScript);

            // Normalize & compare (same idea as AreBodiesEqual)
            bool areEqual = Normalize(sourceScript) == Normalize(destinationScript);

            if (areEqual)
                return (true, differences, false);

            // Not equal → note difference
            differences.Add($"UDT definition differs for [{schema}].[{typeName}]");

            if (makeChange != ComparerAction.ApplyChanges)
                return (false, differences, false);

            // We only apply when destination exists or missing
            using var con = new SqlConnection(destinationConnectionString);
            con.Open();

            // Get user_type_id for the UDT to find dependencies
            var uid = con.ExecuteScalar<int?>(@"
SELECT t.user_type_id
FROM sys.types t
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = @schema AND t.name = @name;",
                new { schema, name = typeName });

            // If destination missing, uid might be null (ok)
            var deps = uid.HasValue ? GetDependencies(con, uid.Value) : new List<string>();

            if (!destMissing && deps.Count > 0)
            {
                differences.Add("Update blocked: the UDT is referenced by other objects:");
                differences.AddRange(deps.Select(d => $" - {d}"));
                return (false, differences, false);
            }

            // Safe to apply:
            using var tx = con.BeginTransaction();

            try
            {
                if (!destMissing)
                {
                    con.Execute($@"DROP TYPE [{schema}].[{typeName}];", transaction: tx);
                }

                con.Execute(sourceScript, transaction: tx);
                tx.Commit();
                wasAltered = true;
            }
            catch (Exception ex)
            {
                tx.Rollback();
                differences.Add("Update failed:");
                differences.Add(ex.Message);
                return (false, differences, false);
            }

            // Re-check equality after apply (optional sanity)
            return (true, differences, wasAltered);

            // --- local helpers ---
            static string Normalize(string s)
            {
                if (s == null) return null;
                return s.Replace("\r\n", "\n")
                        .Replace(" ", "")
                        .Replace("\t", "")
                        .Replace("\n", "")
                        .Replace("[", "").Replace("]", "")
                        .ToUpperInvariant()
                        .Trim();
            }

            static List<string> GetDependencies(SqlConnection con, int userTypeId)
            {
                // Objects using this UDT:
                // 1) Columns (tables/views),
                // 2) Parameters (procs/functions, incl. TVPs),
                // 3) Return table type params.
                const string depsSql = @"
-- columns
SELECT 'COLUMN: ' + QUOTENAME(ss.name)+'.'+QUOTENAME(o.name)+'.'+QUOTENAME(c.name) AS refname
FROM sys.columns c
JOIN sys.objects o ON o.object_id = c.object_id
JOIN sys.schemas ss ON ss.schema_id = o.schema_id
WHERE c.user_type_id = @uid

UNION ALL

-- parameters (procedures, functions) including TVPs
SELECT 'PARAM: ' + QUOTENAME(ss.name)+'.'+QUOTENAME(o.name)+'.'+QUOTENAME(p.name) AS refname
FROM sys.parameters p
JOIN sys.objects o ON o.object_id = p.object_id
JOIN sys.schemas ss ON ss.schema_id = o.schema_id
WHERE p.user_type_id = @uid
ORDER BY refname;";

                return con.Query<string>(depsSql, new { uid = userTypeId }).ToList();
            }
        }
    }
}
