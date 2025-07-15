using System.Text;

namespace DbComparer.HtmlHelper;

public static class HtmlReportWriter
{
    private const string Value = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Procedure Comparison Summary</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            max-width: 1000px;
            margin: 40px auto;
            padding: 20px;
            background-color: #fff;
            color: #333;
        }
        h1 {
            color: #EC317F;
            text-align: center;
            margin-bottom: 40px;
        }
        table {
            width: 100%;
            border-collapse: collapse;
        }
        th, td {
            padding: 12px 16px;
            border-bottom: 1px solid #ddd;
            text-align: left;
        }
        th {
            background-color: #f5f5f5;
        }
        .match {
            color: green;
            font-weight: 600;
        }
        .diff {
            color: red;
            font-weight: 600;
        }
        a {
            color: #EC317F;
            text-decoration: none;
            font-weight: 600;
        }
        a:hover {
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <h1>Procedure Comparison Summary</h1>
    <table>
        <tr>
            <th></th>
            <th>Procedure Name</th>
            <th>Status</th>
            <th>{source} Original</th>
            <th>{destination} Original</th>
            <th>{destination} New</th>
        </tr>
    ";

    #region Summary Report Writer
    public static void WriteSummaryReport(SqlServer sourceServer, SqlServer destinationServer, string summaryPath, List<ProcedureResult> procedures)
    {
        StringBuilder html = new();
        html.Append(Value.Replace("{source}", sourceServer.name).Replace("{destination}", destinationServer.name));

        int procNumber = 1;
        foreach (var proc in procedures)
        {
            string sourceColumn = proc.SourceFile != null
                ? $@"<a href=""{proc.SourceFile}"">View</a>"
                : "—";
            string destinationColumn = proc.DestinationFile != null
                ? $@"<a href=""{proc.DestinationFile}"">View</a>"
                : "—";
            string newColumn = proc.NewFile != null
                ? $@"<a href=""{proc.NewFile}"">View</a>"
                : "—";
            html.Append($@"
        <tr>
            <td>{procNumber}</td>
            <td>{proc.Name}</td>
            <td>{(proc.IsEqual ? "<span class='match'>Match</span>" : "<span class='diff'>Different</span>")}</td>
            <td>{sourceColumn}</td>
            <td>{destinationColumn}</td>
            <td>{newColumn}</td>
        </tr>");
            procNumber++;
        }
        html.Append(@"
    </table>
</body>
</html>");
        File.WriteAllText(summaryPath, html.ToString());
    }
    #endregion

    #region Individual Procedure Body Writer
    public static void WriteProcedureBodyHtml(string filePath, string title, string body, string returnPage)
    {
        string escapedBody = EscapeHtml(body);
        string content = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>{title}</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 40px auto;
            max-width: 900px;
            background-color: #fff;
            color: #222;
            padding: 20px 40px;
            border-radius: 10px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.07);
        }}
        h1 {{
            color: #EC317F;
            text-align: center;
            margin-bottom: 40px;
        }}
        pre {{
            background-color: #f9f9f9;
            padding: 20px;
            border-radius: 8px;
            overflow-x: auto;
            font-size: 1rem;
            line-height: 1.5;
            border: 1px solid #ddd;
            white-space: pre-wrap;
            word-wrap: break-word;
            color: #222;
            margin-bottom: 40px;
        }}
        .return-btn {{
            display: block;
            width: 220px;
            margin: 0 auto;
            padding: 12px 0;
            background-color: #EC317F;
            color: white;
            font-weight: 600;
            text-align: center;
            text-decoration: none;
            border-radius: 6px;
            box-shadow: 0 3px 8px rgba(236, 49, 127, 0.25);
            transition: background-color 0.3s ease;
            font-size: 1rem;
        }}
        .return-btn:hover {{
            background-color: #b42a68;
        }}
    </style>
</head>
<body>
    <h1>{title}</h1>
    <pre>{escapedBody}</pre>
    <a href=""{returnPage}"" class=""return-btn"">Return to Summary</a>
</body>
</html>";
        File.WriteAllText(filePath, content);
    }
    #endregion

    #region Helpers
    private static string EscapeHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
    }
    #endregion
}