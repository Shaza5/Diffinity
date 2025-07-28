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

        var sw = new Stopwatch();
        sw.Start();
        string procIndexPath = DbComparer.CompareProcs(
            new DbServer(SourceDatabase, SourceConnectionString)
            , new DbServer(DestinationDatabase, DestinationConnectionString)
            , OutputFolder
            , ComparerAction.DoNotApplyChanges
            , DbObjectFilter.HideUnchanged
        );
       string viewIndexPath = DbComparer.CompareViews(
           new DbServer(SourceDatabase, SourceConnectionString)
           , new DbServer(DestinationDatabase, DestinationConnectionString)
           , OutputFolder
           , ComparerAction.DoNotApplyChanges
           , DbObjectFilter.HideUnchanged
       );
        string tableIndexpath = DbComparer.CompareTables(
         new DbServer(SourceDatabase, SourceConnectionString)
         , new DbServer(DestinationDatabase, DestinationConnectionString)
         , OutputFolder
         , ComparerAction.DoNotApplyChanges
         , DbObjectFilter.HideUnchanged
     );
        HtmlReportWriter.WriteIndexSummary(SourceConnectionString,DestinationConnectionString,OutputFolder, procIndexPath, viewIndexPath, tableIndexpath);
        sw.Stop();
        Console.WriteLine($"Elapsed time: {sw} ms");
    }
}