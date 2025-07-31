using Dapper;
using Microsoft.Data.SqlClient;


namespace Diffinity.ProcHelper;
public static class ProcedureFetcher
{
    /// <summary>
    /// Retrieves the names of all stored procedures from the source database.
    /// </summary>
    /// <param name="sourceConnectionString"></param>
    /// <returns></returns>
    public static List<string> GetProcedureNames(string sourceConnectionString)
    {
        using var sourceConnection = new SqlConnection(sourceConnectionString);
        var list = sourceConnection.Query<string>("tools.spGetProcsNames").AsList();
        return list;
    }
    
    
    /// <summary>
    /// Returns the body of a stored procedure from both source and destination databases.
    /// </summary>
    /// <param name="procedureName"></param>
    /// <returns></returns>
    public static (string sourceBody, string destinationBody) GetProcedureBody(string sourceConnectionString, string destinationConnectionString, string procedureName)
    {
        using SqlConnection sourceConnection      = new SqlConnection(sourceConnectionString);
        using SqlConnection destinationConnection = new SqlConnection(destinationConnectionString);

        string sourceBody      = sourceConnection.QueryFirst<string>("tools.spGetProcBody",new { procName = procedureName });
        string destinationBody = destinationConnection.QueryFirst<string>("tools.spGetProcBody", new { procName = procedureName });
        return (sourceBody, destinationBody);
    }
}