namespace DbComparer;
public class Program
{
    private const string OutputFolder = @"C:\Users\Nada\Documents\database";
    static readonly string SourceConnectionString = Environment.GetEnvironmentVariable("CorewellCs");
    static readonly string DestinationConnectionString = Environment.GetEnvironmentVariable("CmhCs");
    public static void Main(string[] args)
    {
        DbComparer.CompareProcs(new("Corewell", SourceConnectionString), new("CMH", DestinationConnectionString), OutputFolder, ComparerAction.DoNotApplyChanges,MatchesProcs.Hide);
    }
}