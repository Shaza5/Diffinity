internal static class SqliteTestQueries
{
    public const string GetProceduresNames = "SELECT name FROM Procedures;";
    public const string GetProcedureBody = "SELECT body FROM Procedures WHERE name = @procName;";

    public const string GetViewsNames = "SELECT name FROM Views;";
    public const string GetViewBody = "SELECT body FROM Views WHERE name = @viewName;";

    public const string GetTablesNames = "SELECT DISTINCT FullName FROM TableInfo;";
    public const string GetTableInfo = "SELECT * FROM TableInfo WHERE FullName = @FullName;";
}
