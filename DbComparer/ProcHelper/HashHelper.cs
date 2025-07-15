using System.Security.Cryptography;
using System.Text;

namespace DbComparer.ProcHelper;
public static class HashHelper
{
    public static string ComputeHash(string input)
    {
        if (input == null) return null;
        string normalized=input.Replace("create","CREATE")
                               .Replace("Create","CREATE") 
                               .Replace("\r\n", "\n")
                               .Replace(" ", "")                 
                               .Replace("\t", "")                
                               .Replace("\n", "")
                               .Trim();                                      

        byte[] inputBytes = Encoding.UTF8.GetBytes(normalized);

        using var sha = SHA256.Create();
        byte[] hashBytes = sha.ComputeHash(inputBytes);

        var sb = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
            sb.Append(b.ToString("x2"));

        return sb.ToString();
    }
}
