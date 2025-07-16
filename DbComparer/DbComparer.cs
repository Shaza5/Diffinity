using DbComparer.HtmlHelper;
using DbComparer.ProcHelper;
using DbComparer.ViewHelper;
using Microsoft.Identity.Client;

namespace DbComparer;
public enum ComparerAction
{
    ApplyChanges,
    DoNotApplyChanges
}
public enum DbObjectFilter
{
    ShowUnchangedProcs,
    HideUnchangedProcs
}
public record DbServer(string name, string connectionString);
public class DbComparer : DbObjectHandler
{
    public static void CompareProcs(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter)
    {
        Directory.CreateDirectory(outputFolder);
        string proceduresFolderPath = Path.Combine(outputFolder, "Procedures");
        Directory.CreateDirectory(proceduresFolderPath);


        //List<string> procedures = new() { "temporary.test1", "temporary.test2" , "adminapp.spAvgRequestsCompletedPerHourPerConcierge" };
        List<string> procedures = ProcedureFetcher.GetProcedureNames(sourceServer.connectionString);

        List<dbObjectResult> results = new();

        Serilog.Log.Information("Procs:");
        foreach (var proc in procedures)
        {
            string[] parts = proc.Split('.');
            string schema = parts.Length > 1 ? parts[0] : "dbo"; // fallback if no dot
            string safeSchema = MakeSafe(schema);
            string safeName = MakeSafe(proc);

            string schemaFolder = Path.Combine(proceduresFolderPath, safeSchema);

            (string sourceBody, string destinationBody) = ProcedureFetcher.GetProcedureBody(sourceServer.connectionString, destinationServer.connectionString, proc);
            bool areEqual = AreBodiesEqual(sourceBody, destinationBody);
            string change = areEqual ? "No changes" : "Changes detected";
            Serilog.Log.Information($"{proc}: {change}");
            string sourceFile = $"{safeName}_{sourceServer.name}.html";
            string destinationFile = $"{safeName}_{destinationServer.name}.html";
            string newFile = $"{safeName}_New.html";

            string returnPage = Path.Combine("..", "index.html");

            bool isVisible = false;
            if ((areEqual && filter == DbObjectFilter.ShowUnchangedProcs) || !areEqual)
            {
                Directory.CreateDirectory(schemaFolder);
                string sourcePath = Path.Combine(schemaFolder, sourceFile);
                string destinationPath = Path.Combine(schemaFolder, destinationFile);
                HtmlReportWriter.WriteBodyHtml(sourcePath, $"{sourceServer.name} Procedure Body", sourceBody, returnPage);
                HtmlReportWriter.WriteBodyHtml(destinationPath, $"{destinationServer.name} Procedure Body", destinationBody, returnPage);
                isVisible = true;
            }
            string destinationNewBody = destinationBody;
            bool wasAltered = false;

            if (!areEqual && makeChange == ComparerAction.ApplyChanges)
            {
                AlterDbObject(destinationServer.connectionString, sourceBody);
                (_, destinationNewBody) = ProcedureFetcher.GetProcedureBody(sourceServer.connectionString, destinationServer.connectionString, proc);
                string newPath = Path.Combine(schemaFolder, newFile);
                HtmlReportWriter.WriteBodyHtml(newPath, $"New {destinationServer.name} Procedure Body", destinationNewBody, returnPage);
                wasAltered = true;
            }

            results.Add(new dbObjectResult
            {
                Type="Proc",
                Name = proc,
                IsEqual = areEqual,
                SourceFile = isVisible ? Path.Combine(safeSchema, sourceFile) : null,
                DestinationFile = isVisible ? Path.Combine(safeSchema, destinationFile) : null,
                NewFile = wasAltered ? Path.Combine(safeSchema, newFile) : null
            });
        }

        HtmlReportWriter.WriteSummaryReport(sourceServer, destinationServer, Path.Combine(proceduresFolderPath, "index.html"), results, filter);
    }
    public static void CompareViews(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter)
    {
        Directory.CreateDirectory(outputFolder);
        string viewsFolderPath = Path.Combine(outputFolder, "Views");
        Directory.CreateDirectory(viewsFolderPath);

        //List<string> views = new() { "ccc.vwCommandL1_old","ccc.vwCopyEdits","ccc.vwRequests "};
        List<string> views = ViewFetcher.GetViewsNames(sourceServer.connectionString);

        List<dbObjectResult> results = new();

        Serilog.Log.Information("Views:");
        foreach (var view in views)
        {
            string[] parts = view.Split('.');
            string schema = parts.Length > 1 ? parts[0] : "dbo"; // fallback if no dot
            string safeSchema = MakeSafe(schema);
            string safeName = MakeSafe(view);

            string schemaFolder = Path.Combine(viewsFolderPath, safeSchema);

            (string sourceBody, string destinationBody) = ViewFetcher.GetViewBody(sourceServer.connectionString, destinationServer.connectionString, view);
            bool areEqual = AreBodiesEqual(sourceBody, destinationBody);
            string change = areEqual ? "No changes" : "Changes detected";
            Serilog.Log.Information($"{view}: {change}");
            string sourceFile = $"{safeName}_{sourceServer.name}.html";
            string destinationFile = $"{safeName}_{destinationServer.name}.html";
            string newFile = $"{safeName}_New.html";

            string returnPage = Path.Combine("..", "index.html");

            bool isVisible = false;
            if ((areEqual && filter == DbObjectFilter.ShowUnchangedProcs) || !areEqual)
            {
                Directory.CreateDirectory(schemaFolder);
                string sourcePath = Path.Combine(schemaFolder, sourceFile);
                string destinationPath = Path.Combine(schemaFolder, destinationFile);
                HtmlReportWriter.WriteBodyHtml(sourcePath, $"{sourceServer.name} View Body", sourceBody, returnPage);
                HtmlReportWriter.WriteBodyHtml(destinationPath, $"{destinationServer.name} View Body", destinationBody, returnPage);
                isVisible = true;
            }
            string destinationNewBody = destinationBody;
            bool wasAltered = false;

            if (!areEqual && makeChange == ComparerAction.ApplyChanges)
            {
                AlterDbObject(destinationServer.connectionString, sourceBody);
                (_, destinationNewBody) = ViewFetcher.GetViewBody(sourceServer.connectionString, destinationServer.connectionString, view);
                string newPath = Path.Combine(schemaFolder, newFile);
                HtmlReportWriter.WriteBodyHtml(newPath, $"New {destinationServer.name} View Body", destinationNewBody, returnPage);
                wasAltered = true;
            }

            results.Add(new dbObjectResult
            {
                Type = "View",
                Name = view,
                IsEqual = areEqual,
                SourceFile = isVisible ? Path.Combine(safeSchema, sourceFile) : null,
                DestinationFile = isVisible ? Path.Combine(safeSchema, destinationFile) : null,
                NewFile = wasAltered ? Path.Combine(safeSchema, newFile) : null
            });
        }

        HtmlReportWriter.WriteSummaryReport(sourceServer, destinationServer, Path.Combine(viewsFolderPath, "index.html"), results, filter);
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

