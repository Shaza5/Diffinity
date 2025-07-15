using DbComparer.HtmlHelper;
using DbComparer.ProcHelper;

namespace DbComparer;
public class DbComparer
{
    public static void ProcsAnalyzer(string outputFolder, bool makeChange)
    {
        Directory.CreateDirectory(outputFolder);

        List<string> procedures = new() { "temporary.test1", "temporary.test2" };
        // Or use: ProcedureFetcher.GetProcedureNames(DatabaseConnections.GetCorewellConnection());

        List<ProcedureResult> results = new();

        foreach (var proc in procedures)
        {
            string safeName = MakeSafe(proc);

            (string corewellBody, string cmhOriginal) = ProcedureFetcher.GetProcedureBody(proc);
            bool areEqual = ProcedureComparer.AreBodiesEqual(corewellBody, cmhOriginal);

            string corewellFile = $"{safeName}_Corewell.html";
            string cmhFile = $"{safeName}_CMH.html";
            string newFile = $"{safeName}_New.html";

            string corewellPath = Path.Combine(outputFolder, corewellFile);
            string cmhPath = Path.Combine(outputFolder, cmhFile);

            string returnPage = "SummaryReport.html";

            HtmlReportWriter.WriteProcedureBodyHtml(corewellPath, "Corewell Procedure Body", corewellBody, returnPage);
            HtmlReportWriter.WriteProcedureBodyHtml(cmhPath, "CMH Original Procedure Body", cmhOriginal, returnPage);

            string cmhNewBody = cmhOriginal;
            bool wasAltered = false;

            if (!areEqual && makeChange)
            {
                ProcedureUpdater.AlterProcedure(corewellBody);
                (_, cmhNewBody) = ProcedureFetcher.GetProcedureBody(proc);
                string newPath = Path.Combine(outputFolder, newFile);
                HtmlReportWriter.WriteProcedureBodyHtml(newPath, "New CMH Procedure Body", cmhNewBody, returnPage);
                wasAltered = true;
            }

            results.Add(new ProcedureResult
            {
                Name = proc,
                IsEqual = areEqual,
                CorewellFile = corewellFile,
                CmhFile = cmhFile,
                NewFile = wasAltered ? newFile : null
            });
        }

        HtmlReportWriter.WriteSummaryReport(Path.Combine(outputFolder, "SummaryReport.html"), results);
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
    public string CorewellFile { get; set; }
    public string CmhFile { get; set; }
    public string? NewFile { get; set; } // null if not altered
}