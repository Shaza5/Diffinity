using Dapper;
using Microsoft.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace DbComparer;

public class DbObjectHandler
{
    public static bool AreBodiesEqual(string body1, string body2)
    {
        string hash1 = ComputeHash(body1);
        string hash2 = ComputeHash(body2);

        return hash1 == hash2;

        #region local functions
        string ComputeHash(string input)
        {
            if (input == null) return null;
            string normalized = input.Replace("create", "CREATE")
                                   .Replace("Create", "CREATE")
                                   .Replace("\r\n", "\n")
                                   .Replace(" ", "")
                                   .Replace("\t", "")
                                   .Replace("\n", "")
                                   .Replace("[", "")
                                   .Replace("]","")
                                   .Trim();

            byte[] inputBytes = Encoding.UTF8.GetBytes(normalized);

            using var sha = SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(inputBytes);

            var sb = new StringBuilder(hashBytes.Length * 2);
            foreach (var b in hashBytes)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
        #endregion
    }
    public static void AlterDbObject(string destinationConnectionString, string sourceBody,string destinationBody)
    {
        if (string.IsNullOrWhiteSpace(sourceBody)) throw new ArgumentException("Source body cannot be null or empty.");
        using var sourceConnection = new SqlConnection(destinationConnectionString);
        if (string.IsNullOrWhiteSpace(destinationBody))
        {
            sourceConnection.Execute(sourceBody);
        }
        else
        {
            string alteredBody = ReplaceCreateWithAlter(sourceBody);
            sourceConnection.Execute(alteredBody);
        }

        #region local functions
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
        public string? NewFile { get; set; } // null if not altered
    }
}