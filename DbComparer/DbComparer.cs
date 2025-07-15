using DbComparer.HtmlHelper;
using DbComparer.ProcHelper;
using Microsoft.Identity.Client;

namespace DbComparer;
public enum ComparerAction
{
    ApplyChanges,
    DoNotApplyChanges
}
public enum ProcsFilter
{
    ShowUnchangedProcs,
    HideUnchangedProcs
}
public record DbServer(string name, string connectionString);
public class DbComparer
{
    public static void CompareProcs(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, ProcsFilter filter)
    {
        Directory.CreateDirectory(outputFolder);

        //List<string> procedures = new() { "temporary.test1", "temporary.test2" , "adminapp.spAvgRequestsCompletedPerHourPerConcierge" };
        List<string> procedures = ProcedureFetcher.GetProcedureNames(sourceServer.connectionString);

        List<ProcedureResult> results = new();

        foreach (var proc in procedures)
        {
            string[] parts = proc.Split('.');
            string schema = parts.Length > 1 ? parts[0] : "dbo"; // fallback if no dot
            string safeSchema = MakeSafe(schema);
            string safeName = MakeSafe(proc);

            string schemaFolder = Path.Combine(outputFolder, safeSchema);

            (string sourceBody, string destinationBody) = ProcedureFetcher.GetProcedureBody(sourceServer.connectionString, destinationServer.connectionString, proc);
            bool areEqual = ProcedureComparer.AreBodiesEqual(sourceBody, destinationBody);
            string change = areEqual ? "No changes" : "Changes detected";
            Serilog.Log.Information($"{proc}: {change}");
            string sourceFile = $"{safeName}_Source.html";
            string destinationFile = $"{safeName}_{destinationServer.name}.html";
            string newFile = $"{safeName}_New.html";

            string returnPage = Path.Combine("..", "index.html");

            bool isVisible = false;
            if ((areEqual && filter == ProcsFilter.ShowUnchangedProcs) || !areEqual)
            {
                Directory.CreateDirectory(schemaFolder);
                string sourcePath = Path.Combine(schemaFolder, sourceFile);
                string destinationPath = Path.Combine(schemaFolder, destinationFile);
                HtmlReportWriter.WriteProcedureBodyHtml(sourcePath, $"{sourceServer.name} Procedure Body", sourceBody, returnPage);
                HtmlReportWriter.WriteProcedureBodyHtml(destinationPath, $"{destinationServer.name} Procedure Body", destinationBody, returnPage);
                isVisible = true;
            }
            string destinationNewBody = destinationBody;
            bool wasAltered = false;

            if (!areEqual && makeChange == ComparerAction.ApplyChanges)
            {
                ProcedureUpdater.AlterProcedure(destinationServer.connectionString, sourceBody);
                (_, destinationNewBody) = ProcedureFetcher.GetProcedureBody(sourceServer.connectionString, destinationServer.connectionString, proc);
                string newPath = Path.Combine(schemaFolder, newFile);
                HtmlReportWriter.WriteProcedureBodyHtml(newPath, $"New {destinationServer.name} Procedure Body", destinationNewBody, returnPage);
                wasAltered = true;
            }

            results.Add(new ProcedureResult
            {
                Name = proc,
                IsEqual = areEqual,
                SourceFile = isVisible ? Path.Combine(schemaFolder, sourceFile) : null,
                DestinationFile = isVisible ? Path.Combine(schemaFolder, destinationFile) : null,
                NewFile = wasAltered ? Path.Combine(schemaFolder, newFile) : null
            });
        }

        HtmlReportWriter.WriteSummaryReport(sourceServer, destinationServer, Path.Combine(outputFolder, "index.html"), results, filter);
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