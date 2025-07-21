using Dapper;
using Microsoft.Data.SqlClient;

namespace DbComparer.TableHelper;
public class TableFetcher
{
    /// <summary>
    /// Retrieves the names of all tables from the source database.
    /// </summary>
    /// <param name="sourceConnectionString"></param>
    /// <returns></returns>
    public static List<string> GetTablesNames(string sourceConnectionString)
    {
        using var sourceConnection = new SqlConnection(sourceConnectionString);
        var list = sourceConnection.Query<string>("tools.spGetTablesNames").AsList();
        return list;
    }
    public static (List<infoDto> sourceTableColumns,List<infoDto> destinationTableColumns) GetTableInfo(string sourceConnectionString, string destinationConnectionString, string fullTableName)
    {
        using SqlConnection sourceConnection = new SqlConnection(sourceConnectionString);
        using SqlConnection destinationConnection = new SqlConnection(destinationConnectionString);
        var sourceInfo = sourceConnection.Query<infoDto>("tools.spGetTableInfo", new { FullName = fullTableName }).ToList();
        var destinationInfo = sourceConnection.Query<infoDto>("tools.spGetTableInfo", new { FullName = fullTableName }).ToList();
        return (sourceInfo, destinationInfo);

    }
}
public class infoDto
{
    public string columnName { get; set; }
    public string columnType { get; set; }
    public string isNullable { get; set; }
    public int maxLength { get; set; }
    public string isPrimaryKey { get; set; }
    public string isForeignKey { get; set; }
}
