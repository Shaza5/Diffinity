using Serilog;
using System.Diagnostics;

namespace DbComparer;
public class Program  
{
    private const string OutputFolder = @"DbComparer-output";
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
        DbComparer.CompareProcs(
            new DbServer("Corewell", SourceConnectionString)
            , new DbServer("CMH", DestinationConnectionString)
            , OutputFolder
            , ComparerAction.DoNotApplyChanges
            , DbObjectFilter.HideUnchangedProcs
        );
        DbComparer.CompareViews(
           new DbServer("Corewell", SourceConnectionString)
           , new DbServer("CMH", DestinationConnectionString)
           , OutputFolder
           , ComparerAction.DoNotApplyChanges
           , DbObjectFilter.ShowUnchangedProcs
       );
        DbComparer.CompareTables(
         new DbServer("Corewell", SourceConnectionString)
         , new DbServer("CMH", DestinationConnectionString)
         , OutputFolder
         , ComparerAction.DoNotApplyChanges
         , DbObjectFilter.ShowUnchangedProcs
     );
        sw.Stop();
        Console.WriteLine($"Elapsed time: {sw} ms");
    }
}