using DbComparer.HtmlHelper;
using Microsoft.Data.SqlClient;
using Serilog;
using System.Diagnostics;

namespace DbComparer;
public class Program  
{
    private const string OutputFolder = @"DbComparer-output";
    static readonly string SourceDatabase = "Corewell";
    static readonly string DestinationDatabase = "CMH";
    static readonly string SourceConnectionString = Environment.GetEnvironmentVariable("CorewellCs");
    static readonly string DestinationConnectionString = Environment.GetEnvironmentVariable("CmhCs");
    public static void Main(string[] args)
    { 
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        #region environment variable validation and database connection checks at startup
        // Check if required environment variables are set
        if (string.IsNullOrWhiteSpace(SourceConnectionString) || string.IsNullOrWhiteSpace(DestinationConnectionString))
        {
            Console.Error.WriteLine("Error: One or both required environment variables (CorewellCs, CmhCs) are missing.");
            return;
        }

        // Attempt to connect to the source database
        try
        {
            using var sourceConn = new SqlConnection(SourceConnectionString);
            sourceConn.Open();
            Console.WriteLine("Connected to source database.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to connect to source database: " + ex.Message);
            return;
        }

        // Attempt to connect to the destination database
        try
        {
            using var destConn = new SqlConnection(DestinationConnectionString);
            destConn.Open();
            Console.WriteLine("Connected to destination database.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Failed to connect to destination database: " + ex.Message);
            return;
        }
        #endregion

        var sw = new Stopwatch();
        sw.Start();
        string procIndexPath = DbComparer.CompareProcs(
            new DbServer(SourceDatabase, SourceConnectionString)
            , new DbServer(DestinationDatabase, DestinationConnectionString)
            , OutputFolder
            , ComparerAction.DoNotApplyChanges  // Set to ApplyChanges to update the destination DB
            , DbObjectFilter.HideUnchanged      // Set to ShowUnchangedProcs for a full report
        );
       string viewIndexPath = DbComparer.CompareViews(
           new DbServer(SourceDatabase, SourceConnectionString)
           , new DbServer(DestinationDatabase, DestinationConnectionString)
           , OutputFolder
           , ComparerAction.DoNotApplyChanges  // Set to ApplyChanges to update the destination DB
           , DbObjectFilter.HideUnchanged      // Set to ShowUnchangedProcs for a full report
       );
        string tableIndexpath = DbComparer.CompareTables(
         new DbServer(SourceDatabase, SourceConnectionString)
         , new DbServer(DestinationDatabase, DestinationConnectionString)
         , OutputFolder
         , ComparerAction.DoNotApplyChanges   // Set to ApplyChanges to update the destination DB
         , DbObjectFilter.HideUnchanged       // Set to ShowUnchangedProcs for a full report
     );
        HtmlReportWriter.WriteIndexSummary(SourceConnectionString,DestinationConnectionString,OutputFolder, procIndexPath, viewIndexPath, tableIndexpath);
        sw.Stop();
        Console.WriteLine($"Elapsed time: {sw} ms");
    }
}