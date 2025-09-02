using Dapper;
using Diffinity.TableHelper;


public class SqliteFetcherTests : IDisposable
{
    private readonly SqliteHelper _helper;

    public SqliteFetcherTests()
    {
        _helper = new SqliteHelper();
        _helper.SetupProceduresTable();
        _helper.SetupViewsTable();
        _helper.SetupTablesTable();
    }

    #region Procedure Fetcher Tests
    [Fact]
    public void ProcedureFetcher_GetProcedureNames_ReturnsData()
    {
        var list = _helper._db.Query<string>(SqliteTestQueries.GetProceduresNames).AsList();
        Assert.Single(list);
        Assert.Contains("MyProc", list);
    }

    [Fact]
    public void ProcedureFetcher_GetProcedureBody_ReturnsData()
    {
        var body = _helper._db.QueryFirst<string>(SqliteTestQueries.GetProcedureBody, new { procName = "MyProc" });
        Assert.Equal("SELECT 1;", body);
    }
    #endregion

    #region View Fetcher Tests
    [Fact]
    public void ViewFetcher_GetViewsNames_ReturnsData()
    {
        var list = _helper._db.Query<string>(SqliteTestQueries.GetViewsNames).AsList();
        Assert.Single(list);
        Assert.Contains("MyView", list);
    }

    [Fact]
    public void ViewFetcher_GetViewBody_ReturnsData()
    {
        var body = _helper._db.QueryFirst<string>(SqliteTestQueries.GetViewBody, new { viewName = "MyView" });
        Assert.Equal("SELECT 2;", body);
    }
    #endregion

    #region Tables Fetcher Tests
    [Fact]
    public void TableFetcher_GetTableNames_ReturnsData()
    {
        var list = _helper._db.Query<string>(SqliteTestQueries.GetTablesNames).AsList();
        Assert.Single(list);
        Assert.Contains("MyTable", list);
    }

    [Fact]
    public void TableFetcher_GetTableInfo_ReturnsColumns()
    {
        var columns = _helper._db.Query<tableDto>(SqliteTestQueries.GetTableInfo, new { FullName = "MyTable" }).ToList();
        Assert.Equal(2, columns.Count);
        Assert.Contains(columns, c => c.columnName == "Id");
        Assert.Contains(columns, c => c.columnName == "Name");
    }
    #endregion

    public void Dispose()
    {
        _helper.Dispose();
    }
}
