using Dapper;
using Diffinity.DbHelper;
using Microsoft.Data.SqlClient;


namespace Diffinity.ViewHelper;
public static class ViewFetcher
{
    /// <summary>
    /// Retrieves the names of all views from the source database.
    /// </summary>
    /// <param name="sourceConnectionString"></param>
    /// <returns></returns>
    public static List<string> GetViewsNames(string sourceConnectionString)
    {
        using var sourceConnection = new SqlConnection(sourceConnectionString);
        var list = sourceConnection.Query<string>(Queries.GetViewsNames).AsList();
        return list;
    }
    
    
    /// <summary>
    /// Returns the body of a view from both source and destination databases.
    /// </summary>
    /// <param name="viewName"></param>
    /// <returns></returns>
    public static (string sourceBody, string destinationBody) GetViewBody(string sourceConnectionString, string destinationConnectionString, string viewName)
    {
        using SqlConnection sourceConnection      = new SqlConnection(sourceConnectionString);
        using SqlConnection destinationConnection = new SqlConnection(destinationConnectionString);

        string sourceBody      = sourceConnection.QueryFirst<string>(Queries.GetViewBody,new { viewName = viewName });
        string destinationBody = destinationConnection.QueryFirst<string>(Queries.GetViewBody, new { viewName = viewName });
        return (sourceBody, destinationBody);
    }
}