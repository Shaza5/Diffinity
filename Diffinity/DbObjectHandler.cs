using Dapper;
using Diffinity.TableHelper;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace Diffinity;

public class DbObjectHandler
{
    public static bool AreBodiesEqual(string body1, string body2)
    {
        /// <summary>
        /// Compares two database object definitions (bodies) for equality.
        /// It normalizes the strings by removing whitespace, brackets, and standardizing keywords,
        /// then computes a SHA-256 hash to determine if the bodies are effectively the same.
        /// </summary>
        /// <param name="body1">First database object definition (e.g., procedure, view, table).</param>
        /// <param name="body2">Second database object definition to compare against.</param>
        /// <returns>True if the normalized bodies are equal; otherwise, false.</returns>
        // Compute hash of both normalized bodies and compare them
        string hash1 = ComputeHash(body1);
        string hash2 = ComputeHash(body2);

        return hash1 == hash2;

        #region local functions
        /// <summary>
        /// Normalizes the input string and computes its SHA-256 hash as a hex string.
        /// Normalization includes removing whitespace, brackets, and converting keywords to uppercase.
        /// </summary>
        /// <param name="input">Input string to hash.</param>
        /// <returns>Hexadecimal string representation of SHA-256 hash.</returns>
        string ComputeHash(string input)
        {
            if (input == null) return null;

            // Normalize the input for consistent comparison
            string normalized = input.Replace("create", "CREATE")
                                   .Replace("Create", "CREATE")
                                   .Replace("\r\n", "\n")
                                   .Replace(" ", "")
                                   .Replace("\t", "")
                                   .Replace("\n", "")
                                   .Replace("[", "")
                                   .Replace("]","")
                                   .Trim();

            // Convert normalized string to bytes
            byte[] inputBytes = Encoding.UTF8.GetBytes(normalized);

            // Compute SHA-256 hash
            using var sha = SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(inputBytes);

            // Convert hash bytes to hex string
            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
        #endregion
    }

    public static void AlterDbObject(string destinationConnectionString, string sourceBody,string destinationBody)
    {
        /// <summary>
        /// Alters a database object on the destination server based on the source object body.
        /// If the destination body is empty, it executes the source body as a CREATE statement.
        /// Otherwise, it modifies the source body to use ALTER statements and executes it.
        /// </summary>
        /// <param name="destinationConnectionString">Connection string for the destination database.</param>
        /// <param name="sourceBody">The source database object definition (CREATE statement).</param>
        /// <param name="destinationBody">The existing destination object definition (may be empty).</param>
        
        if (string.IsNullOrWhiteSpace(sourceBody)) throw new ArgumentException("Source body cannot be null or empty.");
        using var sourceConnection = new SqlConnection(destinationConnectionString);

        // If the destination object does not exist, create it
        if (string.IsNullOrWhiteSpace(destinationBody))
        {
            sourceConnection.Execute(sourceBody);
        }
        else
        {
            // Replace CREATE keywords with ALTER to update existing object
            string alteredBody = ReplaceCreateWithAlter(sourceBody);
            sourceConnection.Execute(alteredBody);
        }

        #region local functions
        /// <summary>
        /// Replaces all occurrences of 'CREATE' (any case) with 'ALTER' to convert a CREATE statement into an ALTER statement.
        /// </summary>
        /// <param name="body">SQL statement body to modify.</param>
        /// <returns>Modified SQL statement with ALTER keywords.</returns>
        string ReplaceCreateWithAlter(string body) => body.Replace("CREATE", "ALTER").Replace("Create", "ALTER").Replace("create", "ALTER");
        #endregion
    }
    
    public class dbObjectResult
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsDestinationEmpty { get; set; }
        public bool IsEqual { get; set; }
        public string SourceFile { get; set; }
        public string DestinationFile { get; set; }
        public string? DifferencesFile { get; set; }
        public string? SourceBody { get; set; }
        public List<tableDto> SourceTableInfo { get; set; }
        public string? NewFile { get; set; } // null if not altered
    }
 
}