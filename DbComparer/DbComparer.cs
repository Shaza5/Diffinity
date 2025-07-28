using DbComparer.HtmlHelper;
using DbComparer.ProcHelper;
using DbComparer.TableHelper;
using DbComparer.ViewHelper;
using Microsoft.IdentityModel.Tokens;

namespace DbComparer;
public enum ComparerAction
{
    ApplyChanges,
    DoNotApplyChanges
}
public enum DbObjectFilter
{
    ShowUnchanged,
    HideUnchanged
}
public record DbServer(string name, string connectionString);
public class DbComparer : DbObjectHandler
{
    public static string CompareProcs(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter)
    {
        Directory.CreateDirectory(outputFolder);
        string proceduresFolderPath = Path.Combine(outputFolder, "Procedures");
        Directory.CreateDirectory(proceduresFolderPath);


        List<string> procedures = new() { "joelle.rePopulateCommandBags","ccc.spCreateConcierge","temporary.test1", "temporary.test2", "adminapp.spAvgRequestsCompletedPerHourPerConcierge" };
        //List<string> procedures = ProcedureFetcher.GetProcedureNames(sourceServer.connectionString);

        List<dbObjectResult> results = new();

        Serilog.Log.Information("Procs:");
        foreach (var proc in procedures)
        {
            string[] parts = proc.Split('.');
            string schema = parts.Length > 1 ? parts[0] : "dbo"; 
            string safeSchema = MakeSafe(schema);
            string safeName = MakeSafe(proc);

            string schemaFolder = Path.Combine(proceduresFolderPath, safeSchema);

            (string sourceBody, string destinationBody) = ProcedureFetcher.GetProcedureBody(sourceServer.connectionString, destinationServer.connectionString, proc);
            bool areEqual = AreBodiesEqual(sourceBody, destinationBody);
            string change = areEqual ? "No changes" : "Changes detected";
            Serilog.Log.Information($"{proc}: {change}");
            string sourceFile = $"{safeName}_{sourceServer.name}.html";
            string destinationFile = $"{safeName}_{destinationServer.name}.html";
            string differencesFile = $"{safeName}_differences.html";
            string newFile = $"{safeName}_New.html";

            string returnPage = Path.Combine("..", "index.html");

            bool isDestinationEmpty = string.IsNullOrWhiteSpace(destinationBody);
            bool isVisible = false;
            if ((areEqual && filter == DbObjectFilter.ShowUnchanged) || !areEqual)
            {
                Directory.CreateDirectory(schemaFolder);
                string sourcePath = Path.Combine(schemaFolder, sourceFile);
                string destinationPath = Path.Combine(schemaFolder, destinationFile);
                HtmlReportWriter.WriteBodyHtml(sourcePath, $"{sourceServer.name}", sourceBody, returnPage);
                HtmlReportWriter.WriteBodyHtml(destinationPath, $"{destinationServer.name}", destinationBody, returnPage);
                if (!isDestinationEmpty)
                {
                    string differencesPath = Path.Combine(schemaFolder, differencesFile);
                    HtmlReportWriter.DifferencesWriter(differencesPath, sourceServer.name, destinationServer.name, sourceBody, destinationBody, "Differences", proc, returnPage);
                }
                isVisible = true;
            }
            string destinationNewBody = destinationBody;
            bool wasAltered = false;

            if (!areEqual && makeChange == ComparerAction.ApplyChanges)
            {
                AlterDbObject(destinationServer.connectionString, sourceBody,destinationBody);
                (_, destinationNewBody) = ProcedureFetcher.GetProcedureBody(sourceServer.connectionString, destinationServer.connectionString, proc);
                string newPath = Path.Combine(schemaFolder, newFile);
                HtmlReportWriter.WriteBodyHtml(newPath, $"New {destinationServer.name}", destinationNewBody, returnPage);
                wasAltered = true;
            }

            results.Add(new dbObjectResult
            {
                Type = "Proc",
                Name = proc,
                IsDestinationEmpty = isDestinationEmpty,
                IsEqual = areEqual,
                SourceFile = isVisible ? Path.Combine(safeSchema, sourceFile) : null,
                DestinationFile = isVisible ? Path.Combine(safeSchema, destinationFile) : null,
                DifferencesFile = Path.Combine(safeSchema, differencesFile),
                NewFile = wasAltered ? Path.Combine(safeSchema, newFile) : null
            });
        }

        HtmlReportWriter.WriteSummaryReport(sourceServer, destinationServer, Path.Combine(proceduresFolderPath, "index.html"), results, filter);
        return ("Procedures/index.html");
    }
    public static string CompareViews(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter)
    {
        Directory.CreateDirectory(outputFolder);
        string viewsFolderPath = Path.Combine(outputFolder, "Views");
        Directory.CreateDirectory(viewsFolderPath);

        List<string> views = new() { "joelle.ConciergeAppAddons","ccc.vwCopyEdits", "ccc.vwRequests ", "[core].[vwUtcRequests2]" };
        //List<string> views = ViewFetcher.GetViewsNames(sourceServer.connectionString);

        List<dbObjectResult> results = new();

        Serilog.Log.Information("Views:");
        foreach (var view in views)
        {
            string[] parts = view.Split('.');
            string schema = parts.Length > 1 ? parts[0] : "dbo"; 
            string safeSchema = MakeSafe(schema);
            string safeName = MakeSafe(view);

            string schemaFolder = Path.Combine(viewsFolderPath, safeSchema);

            (string sourceBody, string destinationBody) = ViewFetcher.GetViewBody(sourceServer.connectionString, destinationServer.connectionString, view);
            bool areEqual = AreBodiesEqual(sourceBody, destinationBody);
            string change = areEqual ? "No changes" : "Changes detected";
            Serilog.Log.Information($"{view}: {change}");
            string sourceFile = $"{safeName}_{sourceServer.name}.html";
            string destinationFile = $"{safeName}_{destinationServer.name}.html";
            string differencesFile = $"{safeName}_differences.html";
            string newFile = $"{safeName}_New.html";

            string returnPage = Path.Combine("..", "index.html");

            bool isDestinationEmpty =string.IsNullOrEmpty(destinationBody);
            bool isVisible = false;
            if ((areEqual && filter == DbObjectFilter.ShowUnchanged) || !areEqual)
            {
                Directory.CreateDirectory(schemaFolder);
                string sourcePath = Path.Combine(schemaFolder, sourceFile);
                string destinationPath = Path.Combine(schemaFolder, destinationFile);
                HtmlReportWriter.WriteBodyHtml(sourcePath, $"{sourceServer.name}", sourceBody, returnPage);
                HtmlReportWriter.WriteBodyHtml(destinationPath, $"{destinationServer.name}", destinationBody, returnPage);
                if (!isDestinationEmpty)
                {
                    string differencesPath = Path.Combine(schemaFolder, differencesFile);
                    HtmlReportWriter.DifferencesWriter(differencesPath, sourceServer.name, destinationServer.name, sourceBody, destinationBody, "Differences", view, returnPage);
                }
                isVisible = true;
            }
            string destinationNewBody = destinationBody;
            bool wasAltered = false;

            if (!areEqual && makeChange == ComparerAction.ApplyChanges)
            {
                AlterDbObject(destinationServer.connectionString, sourceBody, destinationBody);
                (_, destinationNewBody) = ViewFetcher.GetViewBody(sourceServer.connectionString, destinationServer.connectionString, view);
                string newPath = Path.Combine(schemaFolder, newFile);
                HtmlReportWriter.WriteBodyHtml(newPath, $"New {destinationServer.name}", destinationNewBody, returnPage);
                wasAltered = true;
            }

            results.Add(new dbObjectResult
            {
                Type = "View",
                Name = view,
                IsDestinationEmpty = isDestinationEmpty,
                IsEqual = areEqual,
                SourceFile = isVisible ? Path.Combine(safeSchema, sourceFile) : null,
                DestinationFile = isVisible ? Path.Combine(safeSchema, destinationFile) : null,
                DifferencesFile = Path.Combine(safeSchema, differencesFile),
                NewFile = wasAltered ? Path.Combine(safeSchema, newFile) : null
            });
        }

        HtmlReportWriter.WriteSummaryReport(sourceServer, destinationServer, Path.Combine(viewsFolderPath, "index.html"), results, filter);
        return ("Views/index.html");
    }
    public static string CompareTables(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter)
    {
        Directory.CreateDirectory(outputFolder);
        string tablesFolderPath = Path.Combine(outputFolder, "Tables");
        Directory.CreateDirectory(tablesFolderPath);

        List<string> tables = new() {"dbo.App","dbo.Client"};
        //List<string> tables = TableFetcher.GetTablesNames(sourceServer.connectionString);

        List<dbObjectResult> results = new();
        bool areEqual = false;

        Serilog.Log.Information("Tables:");
        foreach (var table in tables)
        {
            string[] parts = table.Split('.');
            string schema = parts.Length > 1 ? parts[0] : "dbo";
            string safeSchema = MakeSafe(schema);
            string safeName = MakeSafe(table);
            string schemaFolder = Path.Combine(tablesFolderPath, safeSchema);
            List<string> allDifferences= new List<string>();
            (List<tableDto> sourceInfo, List<tableDto> destinationInfo) = TableFetcher.GetTableInfo(sourceServer.connectionString, destinationServer.connectionString, table);
            bool isDestinationEmpty = destinationInfo.IsNullOrEmpty();
            for (int i = 0; i < sourceInfo.Count; i++)
            {
                var tableDto = sourceInfo[i];
                (areEqual, List<string> differences) = TableComparerAndUpdater.ComparerAndUpdater(destinationServer.connectionString, sourceInfo[i], destinationInfo[i], table, makeChange);
                if (!areEqual)
                {
                    allDifferences.AddRange(differences);
                    Serilog.Log.Information($"{table}: Changes detected");
                }
            }
            if (areEqual)
            {
                Serilog.Log.Information($"{table}: No Changes");
            }

            string sourceFile = $"{safeName}_{sourceServer.name}.html";
            string destinationFile = $"{safeName}_{destinationServer.name}.html";
            string newFile = $"{safeName}_New.html";

            string returnPage = Path.Combine("..", "index.html");

            bool isVisible = false;
            if ((areEqual && filter == DbObjectFilter.ShowUnchanged) || !areEqual)
            {
                Directory.CreateDirectory(schemaFolder);
                string sourcePath = Path.Combine(schemaFolder, sourceFile);
                string destinationPath = Path.Combine(schemaFolder, destinationFile);
                HtmlReportWriter.WriteBodyHtml(sourcePath, $"{sourceServer.name} Table", HtmlReportWriter.PrintTableInfo(sourceInfo,allDifferences), returnPage);
                HtmlReportWriter.WriteBodyHtml(destinationPath, $"{destinationServer.name} Table", HtmlReportWriter.PrintTableInfo(destinationInfo,allDifferences), returnPage);
                isVisible = true;
            }
            List<tableDto> destinationNewInfo = destinationInfo;
            bool wasAltered = false;

            if (makeChange == ComparerAction.ApplyChanges)
            {
                (_, destinationNewInfo) = TableFetcher.GetTableInfo(sourceServer.connectionString, destinationServer.connectionString, table);
                string newPath = Path.Combine(schemaFolder, newFile);
                HtmlReportWriter.WriteBodyHtml(newPath, $"New {destinationServer.name} Table", HtmlReportWriter.PrintTableInfo(destinationNewInfo,null), returnPage);
                wasAltered = true;
            }

            results.Add(new dbObjectResult
            {
                Type = "Table",
                Name = table,
                IsDestinationEmpty = isDestinationEmpty,
                IsEqual = areEqual,
                SourceFile = isVisible ? Path.Combine(safeSchema, sourceFile) : null,
                DestinationFile = isVisible ? Path.Combine(safeSchema, destinationFile) : null,
                NewFile = wasAltered ? Path.Combine(safeSchema, newFile) : null
            });
        }

        HtmlReportWriter.WriteSummaryReport(sourceServer, destinationServer, Path.Combine(tablesFolderPath, "index.html"), results, filter);
        return ("Tables/index.html");
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

