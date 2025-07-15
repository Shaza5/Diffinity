using Dapper;
using DbComparer.DatabaseHelper;
using Microsoft.Data.SqlClient;
using System.Data;


namespace DbComparer.ProcHelper;
public static class ProcedureFetcher
{
    public static List<string> GetProcedureNames(SqlConnection connection)
    {
        return connection.Query<string>("tools.spGetProcsNames",commandType: CommandType.StoredProcedure).AsList();
    }
    public static (string corewellBody, string cmhBody) GetProcedureBody(string procedureName)
    {
        string corewellBody = DatabaseConnections.GetCorewellConnection().QueryFirst<string>("tools.spGetProcBody",new { procName = procedureName },commandType: CommandType.StoredProcedure);
        string cmhBody = DatabaseConnections.GetCmhConnection().QueryFirst<string>("tools.spGetProcBody",new { procName = procedureName },commandType: CommandType.StoredProcedure);
        return (corewellBody, cmhBody);
    }
}