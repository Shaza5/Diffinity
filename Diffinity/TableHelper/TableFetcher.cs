using Dapper;
using Diffinity.DbHelper;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Diffinity.TableHelper;
public class TableFetcher
{
    private const string GetTablesNamesQuery = @"
            SELECT s.name AS SchemaName, t.name AS TableName
            FROM sys.tables t
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            ORDER BY s.name, t.name;
        ";
    private const string GetTableInfoQuery = @"
    SELECT 
        c.name AS columnName,
        ty.name AS columnType,
        CASE WHEN c.is_nullable = 1 THEN 'YES' ELSE 'NO' END AS isNullable,
        CASE 
            WHEN ty.name IN ('varchar','nvarchar','char','nchar') THEN c.max_length
            ELSE NULL
        END AS maxLength,
        CASE 
            WHEN pk.column_id IS NOT NULL THEN 'YES' ELSE 'NO' 
        END AS isPrimaryKey,
        CASE 
            WHEN fk.parent_column_id IS NOT NULL THEN 'YES' ELSE 'NO' 
        END AS isForeignKey
    FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
    JOIN sys.schemas s ON t.schema_id = s.schema_id
    JOIN sys.types ty ON c.user_type_id = ty.user_type_id
    LEFT JOIN (
        SELECT i.object_id, ic.column_id
        FROM sys.indexes i
        JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
        WHERE i.is_primary_key = 1
    ) pk ON c.object_id = pk.object_id AND c.column_id = pk.column_id
    LEFT JOIN (
        SELECT fkc.parent_object_id AS object_id, fkc.parent_column_id AS parent_column_id
        FROM sys.foreign_key_columns fkc
    ) fk ON c.object_id = fk.object_id AND c.column_id = fk.parent_column_id
    WHERE t.name = @tableName
      AND s.name = @schemaName
    ORDER BY c.column_id;
";
    /// <summary>
    /// Retrieves the names of all tables from the source database.
    /// </summary>
    /// <param name="sourceConnectionString"></param>
    /// <returns></returns>
    public static List<(string schema, string name)> GetTablesNames(string sourceConnectionString)
    {
        using var sourceConnection = new SqlConnection(sourceConnectionString);
        var list = sourceConnection.Query<(string schema, string name)>(GetTablesNamesQuery).AsList();
        return list;
    }

    /// <summary>
    /// Returns the info of a table from both source and destination databases.
    /// </summary>
    /// <param name="fullTableName"></param>
    /// <returns></returns>
    public static (List<tableDto> sourceTableColumns,List<tableDto> destinationTableColumns) GetTableInfo(string sourceConnectionString, string destinationConnectionString, string schema, string TableName)
    {
        using SqlConnection sourceConnection = new SqlConnection(sourceConnectionString);
        using SqlConnection destinationConnection = new SqlConnection(destinationConnectionString);
        var sourceInfo = sourceConnection.Query<tableDto>(GetTableInfoQuery, new { tableName = TableName, schemaName = schema }).ToList();
        var destinationInfo = destinationConnection.Query<tableDto>(GetTableInfoQuery, new { tableName = TableName, schemaName = schema }).ToList();
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
