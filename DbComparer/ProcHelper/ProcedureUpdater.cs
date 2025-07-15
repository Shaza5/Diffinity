using Dapper;
using Microsoft.Data.SqlClient;

namespace DbComparer.ProcHelper;
public static class ProcedureUpdater
{
    public static void AlterProcedure(string destinationConnectionString, string procBody)
    {
        if (string.IsNullOrWhiteSpace(procBody)) throw new ArgumentException("Procedure body cannot be null or empty.", nameof(procBody));

        string alteredBody = ReplaceCreateWithAlter(procBody);
        using var sourceConnection = new SqlConnection(destinationConnectionString);
        sourceConnection.Execute(alteredBody);

        #region local functions
        string ReplaceCreateWithAlter(string body) => body.Replace("CREATE", "ALTER").Replace("Create", "ALTER").Replace("create", "ALTER");
        #endregion
    }
}