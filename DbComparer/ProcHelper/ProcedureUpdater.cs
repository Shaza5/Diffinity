using Dapper;
using DbComparer.DatabaseHelper;

namespace DbComparer.ProcHelper;
public static class ProcedureUpdater
{
    public static void AlterProcedure(string procBody)
    {
        if (string.IsNullOrWhiteSpace(procBody))
            throw new ArgumentException("Procedure body cannot be null or empty.", nameof(procBody));

        string alteredBody = ReplaceCreateWithAlter(procBody);
        DatabaseConnections.GetCmhConnection().Execute(alteredBody, commandType: System.Data.CommandType.Text);
    }
    private static string ReplaceCreateWithAlter(string body)
    {
        string alteredBody=body.Replace("CREATE", "ALTER").Replace("Create", "ALTER").Replace("create", "ALTER");
        return alteredBody;
    }
}