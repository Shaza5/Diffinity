using Dapper;
using Microsoft.Data.SqlClient;

public class TestDatabase : IDisposable
{
    private const string MasterConnectionString = @"Server=(localdb)\mssqllocaldb;Integrated Security=true;";
    public string ConnectionString { get; }
    private readonly string _dbName;

    public TestDatabase()
    {
        _dbName = "TestDb_" + Guid.NewGuid().ToString("N");
        using var master = new SqlConnection(MasterConnectionString);
        master.Open();
        master.Execute($"CREATE DATABASE [{_dbName}];");

        ConnectionString = $"{MasterConnectionString}Initial Catalog={_dbName};";
    }

    public void Dispose()
    {
        using var master = new SqlConnection(MasterConnectionString);
        master.Open();
        master.Execute($@"
            ALTER DATABASE [{_dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [{_dbName}];
        ");
    }
}
