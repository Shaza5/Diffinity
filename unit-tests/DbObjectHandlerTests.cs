using Dapper;
using Diffinity;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

public class DbObjectHandlerTests
{
    #region AreBodiesEqual Tests
    public void AreBodiesEqual_ShouldReturnTrue_ForEquivalentBodies()
    {
        string body1 = "CREATE TABLE Test(Id INT);";
        string body2 = "CREATE TABLE Test(Id INT);";

        bool result = DbObjectHandler.AreBodiesEqual(body1, body2);

        Assert.True(result);
    }

    [Fact]
    public void AreBodiesEqual_ShouldReturnFalse_ForDifferentBodies()
    {
        string body1 = "CREATE TABLE Test(Id INT);";
        string body2 = "CREATE TABLE Test(Id INT, Name TEXT);";

        bool result = DbObjectHandler.AreBodiesEqual(body1, body2);

        Assert.False(result);
    }

    [Fact]
    public void AreBodiesEqual_ShouldReturnTrue_IgnoresWhitespaceAndBrackets()
    {
        string body1 = "CREATE PROCEDURE [dbo].[TestProc] AS SELECT 1;";
        string body2 = "Create PROCEDURE dbo.TestProc AS SELECT 1;";

        bool result = DbObjectHandler.AreBodiesEqual(body1, body2);

        Assert.True(result);
    }
    #endregion

    #region AlterDbObject tests
    [Fact]
    public void AlterDbObject_CreatesObject_WhenDestinationEmpty()
    {
        using var db = new TestDatabase();
        string createProc = "CREATE PROCEDURE dbo.TestProc AS SELECT 1;";

        // Destination body empty → should execute CREATE
        DbObjectHandler.AlterDbObject(db.ConnectionString, createProc, "");

        // Verify procedure exists
        using var testDb = new SqlConnection(db.ConnectionString);
        var procExists = testDb.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sys.procedures WHERE name = 'TestProc'");
        Assert.Equal(1, procExists);
    }

    [Fact]
    public void AlterDbObject_AltersObject_WhenDestinationExists()
    {
        using var db = new TestDatabase();
        string createProc = "CREATE PROCEDURE dbo.TestProc AS SELECT 1;";

        // Create first
        DbObjectHandler.AlterDbObject(db.ConnectionString, createProc, "");

        string alteredProc = "CREATE PROCEDURE dbo.TestProc AS SELECT 2;";
        DbObjectHandler.AlterDbObject(db.ConnectionString, alteredProc, createProc);

        // Verify the procedure body changed
        using var testDb = new SqlConnection(db.ConnectionString);
        string body = testDb.ExecuteScalar<string>(
            "SELECT OBJECT_DEFINITION(OBJECT_ID('dbo.TestProc'))");
        Assert.Contains("SELECT 2", body);
    }

    [Fact]
    public void AlterDbObject_Throws_WhenSourceEmpty()
    {
        using var db = new TestDatabase();

        Assert.Throws<ArgumentException>(() =>
            DbObjectHandler.AlterDbObject(db.ConnectionString, "", ""));
    }
    #endregion
}
