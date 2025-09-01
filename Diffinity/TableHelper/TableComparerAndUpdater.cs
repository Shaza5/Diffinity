using Dapper;
using Microsoft.Data.SqlClient;

namespace Diffinity.TableHelper;
public static class TableComparerAndUpdater
{
    /// <summary>
    /// Compares two table column definitions and optionally updates the destination schema to match the source.
    /// </summary>
    public static (bool, List<string>) ComparerAndUpdater(string destinationConnectionString, tableDto sourceTable, tableDto destinationTable, string fullTableName, ComparerAction makeChange)
    {
        if (destinationTable == null) return (false,null); 
        using SqlConnection destinationConnection = new SqlConnection(destinationConnectionString);
        destinationConnection.Open();

        List<string> differences = new();
        bool areEqual = true;

        #region 1- Column Name
        if (sourceTable.columnName != destinationTable.columnName)
        {
            differences.Add(sourceTable.columnName);
            differences.Add(destinationTable.columnName);
            areEqual = false;
            if (makeChange == ComparerAction.ApplyChanges)
            {
                string updateColumnName = $"EXEC sp_rename '{fullTableName}.{destinationTable.columnName}', '{sourceTable.columnName}', 'COLUMN'";
                destinationConnection.Execute(updateColumnName);
            }
        }
        #endregion

        #region 2- Column data type
        if (sourceTable.columnType != destinationTable.columnType)
        {
            differences.Add($"columnType: '{sourceTable.columnType}' != '{destinationTable.columnType}'");
            areEqual = false;
            if (makeChange == ComparerAction.ApplyChanges)
            {
                string updateColumnType = $@"ALTER TABLE {fullTableName} ALTER COLUMN {destinationTable.columnName} {sourceTable.columnType};";
                destinationConnection.Execute(updateColumnType);
            }
        }
        #endregion

        #region 3- Is Nullable
        if (sourceTable.isNullable != destinationTable.isNullable)
        {
            differences.Add($"isNullable: '{sourceTable.isNullable}' != '{destinationTable.isNullable}'");
            areEqual = false;
            if (makeChange == ComparerAction.ApplyChanges)
            {
                string nullability = sourceTable.isNullable == "YES" ? "NULL" : "NOT NULL";
                string updateNullability = $@"ALTER TABLE {fullTableName} ALTER COLUMN {destinationTable.columnName} {sourceTable.columnType} {nullability};";
                destinationConnection.Execute(updateNullability);
            }
        }
        #endregion

        #region 5- Max length
        if (sourceTable.maxLength != destinationTable.maxLength)
        {
            differences.Add($"maxLength: '{sourceTable.maxLength}' != '{destinationTable.maxLength}'");
            areEqual = false;
            if (makeChange == ComparerAction.ApplyChanges)
            {
                string updateLength = $@"ALTER TABLE {fullTableName} ALTER COLUMN {destinationTable.columnName} {sourceTable.columnType}({sourceTable.maxLength});";
                destinationConnection.Execute(updateLength);
            }
        }
        #endregion

        #region 6-Primary Key
        if (sourceTable.isPrimaryKey != destinationTable.isPrimaryKey)
        {
            differences.Add($"isPrimaryKey: '{sourceTable.isPrimaryKey}' != '{destinationTable.isPrimaryKey}'");
            areEqual = false;
            if (makeChange == ComparerAction.ApplyChanges)
            {
            }
        }
        #endregion

        #region 7-Foreign Key
        if (sourceTable.isForeignKey != destinationTable.isForeignKey)
        {
            differences.Add($"isForeignKey: '{sourceTable.isForeignKey}' != '{destinationTable.isForeignKey}'");
            areEqual = false;

            if (makeChange == ComparerAction.ApplyChanges)
            {
            }
        }
        #endregion
        return (areEqual, differences);
    }
}