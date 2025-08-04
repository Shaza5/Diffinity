using Diffinity.TableHelper;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Data.SqlClient;
using ColorCode;
using System.Reflection;
using System.Text;
using static Diffinity.DbObjectHandler;



namespace Diffinity.HtmlHelper;

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
        h1,h2 {
            color: #EC317F;
            text-align: center;
            margin-bottom: 10px;
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
        h3{
        color: #B42A68;
        text-align: center;
        margin-bottom:40px;
        }
    </style>
</head>
<body>
    <h1>Database Comparison Summary</h1>
    <h2>{sourceServer} : {sourceDatabase}</h2>
    <h2>{destinationServer} : {destinationDatabase}</h2>
    <h3>{Date}</h3>
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
        .top-nav {
            display: flex;
            justify-content: center;
            gap: 60px; /* controls spacing between links */
            margin-bottom: 40px;
        }
        
        .top-nav a {
            position: relative;
            color: #EC317F;
            text-decoration: none;
            font-weight: 600;
            font-size: 1.2rem;
            padding-bottom: 6px;
        }
        
        .top-nav a::after {
            content: '';
            position: absolute;
            left: 0;
            bottom: 0;
            width: 100%;
            height: 3px;
            background-color: #EC317F;
            transform: scaleX(0);
            transform-origin: bottom left;
            transition: transform 0.3s ease;
        }
        
        .top-nav a:hover::after {
            transform: scaleX(1);
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
    {nav}
    {NewTable}
    <table>
        <tr>
            <th></th>
            <th>{MetaData} Name</th>
            <th>{source} Original</th>
            <th>{destination} Original</th>
            {differences}
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
        .copy-btn {
            float: right;
            margin: 0px 12px 0px 50px;
            background-color: #EC317F;
            color: white;
            border: none;
            font-size : 15px;
            padding: 10px 12px;
            border-radius: 4px;
            cursor: pointer;
            box-shadow: 0 2px 6px rgba(236, 49, 127, 0.2);
        }
        .copy-btn:hover {
              background-color: #b42a68;
        }
        .use{
            color: #EC317F;
            font-weight : bold;
            float: left;
            font-size: 20px
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
            overflow: hidden;
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
       .code-scroll {
            height: 605px;
            overflow: auto; 
        }
        .code-block {
            display: grid;
            grid-template-columns: 50px 1fr;
            font-size: 0.95rem;
            line-height: 1.4;
            padding: 10px;
            white-space: pre;
            width: fit-content; 
        }
         .line-number {
             color: #999;
             text-align: right;
             padding-right: 10px;
             user-select: none;
         }
         .line-text {
              white-space: pre;        
              font-family: Consolas;
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
        </head>
        <body>";

    #region Index Report Writer
    /// <summary>
    /// Writes the main index summary HTML page linking to individual reports for procedures, views, and tables.
    /// </summary>
    public static string WriteIndexSummary(string sourceConnectionString, string destinationConnectionString, string outputPath, string? procIndexPath = null, string? viewIndexPath = null, string? tableIndexPath = null)
    {
        // Extract server and database names from connection strings
        var sourceBuilder = new SqlConnectionStringBuilder(sourceConnectionString);
        var destinationBuilder = new SqlConnectionStringBuilder(destinationConnectionString);
        string sourceServer = sourceBuilder.DataSource;
        string destinationServer = destinationBuilder.DataSource;
        string sourceDatabase = sourceBuilder.InitialCatalog;
        string destinationDatabase = destinationBuilder.InitialCatalog;
        
        StringBuilder html = new StringBuilder();
        DateTime date = DateTime.UtcNow; ;
        string Date = date.ToString("MM/dd/yyyy hh:mm tt ") + "UTC";

        // Create links to individual index pages
        string procsIndex = procIndexPath==null ? "": $@"<a href=""{procIndexPath}"" class=""btn"">Procedures</a>";
        string viewsIndex = viewIndexPath==null ? "" :$@"<a href=""{viewIndexPath}"" class=""btn"">Views</a>";
        string tablesIndex = tableIndexPath == null ? "" : $@"<a href=""{tableIndexPath}"" class=""btn"">Tables</a>";

        // Replace placeholders in the index template
        html.Append(IndexTemplate.Replace("{sourceServer}",sourceServer).Replace("{sourceDatabase}", sourceDatabase).Replace("{destinationServer}", destinationServer).Replace("{destinationDatabase}", destinationDatabase).Replace("{procsIndex}", procsIndex).Replace("{viewsIndex}", viewsIndex).Replace("{tablesIndex}", tablesIndex).Replace("{Date}", Date));
        string indexPath = Path.Combine(outputPath, "index.html");

        // Write to index.html
        File.WriteAllText(indexPath, html.ToString());
        return indexPath;
    }
    #endregion

    #region Summary Report Writer
    /// <summary>
    /// Writes a detailed summary report comparing objects (procedures, views, tables) between source and destination.
    /// </summary>
    public static void WriteSummaryReport(DbServer sourceServer, DbServer destinationServer, string summaryPath, List<dbObjectResult> results, DbObjectFilter filter,Run run)
    {
        StringBuilder html = new();
        var result = results[0];
        string returnPage = Path.Combine("..", "index.html");
        html.Append(ComparisonTemplate.Replace("{source}", sourceServer.name).Replace("{destination}", destinationServer.name).Replace("{MetaData}", result.Type).Replace("{nav}",BuildNav(run)));

        #region 1-Create the new table
        var newProcedures = results.Where(r => r.IsDestinationEmpty).ToList();
        if (newProcedures.Any())
        {
            StringBuilder newTable = new StringBuilder();
            newTable.AppendLine($@"<h2 style=""color: #B42A68;"">New {result.Type}s in {sourceServer.name} Database : </h2>
            <table>
                <tr>
                    <th></th>
                    <th>{result.Type} Name</th>
                    <th></th>
                </tr>");

            int newCount = 1;
            foreach (var item in newProcedures)
            {
                if (item.SourceFile == null) continue;
                string sourceLink = $@"<a href=""{item.SourceFile}"">View</a";
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
        html.AppendLine($@"<h2 style = ""color: #B42A68;"">Changed {result.Type}s :</h2>");
        foreach (var item in results)
        {
            if (item.IsDestinationEmpty) continue;
            // Prepare file links
            string sourceColumn = item.SourceFile != null ? $@"<a href=""{item.SourceFile}"">View</a>" : "—";
            string destinationColumn = item.DestinationFile != null ? $@"<a href=""{item.DestinationFile}"">View</a>" : "—";
            string differencesColumn = null;

            // Add differences column only if file exists
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
                    <td>{sourceColumn}</td>
                    <td>{destinationColumn}</td>
                    {differencesColumn}
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
        #region Local function
        string BuildNav(Run run)
        {
            string proceduresPath = "../Procedures/index.html";
            string viewsPath = "../Views/index.html";
            string tablesPath= "../Tables/index.html";
            var sb = new StringBuilder();
            sb.AppendLine(@"<nav class=""top-nav"">");
            switch (run)
            {
                case Run.Proc:
                    sb.AppendLine($@"  <a href=""{proceduresPath}"">Procedures</a>");
                    break;

                case Run.View:
                    sb.AppendLine($@"  <a href=""{viewsPath}"">Views</a>");
                    break;

                case Run.Table:
                    sb.AppendLine($@"  <a href=""{tablesPath}"">Tables</a>");
                    break;

                case Run.ProcView:
                    sb.AppendLine($@"  <a href=""{proceduresPath}"">Procedures</a>");
                    sb.AppendLine($@"  <a href=""{viewsPath}"">Views</a>");
                    break;

                case Run.ProcTable:
                    sb.AppendLine($@"  <a href=""{proceduresPath}"">Procedures</a>");
                    sb.AppendLine($@"  <a href=""{tablesPath}"">Tables</a>");
                    break;

                case Run.ViewTable:
                    sb.AppendLine($@"  <a href=""{viewsPath}"">Views</a>");
                    sb.AppendLine($@"  <a href=""{tablesPath}"">Tables</a>");
                    break;

                case Run.All:
                    sb.AppendLine($@"  <a href=""{proceduresPath}"">Procedures</a>");
                    sb.AppendLine($@"  <a href=""{viewsPath}"">Views</a>");
                    sb.AppendLine($@"  <a href=""{tablesPath}"">Tables</a>");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(run), run, "Invalid Run option");
            }
            sb.AppendLine("</nav>");
            return sb.ToString();
        }
        #endregion
    }
    #endregion

    #region Individual Procedure Body Writer
    /// <summary>
    /// Writes the HTML page showing the body of a single procedure/view/table, with copy functionality.
    /// </summary>
    public static void WriteBodyHtml(string filePath, string title, string body, string returnPage)
    {
        StringBuilder html = new StringBuilder();
        html.AppendLine(BodyTemplate.Replace("{title}", title));
        string escapedBody = EscapeHtml(body);

        if (title.Contains("Table"))
        {
            escapedBody = body;
        }
        string coloredCode = HighlightSql(escapedBody);

        html.AppendLine($@"<body>
        <h1>{title}</h1>
            <div>
            <span class=""use"">Use {title}</span> <button class='copy-btn' onclick='copyPane(this)'>Copy</button><br>
            <span class=""copy-target"">{coloredCode}</span>
            </div>
            
                  <script>
            function copyPane(button) {{
                const container = button.closest('div');
                const codeBlock = container.querySelector('.copy-target');
                const text = codeBlock?.innerText.trim();
            
                navigator.clipboard.writeText(text).then(() => {{
                    button.textContent = 'Copied!';
                    setTimeout(() => button.textContent = 'Copy', 2000);
                }}).catch(err => {{
                    console.error('Copy failed:', err);
                    alert('Failed to copy!');
                }});
            }}
            </script>
            <a href=""{returnPage}"" class=""return-btn"">Return to Summary</a>
            </body>
            </html>");
        File.WriteAllText(filePath, html.ToString());
    }
    #endregion

    #region Differences Writer
    /// <summary>
    /// Generates a side-by-side HTML diff view for a procedure/view/table.
    /// </summary>
    public static void DifferencesWriter(string differencesPath, string sourceName, string destinationName, string sourceBody, string destinationBody, string title, string Name, string returnPage)
    {
        var differ = new Differ();
        string[] sourceBodyColored = HighlightSql(sourceBody).Split("\n");
        string[] destinationBodyColored = HighlightSql(destinationBody).Split("\n");
        var sideBySideBuilder = new SideBySideDiffBuilder(differ);
        var model = sideBySideBuilder.BuildDiffModel(string.Join("\n", destinationBodyColored), string.Join("\n",sourceBodyColored));

        var html = new StringBuilder();
        html.AppendLine(DifferencesTemplate.Replace("{title}", title));

        // Destination block
        html.AppendLine(@$"<h1>{Name}</h1>
                        <div class='diff-wrapper'>
                        <div class='pane'><h2>{destinationName}</h2><div class=""code-scroll""><div class='code-block'>");
        foreach (var line in model.OldText.Lines)
        {
            string css = GetCssClass(line.Type);
            string lineNumber = line.Position == 0 ? "" : line.Position.ToString();
            html.AppendLine(@$"<div class='line-number'>{lineNumber}</div>
                           <div class='line-text {css}'>{line.Text}</div>");
        }

        // Source block
        html.AppendLine(@$"</div></div></div>
                        <div class='pane'><h2>{sourceName}</h2><div class=""code-scroll""><div class='code-block'>");
        foreach (var line in model.NewText.Lines)
        {
            string css = GetCssClass(line.Type);
            string lineNumber = line.Position == 0 ? "" : line.Position.ToString();
            html.AppendLine(@$"<div class='line-number'>{lineNumber}</div>
                            <div class='line-text {css}'>{line.Text}</div>");
        }

        // Scroll sync script
        html.AppendLine(@$"</div></div></div></div><br>
                 <a href=""{returnPage}"" class=""return-btn"">Return to Summary</a>

                 <script>
                 const blocks = document.querySelectorAll('.code-scroll');
                 
                 function syncScroll(source, target) {{
                     target.scrollTop = source.scrollTop;
                     target.scrollLeft = source.scrollLeft;
                 }}

                 if (blocks.length === 2) {{
                     let isSyncingScroll = false;

                     blocks[0].addEventListener('scroll', () => {{
                         if (isSyncingScroll) return;
                         isSyncingScroll = true;
                         syncScroll(blocks[0], blocks[1]);
                         isSyncingScroll = false;
                     }});

                     blocks[1].addEventListener('scroll', () => {{
                         if (isSyncingScroll) return;
                         isSyncingScroll = true;
                         syncScroll(blocks[1], blocks[0]);
                         isSyncingScroll = false;
                     }});
                 }}
                 </script>

                 </body>
                 </html>");
        File.WriteAllText(differencesPath, html.ToString());
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Escapes HTML special characters.
    /// </summary>
    static string EscapeHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        return input.Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");
    }

    /// <summary>
    /// Maps DiffPlex line change types to CSS class names.
    /// </summary>
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

    /// <summary>
    /// Highlights SQL source code by applying syntax coloring for display purposes.
    /// </summary>
    static string HighlightSql(string sqlCode)
    {
        var colorizer = new CodeColorizer();
        string coloredCode=colorizer.Colorize(sqlCode, Languages.Sql).Replace(@"<div style=""color:Black;background-color:White;""><pre>", "").Replace("</div>", "");
        return coloredCode;
    }

    /// <summary>
    /// Prints table column details with optional difference highlighting.
    /// </summary>
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