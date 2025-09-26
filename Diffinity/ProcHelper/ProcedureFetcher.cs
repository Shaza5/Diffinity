using Dapper;
using Microsoft.Data.SqlClient;
using Diffinity.DbHelper;


namespace Diffinity.ProcHelper;
public static class ProcedureFetcher
{
    private const string GetProceduresNamesQuery = @"
            SELECT s.name AS SchemaName, p.name AS ProcName
            FROM sys.procedures p
            JOIN sys.schemas s ON p.schema_id = s.schema_id
            ORDER BY s.name, p.name;
        ";

    private const string GetProcedureBodyQuery = @"
            SELECT sm.definition
            FROM sys.procedures p
            JOIN sys.schemas s ON p.schema_id = s.schema_id
            JOIN sys.sql_modules sm ON p.object_id = sm.object_id
            WHERE p.name = @procName
              AND s.name = @schemaName;
        ";
    /// <summary>
    /// Retrieves the names of all stored procedures from the source database.
    /// </summary>
    /// <param name="sourceConnectionString"></param>
    /// <returns></returns>
    public static List<(string schema, string name)> GetProcedureNames(string sourceConnectionString)
    {
        using var sourceConnection = new SqlConnection(sourceConnectionString);
        var list = sourceConnection.Query<(string schema, string name)>(GetProceduresNamesQuery).AsList();
        return list;
    }


    /// <summary>
    /// Returns the body of a stored procedure from both source and destination databases.
    /// </summary>
    /// <param name="procedureName"></param>
    /// <returns></returns>
    public static (string sourceBody, string destinationBody) GetProcedureBody(string sourceConnectionString, string destinationConnectionString,string schema, string procedureName)
    {
        using SqlConnection sourceConnection      = new SqlConnection(sourceConnectionString);
        using SqlConnection destinationConnection = new SqlConnection(destinationConnectionString);

        string sourceBody      = sourceConnection.QueryFirst<string>(GetProcedureBodyQuery, new { procName = procedureName, schemaName = schema });
        string destinationBody = destinationConnection.QueryFirstOrDefault<string>(GetProcedureBodyQuery, new { procName = procedureName, schemaName = schema }) ?? "";
        return (sourceBody, destinationBody);
    }
}