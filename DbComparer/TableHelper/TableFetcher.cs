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
    public static string PrintTableInfo(List<tableDto> tableInfo)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"<table border='1'>
       <tr>
       <th>Column Name</th>
       <th>Column Type</th>
       <th>Is Nullable</th>
       <th>Max Length</th>
       <th>Is Primary Key</th>
       <th>Is Foreign Key</th>
       </tr>");

        foreach (tableDto column in tableInfo)
        {
            sb.AppendLine($@"<tr>
            <td>{column.columnName}</td>
            <td>{column.columnType}</td>
            <td>{column.isNullable}</td>
            <td>{column.maxLength}</td>
            <td>{column.isPrimaryKey}</td>
            <td>{column.isForeignKey}</td>
            </tr>");
        }

        sb.AppendLine("</table>");
        return sb.ToString();
    }
}
public class tableDto
{
    public string columnName { get; set; }
    public string columnType { get; set; }
    public string isNullable { get; set; }
    public int maxLength { get; set; }
    public string isPrimaryKey { get; set; }
    public string isForeignKey { get; set; }
}
