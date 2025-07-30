using Dapper;
using Microsoft.Data.SqlClient;
using System.Text;

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

    /// <summary>
    /// Returns the info of a table from both source and destination databases.
    /// </summary>
    /// <param name="fullTableName"></param>
    /// <returns></returns>
    public static (List<tableDto> sourceTableColumns,List<tableDto> destinationTableColumns) GetTableInfo(string sourceConnectionString, string destinationConnectionString, string fullTableName)
    {
        using SqlConnection sourceConnection = new SqlConnection(sourceConnectionString);
        using SqlConnection destinationConnection = new SqlConnection(destinationConnectionString);
        var sourceInfo = sourceConnection.Query<tableDto>("tools.spGetTableInfo", new { FullName = fullTableName }).ToList();
        var destinationInfo = sourceConnection.Query<tableDto>("tools.spGetTableInfo", new { FullName = fullTableName }).ToList();
        return (sourceInfo, destinationInfo);

    }
}
public class tableDto
{
    public string columnName { get; set; }
    public string columnType { get; set; }
    public string isNullable { get; set; }
    public string maxLength { get; set; }
    public string isPrimaryKey { get; set; }
    public string isForeignKey { get; set; }
}
