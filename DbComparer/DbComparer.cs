using DbComparer.HtmlHelper;
using DbComparer.ProcHelper;

namespace DbComparer;
public enum ComparerAction
{
    ApplyChanges,
    DoNotApplyChanges
}
public record SqlServer(string name, string connectionString);
public class DbComparer
{
    public static void CompareProcs(SqlServer sourceServer, SqlServer destinationServer, string outputFolder, ComparerAction makeChange)
    {
        Directory.CreateDirectory(outputFolder);

        //List<string> procedures = new() { "temporary.test1", "temporary.test2" };
        List<string> procedures = ProcedureFetcher.GetProcedureNames(sourceServer.connectionString);

        List<ProcedureResult> results = new();

        foreach (var proc in procedures)
        {
            string safeName = MakeSafe(proc);

            (string sourceBody, string destinationBody) = ProcedureFetcher.GetProcedureBody(sourceServer.connectionString, destinationServer.connectionString, proc);
            bool areEqual = ProcedureComparer.AreBodiesEqual(sourceBody, destinationBody);

            string sourceFile = $"{safeName}_Source.html";
            string destinationFile = $"{safeName}_Destination.html";
            string newFile = $"{safeName}_New.html";

            string sourcePath = Path.Combine(outputFolder, sourceFile);
            string destinationPath = Path.Combine(outputFolder, destinationFile);

            string returnPage = "SummaryReport.html";

            HtmlReportWriter.WriteProcedureBodyHtml(sourcePath, $"{sourceServer.name} Procedure Body", sourceBody, returnPage);
            HtmlReportWriter.WriteProcedureBodyHtml(destinationPath, $"{destinationServer.name} Procedure Body", destinationBody, returnPage);

            string destinationNewBody = destinationBody;
            bool wasAltered = false;

            if (!areEqual && makeChange == ComparerAction.ApplyChanges)
            {
                ProcedureUpdater.AlterProcedure(destinationServer.connectionString, sourceBody);
                (_, destinationNewBody) = ProcedureFetcher.GetProcedureBody(sourceServer.connectionString,destinationServer.connectionString, proc);
                string newPath = Path.Combine(outputFolder, newFile);
                HtmlReportWriter.WriteProcedureBodyHtml(newPath, $"New {destinationServer.name} Procedure Body", destinationNewBody, returnPage);
                wasAltered = true;
            }

            results.Add(new ProcedureResult
            {
                Name = proc,
                IsEqual = areEqual,
                SourceFile = sourceFile,
                DestinationFile = destinationFile,
                NewFile = wasAltered ? newFile : null
            });
        }

        HtmlReportWriter.WriteSummaryReport(sourceServer, destinationServer, Path.Combine(outputFolder, "SummaryReport.html"), results);
    }
    private static string MakeSafe(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
        {
            name = name.Replace(c, '_');
        }
        return name;
    }
}

public class ProcedureResult
{
    public string Name { get; set; }
    public bool IsEqual { get; set; }
    public string SourceFile { get; set; }
    public string DestinationFile { get; set; }
    public string? NewFile { get; set; } // null if not altered
}