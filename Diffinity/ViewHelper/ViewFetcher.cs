using Dapper;
using Diffinity.DbHelper;
using Microsoft.Data.SqlClient;


namespace Diffinity.ViewHelper;
public static class ViewFetcher
{
    private const string GetViewsNamesQuery = @"
            SELECT s.name AS SchemaName, v.name AS ViewName
            FROM sys.views v
            JOIN sys.schemas s ON v.schema_id = s.schema_id
            ORDER BY s.name, v.name;
        ";
    private const string GetViewBodyQuery = @"
            SELECT sm.definition
            FROM sys.views v
            JOIN sys.schemas s ON v.schema_id = s.schema_id
            JOIN sys.sql_modules sm ON v.object_id = sm.object_id
            WHERE v.name = @viewName
              AND s.name = @schemaName;
        ";
    /// <summary>
    /// Retrieves the names of all views from the source database (bracketed).
    /// </summary>
    public static List<(string schema, string name)> GetViewsNames(string sourceConnectionString)
    {
        using var sourceConnection = new SqlConnection(sourceConnectionString);
        var list = sourceConnection.Query<(string schema, string name)>(GetViewsNamesQuery).AsList();
        return list;
    }
    /// <summary>
    /// Returns the body of a view from both source and destination databases.
    /// </summary>
    public static (string sourceBody, string destinationBody) GetViewBody(string sourceConnectionString, string destinationConnectionString, string schema, string viewName)
    {
        using SqlConnection sourceConnection = new SqlConnection(sourceConnectionString);
        using SqlConnection destinationConnection = new SqlConnection(destinationConnectionString);

        string sourceBody = DbObjectHandler.ReplaceCreateWithCreateOrAlter(sourceConnection.QueryFirst<string>(GetViewBodyQuery, new { viewName = viewName, schemaName = schema }));
        string destinationBody = DbObjectHandler.ReplaceCreateWithCreateOrAlter(destinationConnection.QueryFirstOrDefault<string>(GetViewBodyQuery, new { viewName = viewName, schemaName = schema }) ?? "");
        sourceBody = DbObjectHandler.BracketObjectNameOnly(sourceBody);

        destinationBody = DbObjectHandler.BracketObjectNameOnly(destinationBody);

        return (sourceBody, destinationBody);
    }
}