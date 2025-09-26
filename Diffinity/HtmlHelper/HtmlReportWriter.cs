using ColorCode;
using Diffinity.TableHelper;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
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
    <h3>{Duration}</h3>
    <ul>
        <li>{procsIndex}</li>
        <li>{viewsIndex}</li>
        <li>{tablesIndex}</li>
        <li>{ignoredIndex}</li>
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
    <h1>[{source}] vs [{destination}] </h1>
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
    private const string IgnoredTemplate = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <title>Ignored Summary</title>
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
            width: 80%;
            border-collapse: collapse;
            margin:auto;
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
    <h1>Ignored Summary</h1>
    {nav}
    <table>
        <tr>
            <th></th>
            <th>Name</th>
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
        .difference {
            background-color: #fff3b0;  
            color: #000;              
        }
        .source {
            background-color: #d4edda; 
            color: #000;
        }
        .destination {
            background-color: #f8d7da;  
            color: #000;
        }
        .side-by-side {
            width: 100%;
            display: flex;
            gap: 20px;
            flex-wrap: wrap;
        }
        .side-by-side > div {
            flex: 1;
            min-width: 300px;
            margin-bottom: 0;  
        }
        .db-block {
            flex: 1;
            min-width: 300px;
            background: none;
            border: none;
            padding: 0;
            margin-bottom: 40px; 
        }
        .code-scroll {
            height: 400px;      
            overflow-y: auto;   
            overflow-x: auto;    
            margin-top: 10px;
        }
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
        .copy-btn {
            float: right;
            margin: 3px 12px 0px 50px;
            background-color: #EC317F;
            color: white;
            border: none;
            top: -2px;
            font-size : 15px;
            padding: 10px 12px;
            border-radius: 4px;
            cursor: pointer;
            box-shadow: 0 2px 6px rgba(236, 49, 127, 0.2);
        }
        .copy-btn:hover {
              background-color: #b42a68;
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
    public static string WriteIndexSummary(string sourceConnectionString, string destinationConnectionString, string outputPath, long Duration, string? ignoredIndexPath = null, string? procIndexPath = null, string? viewIndexPath = null, string? tableIndexPath = null)
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
        TimeSpan ts = TimeSpan.FromMilliseconds(Duration);
        double minutes = Duration / 60000.0;
        string formattedDuration = $"{minutes:F1} minutes";

        // Create links to individual index pages
        string procsIndex = procIndexPath == null ? "" : $@"<a href=""{procIndexPath}"" class=""btn"">Procedures</a>";
        string viewsIndex = viewIndexPath == null ? "" : $@"<a href=""{viewIndexPath}"" class=""btn"">Views</a>";
        string tablesIndex = tableIndexPath == null ? "" : $@"<a href=""{tableIndexPath}"" class=""btn"">Tables</a>";
        string ignoredIndex = ignoredIndexPath == null ? "" : $@"<a href=""{ignoredIndexPath}"" class=""btn"">Ignored</a>";

        // Replace placeholders in the index template
        html.Append(IndexTemplate.Replace("{sourceServer}", sourceServer).Replace("{sourceDatabase}", sourceDatabase).Replace("{destinationServer}", destinationServer).Replace("{destinationDatabase}", destinationDatabase).Replace("{procsIndex}", procsIndex).Replace("{viewsIndex}", viewsIndex).Replace("{tablesIndex}", tablesIndex).Replace("{ignoredIndex}", ignoredIndex).Replace("{Date}", Date).Replace("{Duration}", formattedDuration));
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
    public static (string html, string countObjects) WriteSummaryReport(DbServer sourceServer, DbServer destinationServer, string summaryPath, List<dbObjectResult> results, DbObjectFilter filter, Run run, bool isIgnoredEmpty,string ignoredCount)
    {
        StringBuilder html = new();
        var result = results[0];
        string returnPage = Path.Combine("..", "index.html");
        html.Append(ComparisonTemplate.Replace("{source}", sourceServer.name).Replace("{destination}", destinationServer.name).Replace("{MetaData}", result.Type).Replace("{nav}", BuildNav(run, isIgnoredEmpty,ignoredCount)));

        #region 1-Create the new table
        var newObjects = results.Where(r => r.IsDestinationEmpty).ToList();
        if (newObjects.Any())
        {
            StringBuilder newTable = new StringBuilder();
            newTable.AppendLine($@"<h2 style=""color: #B42A68;"">New {result.Type}s in {sourceServer.name} Database : </h2>
            <table>
                <tr>
                    <th></th>
                    <th>{result.Type} Name</th>
                    <th></th>
                    <th></th>
                </tr>");

            int newCount = 1;
            foreach (var item in newObjects)
            {

                string sourceBody = item.Type == "Table" ? PrintTableInfo(item.SourceTableInfo, new List<string>()) : item.SourceBody;


                string sourceLink = $@"<a href=""{item.SourceFile}"">View</a";
                string copyButton = $@"<button class=""copy-btn"" onclick=""copyPane(this)"">Copy</button><br>
                       <span class=""copy-target"" style=""display:none;"">{sourceBody}</span>";

                newTable.Append($@"<tr>
                                <td>{newCount}</td>
                                <td>{item.schema}.{item.Name}</td>
                                <td>{sourceLink}</td>
                                <td>{copyButton}</td>
                                </tr>");
                newCount++;
            }

            newTable.Append("</table><br><br>");
            newTable.AppendLine(
                @"<script>
                    function copyPane(button) {
                        const container = button.closest('tr');
                        const codeBlock = container.querySelector('.copy-target');
                        const text = codeBlock?.innerText.trim();

                        navigator.clipboard.writeText(text).then(() => {
                            button.textContent = 'Copied!';
                            setTimeout(() => button.textContent = 'Copy', 2000);
                        }).catch(err => {
                            console.error('Copy failed:', err);
                            alert('Failed to copy!');
                        });
                     }
                </script>"
            );
            html.Replace("{NewTable}", newTable.ToString());
     
        }
        else
        {
            html.Replace("{NewTable}", "");
        }
        #endregion

        #region 2-Create the Comparison Table
        int Number = 1;
        var existingObjects = results.Where(r => !r.IsDestinationEmpty).ToList();
        html.AppendLine($@"<h2 style = ""color: #B42A68;"">Changed {result.Type}s :</h2>");
        foreach (var item in existingObjects)
        {
                html.Replace("{differences}", "<th>Changes</th>");
                // Prepare file links
                string sourceColumn = item.SourceFile != null ? $@"<a href=""{item.SourceFile}"">View</a>" : "—";
                string destinationColumn = item.DestinationFile != null ? $@"<a href=""{item.DestinationFile}"">View</a>" : "—";
                string differencesColumn = item.DifferencesFile != null ? $@"<a href=""{item.DifferencesFile}"">View</a>" : "—";
                string newColumn = item.NewFile != null ? $@"<a href=""{item.NewFile}"">View</a>" : "—";

                if ((item.IsEqual && filter == DbObjectFilter.ShowUnchanged) || !item.IsEqual)
                {
                    html.Append($@"<tr>
                    <td>{Number}</td>
                    <td>{item.schema}.{item.Name}</td>
                    <td>{sourceColumn}</td>
                    <td>{destinationColumn}</td>
                    <td>{differencesColumn}</td>
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

        #region 3-Update counts in the nav bar
        int newObjectsCount = newObjects.Count();
        int notEqualCount = existingObjects.Count(r => !r.IsEqual);
        int equalCount = existingObjects.Count(r => r.IsEqual);
        string countObjects = filter == DbObjectFilter.ShowUnchanged ? $"({newObjectsCount}/{notEqualCount}/{equalCount})" : $"({newObjectsCount}/{notEqualCount})";
        #endregion

        return (html.ToString(), countObjects);
    }
    #endregion

    #region Ignored Report Writer
    public static DbComparer.summaryReportDto WriteIgnoredReport(string outputFolder, HashSet<string> ignoredObjects, Run run)
    {
        #region 1- Setup folder structure for reports
        Directory.CreateDirectory(outputFolder);
        string ignoredFolderPath = Path.Combine(outputFolder, "Ignored");
        Directory.CreateDirectory(ignoredFolderPath);
        #endregion

        StringBuilder html = new();
        string returnPage = Path.Combine("..", "index.html");
        string ignoredCount = ignoredObjects.Count().ToString();
        html.Append(IgnoredTemplate.Replace("{nav}", BuildNav(run, false,ignoredCount)));

        #region Create the Ignored Table
        int Number = 1;
        html.AppendLine($@"<h2 style = ""color: #B42A68; margin-left:100px"">Ignored Objects :</h2>");
        foreach (var item in ignoredObjects)
        {
            html.Append($@"<tr>
                    <td>{Number}</td>
                    <td>{item}</td>
                     </tr>");
            Number++;
        }
        html.Append($@"</table>
                       <br>
                       <a href=""{returnPage}"" class=""return-btn"">Return to Index</a>
                       </body>
                       </html>");

        return new DbComparer.summaryReportDto
        {
            path = "Ignored/index.html",
            fullPath = Path.Combine(ignoredFolderPath, "index.html"),
            html = html.ToString()
        };
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
        string coloredCode = HighlightSql(body);
        //string escapedBody = EscapeHtml(body);

        if (title.Contains("Table"))
        {
            coloredCode = body;
        }


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
        string normalizedSourceBody = Normalize(sourceBody);
        string normalizedDestinationBody = Normalize(destinationBody);
        string[] sourceBodyColored = HighlightSql(normalizedSourceBody).Split("\n");
        string[] destinationBodyColored = HighlightSql(normalizedDestinationBody).Split("\n");
        var sideBySideBuilder = new SideBySideDiffBuilder(differ);
        var model = sideBySideBuilder.BuildDiffModel(string.Join("\n", destinationBodyColored), string.Join("\n", sourceBodyColored));

        var html = new StringBuilder();
        html.AppendLine(DifferencesTemplate.Replace("{title}", title));

        // Source block
        html.AppendLine(@$"<h1>{Name}</h1>
                        <div class='diff-wrapper'>
                        <div class='pane'>
                        <button class='copy-btn' data-target='left'>Copy</button>
                        <h2>{sourceName}</h2>
                        <div class='code-scroll' id='left'><div class='code-block'>

");
        foreach (var line in model.NewText.Lines)
        {
            string css = GetCssClass(line.Type);
            string lineNumber = line.Position == 0 ? "" : line.Position.ToString();
            html.AppendLine(@$"<div class='line-number'>{lineNumber}</div>
                            <div class='line-text {css}'>{line.Text}</div>");
        }
        // Destination block
        html.Append($@"</div></div></div>
                        <div class='pane'>
                        <button class='copy-btn' data-target='right'>Copy</button>
                        <h2>{destinationName}</h2>
                        <div class='code-scroll' id='right'><div class='code-block'>
                        ");
        foreach (var line in model.OldText.Lines)
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
                document.querySelectorAll('.copy-btn').forEach(button => {{
                    button.addEventListener('click', () => {{
                        const targetId = button.getAttribute('data-target');
                        const block = document.getElementById(targetId);
                        const text = Array.from(block.querySelectorAll('.line-text'))
                                          .map(line => line.textContent)
                                          .join('\n');
                        navigator.clipboard.writeText(text).then(() => {{
                            button.textContent = 'Copied!';
                            setTimeout(() => button.textContent = 'Copy', 2000);
                        }});
                    }});
                }});
                </script>
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

        #region local function
        string Normalize(string input)
        {
            if (input == null) return null;

            // Normalize the input for consistent comparison
            return input.Replace("[", "").Replace("]", "");
        }
        #endregion
    }

    /// <summary>
    /// Prints table column details with to show the differences 
    /// </summary>
    public static void TableDifferencesWriter(string filePath, string sourceName, string destinationName, List<tableDto> sourceTable, List<tableDto> destinationTable, List<string> differences, string title, string objectName, string returnPage)
    {
        var html = new StringBuilder();
        html.AppendLine(BodyTemplate.Replace("{title}", title));

        html.AppendLine($@"
            <body>
            <h1>{title} for {objectName}</h1>
            <div class='side-by-side'>
                <div class='db-block'>
                    <span class='use'>{sourceName}</span>
                    <div class='code-scroll' id='left'>
                        <table>
                            <tr><th>Column Name</th><th>Column Type</th><th>Is Nullable</th><th>Max Length</th><th>Is Primary Key</th><th>Is Foreign Key</th></tr>");

        var destTableHtml = new StringBuilder();
        destTableHtml.AppendLine($@"
            <div class='db-block'>
                <span class='use'>{destinationName}</span>
                <div class='code-scroll' id='right'>
                    <table>
                        <tr><th>Column Name</th><th>Column Type</th><th>Is Nullable</th><th>Max Length</th><th>Is Primary Key</th><th>Is Foreign Key</th></tr>");

        // Track remaining columns that are not yet output
        var sourceRemaining = new Queue<tableDto>(sourceTable);
        var destRemaining = new Queue<tableDto>(destinationTable);

        // Output rows until both queues are empty
        while (sourceRemaining.Any() || destRemaining.Any())
        {
            tableDto srcCol = sourceRemaining.Any() ? sourceRemaining.Peek() : null;
            tableDto destCol = destRemaining.Any() ? destRemaining.Peek() : null;

            bool srcHas = srcCol != null;
            bool destHas = destCol != null;

            // If names match, we pop both; otherwise, pop whichever comes first in each table
            if (srcHas && destHas && srcCol.columnName.Equals(destCol.columnName, StringComparison.OrdinalIgnoreCase))
            {
                sourceRemaining.Dequeue();
                destRemaining.Dequeue();
            }
            else if (srcHas && (!destHas || !destinationTable.Select(c => c.columnName).Contains(srcCol.columnName, StringComparer.OrdinalIgnoreCase)))
            {
                srcCol = sourceRemaining.Dequeue();
                destCol = null;
            }
            else if (destHas && (!srcHas || !sourceTable.Select(c => c.columnName).Contains(destCol.columnName, StringComparer.OrdinalIgnoreCase)))
            {
                destCol = destRemaining.Dequeue();
                srcCol = null;
            }
            else
            {
                // Names do not match; align by order
                srcCol = sourceRemaining.Any() ? sourceRemaining.Dequeue() : null;
                destCol = destRemaining.Any() ? destRemaining.Dequeue() : null;
            }

            // Build source row
            if (srcCol != null)
            {
                string nameCss = destCol == null ? "source" :
                    (destCol.columnType != srcCol.columnType || destCol.isNullable != srcCol.isNullable ||
                     destCol.maxLength != srcCol.maxLength || destCol.isPrimaryKey != srcCol.isPrimaryKey ||
                     destCol.isForeignKey != srcCol.isForeignKey) ? "difference" : "";

                string typeCss = destCol != null && srcCol.columnType != destCol.columnType ? "difference" : "";
                string nullCss = destCol != null && srcCol.isNullable != destCol.isNullable ? "difference" : "";
                string lenCss = destCol != null && srcCol.maxLength != destCol.maxLength ? "difference" : "";
                string pkCss = destCol != null && srcCol.isPrimaryKey != destCol.isPrimaryKey ? "difference" : "";
                string fkCss = destCol != null && srcCol.isForeignKey != destCol.isForeignKey ? "difference" : "";

                html.AppendLine($@"
                <tr>
                    <td class='{nameCss}'>{srcCol.columnName}</td>
                    <td class='{typeCss}'>{srcCol.columnType}</td>
                    <td class='{nullCss}'>{srcCol.isNullable}</td>
                    <td class='{lenCss}'>{srcCol.maxLength}</td>
                    <td class='{pkCss}'>{srcCol.isPrimaryKey}</td>
                    <td class='{fkCss}'>{srcCol.isForeignKey}</td>
                </tr>");
            }
            else
            {
                html.AppendLine("<tr><td colspan='6' class='missing'>&nbsp;</td></tr>");
            }

            // Build destination row
            if (destCol != null)
            {
                string nameCss = srcCol == null ? "destination" :
                    (srcCol.columnType != destCol.columnType || srcCol.isNullable != destCol.isNullable ||
                     srcCol.maxLength != destCol.maxLength || srcCol.isPrimaryKey != destCol.isPrimaryKey ||
                     srcCol.isForeignKey != destCol.isForeignKey) ? "difference" : "";

                string typeCss = srcCol != null && destCol.columnType != srcCol.columnType ? "difference" : "";
                string nullCss = srcCol != null && destCol.isNullable != srcCol.isNullable ? "difference" : "";
                string lenCss = srcCol != null && destCol.maxLength != srcCol.maxLength ? "difference" : "";
                string pkCss = srcCol != null && destCol.isPrimaryKey != srcCol.isPrimaryKey ? "difference" : "";
                string fkCss = srcCol != null && destCol.isForeignKey != srcCol.isForeignKey ? "difference" : "";

                destTableHtml.AppendLine($@"
                <tr>
                    <td class='{nameCss}'>{destCol.columnName}</td>
                    <td class='{typeCss}'>{destCol.columnType}</td>
                    <td class='{nullCss}'>{destCol.isNullable}</td>
                    <td class='{lenCss}'>{destCol.maxLength}</td>
                    <td class='{pkCss}'>{destCol.isPrimaryKey}</td>
                    <td class='{fkCss}'>{destCol.isForeignKey}</td>
                </tr>");
            }
            else
            {
                destTableHtml.AppendLine("<tr><td colspan='6' class='missing'>&nbsp;</td></tr>");
            }
        }

        html.AppendLine("</table></div></div>");
        destTableHtml.AppendLine("</table></div></div>");
        html.AppendLine(destTableHtml.ToString());

        html.AppendLine($@"
            </div>
            <a href='{returnPage}' class='return-btn'>Return to Summary</a>
            <script>
            const blocks = document.querySelectorAll('.code-scroll');
            function syncScrollY(src, tgt) {{ tgt.scrollTop = src.scrollTop; }}
            if (blocks.length === 2) {{
              let isSyncing = false;
              blocks[0].addEventListener('scroll', () => {{ if(isSyncing) return; isSyncing = true; syncScrollY(blocks[0], blocks[1]); isSyncing = false; }});
              blocks[1].addEventListener('scroll', () => {{ if(isSyncing) return; isSyncing = true; syncScrollY(blocks[1], blocks[0]); isSyncing = false; }});
            }}
            </script>
            </body>
            </html>");

        File.WriteAllText(filePath, html.ToString());
    }
    #endregion

    #region Helpers
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
        if (sqlCode == null) return null;
        var colorizer = new CodeColorizer();
        string coloredCode = colorizer.Colorize(sqlCode, Languages.Sql).Replace(@"<div style=""color:Black;background-color:White;""><pre>", "").Replace("</div>", "");
        return coloredCode;
    }
    /// <summary>
    /// Write the nav section in the comparison summary pages
    /// </summary>
    static string BuildNav(Run run, bool isIgnoredEmpty,string count)
    {
        string proceduresPath = "../Procedures/index.html";
        string viewsPath = "../Views/index.html";
        string tablesPath = "../Tables/index.html";
        string ignoredPath = "../Ignored/index.html";
        var sb = new StringBuilder();
        sb.AppendLine(@"<nav class=""top-nav"">");
        switch (run)
        {
            case Run.Proc:
                sb.AppendLine($@"  <a href=""{proceduresPath}"">Procedures {{procsCount}}</a>");
                break;
            case Run.View:
                sb.AppendLine($@"  <a href=""{viewsPath}"">Views {{viewsCount}}</a>");
                break;
            case Run.Table:
                sb.AppendLine($@"  <a href=""{tablesPath}"">Tables {{tablesCount}}</a>");
                break;
            case Run.ProcView:
                sb.AppendLine($@"  <a href=""{proceduresPath}"">Procedures {{procsCount}}</a>");
                sb.AppendLine($@"  <a href=""{viewsPath}"">Views {{viewsCount}}</a>");
                break;
            case Run.ProcTable:
                sb.AppendLine($@"  <a href=""{proceduresPath}"">Procedures {{procsCount}}</a>");
                sb.AppendLine($@"  <a href=""{tablesPath}"">Tables {{tablesCount}}</a>");
                break;
            case Run.ViewTable:
                sb.AppendLine($@"  <a href=""{viewsPath}"">Views {{viewsCount}}</a>");
                sb.AppendLine($@"  <a href=""{tablesPath}"">Tables {{tablesCount}}</a>");
                break;

            case Run.All:
                sb.AppendLine($@"  <a href=""{proceduresPath}"">Procedures {{procsCount}}</a>");
                sb.AppendLine($@"  <a href=""{viewsPath}"">Views {{viewsCount}}</a>");
                sb.AppendLine($@"  <a href=""{tablesPath}"">Tables {{tablesCount}}</a>");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(run), run, "Invalid Run option");
        }
        if (!isIgnoredEmpty)
        {
            sb.AppendLine($@"  <a href=""{ignoredPath}"">Ignored({count})</a>");
        }
        sb.AppendLine("</nav>");
        return sb.ToString();
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