using Microsoft.Data.SqlClient;

namespace DbComparer.DatabaseHelper;
public static class DatabaseConnections
{
    private static string CorewellCs =
        Environment.GetEnvironmentVariable("CorewellCs");
    private static string CmhCs =
        Environment.GetEnvironmentVariable("CmhCs");
    public static SqlConnection GetCorewellConnection()
    {
        var connection = new SqlConnection(CorewellCs);
        connection.Open();
        return connection;
    }
    public static SqlConnection GetCmhConnection()
    {
        var connection = new SqlConnection(CmhCs);
        connection.Open();
        return connection;
    }
}
