using DbComparer.TableHelper;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Reflection;
using System.Text;
using static DbComparer.DbObjectHandler;



namespace DbComparer.HtmlHelper;

public static class HtmlReportWriter
{
    private const string IndexTemplate = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Database Comparison Index</title>
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
        ul {
            list-style-type: none;
            padding: 0;
        }
        li {
            margin: 20px 0;
            font-size: 1.2em;
        }
        a {
            color: #EC317F;
            text-decoration: none;
            font-weight: 600;
        }
        a:hover {
            text-decoration: underline;
        }
      .btn {
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
        }
        .btn:hover {
            background-color: #b42a68;
        }
    </style>
</head>
<body>
    <h1>Database Comparison Summary</h1>
    <ul>
        <li>{procsIndex}</li>
        <li>{viewsIndex}</li>
        <li>{tablesIndex}</li>
    </ul>
</body>
</html>
";
    private const string ComparisonTemplate = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>{MetaData} Comparison Summary</title>
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
  .return-btn {
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
        }
        .return-btn:hover {
            background-color: #b42a68;
        }
    </style>
</head>
<body>
    <h1>{MetaData} Comparison Summary</h1>
    {NewTable}
    <table>
        <tr>
            <th></th>
            <th>{MetaData} Name</th>
            <th>Status</th>
            <th>{source} Original</th>
            <th>{destination} Original</th>
            {differences}
            <th>{destination} New</th>
        </tr>
    ";
    private const string BodyTemplate = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>{title}</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 40px auto;
            max-width: 900px;
            background-color: #fff;
            color: #222;
            padding: 20px 40px;
            border-radius: 10px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.07);
        }
        h1 {
            color: #EC317F;
            text-align: center;
            margin-bottom: 40px;
        }
        div {
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
        }
        table {width: 100%;
            border-collapse: collapse;
            margin-bottom: 40px;
            font-size: 1rem;
            border: 1px solid #ddd;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 3px 8px rgba(0, 0, 0, 0.05);
        }
       
        th, td {padding: 12px 16px;
            text-align: left;
            border-bottom: 1px solid #eee;
        }
        
        th {background - color: #EC317F;
            color: #EC317F;
            font-weight: 600;
            text-transform: uppercase;
            font-size: 0.9rem;
        }
        
        tr:nth-child(even) {background - color: #f9f9f9;
        }
        
        tr:hover {background - color: #f1f1f1;
        }
        
        td {color: #222;
        }
        .return-btn {
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
        }
        .red {color: red;
    }

        .return-btn:hover {
            background-color: #b42a68;
        }
    .source { color: green; } 
    .destination { color: red; }
    </style>
  </head>";
    private const string DifferencesTemplate = @"<!DOCTYPE html>
       <html>
       <head>
       <meta charset='utf-8' />
       <title>{title}</title>
       <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 40px auto;
            max-width: 1600px;
            background-color: #fff;
            color: #222;
            padding: 20px 40px;
            border-radius: 10px;
            box-shadow: 0 4px 10px rgba(0,0,0,0.07);
        }
        h1 {
            color: #EC317F;
            text-align: center;
            margin-bottom: 40px;
        }
        .diff-wrapper {
            display: flex;
            gap: 20px;
        }
        .pane {
            width: 48%;
            background-color: #f9f9f9;
            padding: 0;
            border-radius: 8px;
            overflow: auto;
            height: 650px;
            border: 1px solid #ddd;
            box-shadow: 0 3px 8px rgba(0, 0, 0, 0.05);
        }
        .pane h2 {
            margin: 0;
            padding: 12px 16px;
            background-color: #f0f0f0;
            color: black;
            border-radius: 8px 8px 0 0;
            text-align: center;
            font-size: 1rem;
        }
        .code-block {
            display: grid;
            grid-template-columns: 50px 1fr;
            font-size: 0.95rem;
            line-height: 1.4;
            padding: 10px;
        }
        .line-number {
            color: #999;
            text-align: right;
            padding-right: 10px;
            user-select: none;
        }
        .line-text {
            font-family: Consolas;
            white-space: pre;
            overflow-wrap: break-word;
        }
       .return-btn {
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
        } 
       .return-btn:hover {
            background-color: #b42a68;
        }
        .inserted { background-color: #c6f6c6; }
        .deleted { background-color: #f6c6c6; }
        .imaginary { background-color: #eee; color: #999; }
        .modified { background-color: #fff3b0; }
        </style>
        </head>";

    #region Index Report Writer
    public static void WriteIndexSummary(string outputPath, string procIndexPath, string viewIndexPath, string tableIndexPath)
    {
        StringBuilder html = new StringBuilder();
        string procsIndex = $@"<a href=""{procIndexPath}"" class=""btn"">Procedures</a>";
        string viewsIndex = $@"<a href=""{viewIndexPath}"" class=""btn"">Views</a>";
        string tablesIndex = $@"<a href=""{tableIndexPath}"" class=""btn"">Tables</a>";
        html.Append(IndexTemplate.Replace("{procsIndex}", procsIndex).Replace("{viewsIndex}", viewsIndex).Replace("{tablesIndex}", tablesIndex));
        string indexPath = Path.Combine(outputPath, "index.html");
        File.WriteAllText(indexPath, html.ToString());
    }
    #endregion

    #region Summary Report Writer
    public static void WriteSummaryReport(DbServer sourceServer, DbServer destinationServer, string summaryPath, List<dbObjectResult> results, DbObjectFilter filter)
    {
        StringBuilder html = new();
        var result = results[0];
        string returnPage = Path.Combine("..", "index.html");
        html.Append(ComparisonTemplate.Replace("{source}", sourceServer.name).Replace("{destination}", destinationServer.name).Replace("{MetaData}", result.Type));

        #region 1-Create the new table
        var newProcedures = results.Where(r => r.IsDestinationEmpty).ToList();
        if (newProcedures.Any())
        {
            StringBuilder newTable = new StringBuilder();
            newTable.AppendLine($@"<h2 style=""color: #B42A68;"">New {result.Type}s in {sourceServer.name} Database </h2>
            <table>
                <tr>
                    <th></th>
                    <th>{result.Type} Name</th>
                    <th></th>
                </tr>");

            int newCount = 1;
            foreach (var item in newProcedures)
            {
                string sourceLink = item.SourceFile != null ? $@"<a href=""{item.SourceFile}"">View</a" : "—";
                newTable.Append($@"<tr>
                                <td>{newCount}</td>
                                <td>{item.Name}</td>
                                <td>{sourceLink}</td>
                                </tr>");
                newCount++;
            }

            newTable.Append("</table><br><br>");
            html.Replace("{NewTable}", newTable.ToString());
        }
        else
        {
            html.Replace("{NewTable}", "");
        }
        #endregion

        #region 2-Create the Comparison Table
        int Number = 1;
        foreach (var item in results)
        {
            if (item.IsDestinationEmpty) continue;
            string sourceColumn = item.SourceFile != null ? $@"<a href=""{item.SourceFile}"">View</a>" : "—";
            string destinationColumn = item.DestinationFile != null ? $@"<a href=""{item.DestinationFile}"">View</a>" : "—";
            string differencesColumn = null;
            if (item.DifferencesFile != null)
            {
                html.Replace("{differences}", "<th>Differences</th>");
                differencesColumn = $@"<td><a href=""{item.DifferencesFile}"">View</a></td>";
            }
            else
            {
                html.Replace("{differences}", "");
            }
            ;
            string newColumn = item.NewFile != null ? $@"<a href=""{item.NewFile}"">View</a>" : "—";

            if ((item.IsEqual && filter == DbObjectFilter.ShowUnchanged) || !item.IsEqual)
            {
                html.Append($@"<tr>
                    <td>{Number}</td>
                    <td>{item.Name}</td>
                    <td>{(item.IsEqual ? "<span class='match'>Match</span>" : "<span class='diff'>Different</span>")}</td>
                    <td>{sourceColumn}</td>
                    <td>{destinationColumn}</td>
                    {differencesColumn}
                    <td>{newColumn}</td>
                     </tr>");
                Number++;
            }
        }
        html.Append($@"</table>
                       <br>
                       <a href=""{returnPage}"" class=""return-btn"">Return to Index</a>
                       </body>
                       </html>");
        #endregion

        File.WriteAllText(summaryPath, html.ToString());
    }
    #endregion

    #region Individual Procedure Body Writer
    public static void WriteBodyHtml(string filePath, string title, string body, string returnPage)
    {
        StringBuilder html = new StringBuilder();
        html.AppendLine(BodyTemplate.Replace("{title}", title));
        string escapedBody = EscapeHtml(body);
        if (title.Contains("Table"))
        {
            escapedBody = body;
        }
        html.AppendLine($@"<body>
            <h1>{title}</h1>
            <div>{escapedBody}</div>
            <a href=""{returnPage}"" class=""return-btn"">Return to Summary</a>
            </body>
            </html>");
        File.WriteAllText(filePath, html.ToString());
    }
    #endregion

    #region Differences Writer
    public static void DifferencesWriter(string differencesPath, string sourceName, string destinationName, string sourceBody, string destinationBody, string title, string Name, string returnPage)
    {
        var differ = new Differ();
        var sideBySideBuilder = new SideBySideDiffBuilder(differ);
        var model = sideBySideBuilder.BuildDiffModel(destinationBody, sourceBody);

        var html = new StringBuilder();
        html.AppendLine(DifferencesTemplate.Replace("{title}", title));
        html.AppendLine(@$"<body>
                        <h1>{Name}</h1>
                        <div class='diff-wrapper'>
                        <div class='pane'><h2>{destinationName}</h2><div class='code-block'>");
        foreach (var line in model.OldText.Lines)
        {
            string css = GetCssClass(line.Type);
            string lineNumber = line.Position == 0 ? "" : line.Position.ToString();
            html.AppendLine(@$"<div class='line-number'>{lineNumber}</div>
                           <div class='line-text {css}'>{System.Net.WebUtility.HtmlEncode(line.Text)}</div>");
        }
        html.AppendLine(@$"</div></div>
                        <div class='pane'><h2>{sourceName}</h2><div class='code-block'>");
        foreach (var line in model.NewText.Lines)
        {
            string css = GetCssClass(line.Type);
            string lineNumber = line.Position == 0 ? "" : line.Position.ToString();
            html.AppendLine(@$"<div class='line-number'>{lineNumber}</div>
                            <div class='line-text {css}'>{System.Net.WebUtility.HtmlEncode(line.Text)}</div>");
        }
        html.AppendLine(@"</div></div></div>
                        </body>
                        </html>");
        File.WriteAllText(differencesPath, html.ToString());
    }
    #endregion

    #region Helpers
    static string EscapeHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
    }
    static string GetCssClass(ChangeType type)
    {
        return type switch
        {
            ChangeType.Inserted => "inserted",
            ChangeType.Deleted => "deleted",
            ChangeType.Imaginary => "imaginary",
            ChangeType.Modified => "modified",
            _ => ""
        };
    }
    public static string PrintTableInfo(List<tableDto> tableInfo, List<string>? differences)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"<table border='1'>
       <tr>
       <th>Column Name</th>
       <th>Column Type</th>
       <th>Is Nullable</th>
       <th>Max Length</th>
       <th>Is Primary Key</th>
       <th>Is Foreign Key</th>
       </tr>");

        foreach (tableDto column in tableInfo)
        {
            string ColumnName = $"<td>{column.columnName}</td>";
            if (differences.Contains(column.columnName))
            {
                ColumnName = $@"<td class=""red"">{column.columnName}</td>";
            }
            string ColumnType = $"<td>{column.columnType}</td>";
            if (differences.Contains(column.columnType))
            {
                ColumnType = $@" <td class=""red"">{column.columnType}</td>";
            }
            string isNullable = $"<td>{column.isNullable}</td>";
            if (differences.Contains(column.isNullable))
            {
                isNullable = $@"<td class=""red"">{column.isNullable}</td>";
            }
            string maxLength = $"<td>{column.maxLength}</td>";
            if (differences.Contains(column.maxLength))
            {
                maxLength = $@"<td class=""red"">{column.maxLength}</td>";
            }
            string isPrimaryKey = $"<td>{column.isPrimaryKey}</td>";
            if (differences.Contains(column.isPrimaryKey))
            {
                isPrimaryKey = $@"<td class=""red"">{column.isPrimaryKey}</td>";
            }
            string isForeignKey = $"<td>{column.isForeignKey}</td>";
            if (differences.Contains(column.isForeignKey))
            {
                isForeignKey = $@"<td class=""red"">{column.isForeignKey}</td>";
            }
            sb.AppendLine($@"<tr>
            {ColumnName}
            {ColumnType}
            {isNullable}
            {maxLength}
            {isPrimaryKey}
            {isForeignKey}
            </tr>");
        }

        sb.AppendLine("</table>");
        return sb.ToString();

    }
    #endregion

}