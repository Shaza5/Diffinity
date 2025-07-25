using DbComparer.TableHelper;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using static DbComparer.DbObjectHandler;



namespace DbComparer.HtmlHelper;

public static class HtmlReportWriter
{
    private const string IndexPageTemplate = @"
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
    private const string ComparisonSummaryPageTemplate = @"
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
    private const string BodyPageTemplate = @"
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

    #region Summary Report Writer
    public static void WriteSummaryReport(DbServer sourceServer, DbServer destinationServer, string summaryPath, List<dbObjectResult> results, DbObjectFilter filter)
    {
        StringBuilder html = new();
        var result = results[0];
        html.Append(ComparisonSummaryPageTemplate.Replace("{source}", sourceServer.name).Replace("{destination}", destinationServer.name).Replace("{MetaData}", result.Type));
        string returnPage = Path.Combine("..", "index.html");
        int Number = 1;
        foreach (var item in results)
        {
            string sourceColumn = item.SourceFile != null
                ? $@"<a href=""{item.SourceFile}"">View</a>"
                : "—";
            string destinationColumn;
            if (!item.IsDestinationEmpty)
            {
                 destinationColumn = item.DestinationFile != null
                    ? $@"<a href=""{item.DestinationFile}"">View</a>"
                    : "—";
            }
            else
            {
                destinationColumn = "<span class='diff'>Not Found</span>";
            }
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
            string newColumn = item.NewFile != null
                ? $@"<a href=""{item.NewFile}"">View</a>"
                : "—";

            if ((item.IsEqual && filter == DbObjectFilter.ShowUnchanged) || !item.IsEqual)
            {
                html.Append($@"
        <tr>
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
        html.Append($@"
    </table>
<br>
   <a href=""{returnPage}"" class=""return-btn"">Return to Index</a>
</body>
</html>");
        File.WriteAllText(summaryPath, html.ToString());
    }
    #endregion

    #region Comparison Summary Report Writer
    public static void WriteComparisonSummary(string outputPath, string procIndexPath, string viewIndexPath, string tableIndexPath)
    {
        StringBuilder html = new StringBuilder();
        string procsIndex = $@"<a href=""{procIndexPath}"" class=""btn"">Procedures</a>";
        string viewsIndex = $@"<a href=""{viewIndexPath}"" class=""btn"">Views</a>";
        string tablesIndex = $@"<a href=""{tableIndexPath}"" class=""btn"">Tables</a>";
        html.Append(IndexPageTemplate.Replace("{procsIndex}", procsIndex).Replace("{viewsIndex}", viewsIndex).Replace("{tablesIndex}", tablesIndex));
        string indexPath = Path.Combine(outputPath, "index.html");
        File.WriteAllText(indexPath, html.ToString());
    }
    #endregion

    #region Individual Procedure Body Writer
    public static void WriteBodyHtml(string filePath, string title, string body, string returnPage)
    {
        StringBuilder html = new StringBuilder();
        html.AppendLine(BodyPageTemplate.Replace("{title}", title));
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
    public static void DifferencesWriter(string differencesPath, string sourceName, string destinationName, string sourceBody, string destinationBody, string title, string procName, string returnPage)
    {
        var html = new StringBuilder();
        html.AppendLine(BodyPageTemplate.Replace("{title}", title));
        html.AppendLine(@$"<body>
                        <h1>{procName}</h1>
                        <div>");

        var diffBuilder = new InlineDiffBuilder(new Differ());
        destinationBody ??= string.Empty;
        sourceBody ??= string.Empty;
        var result = diffBuilder.BuildDiffModel(destinationBody, sourceBody);
        foreach (var line in result.Lines)
        {
            string css = line.Type switch
            {
                ChangeType.Inserted => "source",
                ChangeType.Deleted => "destination",
                // ChangeType.Modified => "mod",
                _ => ""
            };

            string prefix = line.Type switch
            {
                ChangeType.Inserted => $"{sourceName} : ",
                ChangeType.Deleted => $"{destinationName} : ",
                //ChangeType.Modified => "~ ",
                _ => "  "
            };

            html.AppendLine($"<span class='{css}'>{System.Net.WebUtility.HtmlEncode(prefix + line.Text)}</span>");
        }
        html.AppendLine("</div>");
        html.AppendLine($@"<a href=""{returnPage}"" class=""return-btn"">Return to Summary</a>
    </body>
    </html>");
        File.WriteAllText(differencesPath, html.ToString());
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