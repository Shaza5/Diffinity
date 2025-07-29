# DbComparer

DbComparer is a C#-based application designed to compare stored procedures between two SQL Server databases. It identifies differences and can optionally apply changes to synchronize the procedures. The tool generates a detailed HTML report summarizing the comparison, with links to view the source and destination procedure bodies.

## Features

-   Compares stored procedures, views and tables between two SQL Server databases.
-   Uses a hash-based comparison to detect changes efficiently.
-   Generates an HTML summary report of the differences.
-   Provides Side-by-side visual diffs of source and destination objects.
-   Optionally applies changes to the destination database to match the source.
-   Filters the report to show all objects or only those with differences.
-   Logs execution details to both the console and a log file.

## Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/HelenNow/DbComparer.git
    cd DbComparer
    ```

2.  **Set up environment variables:**
    This project uses environment variables to store database connection strings. You need to set the following variables:
    -   `CorewellCs`: The connection string for the source database.
    -   `CmhCs`: The connection string for the destination database.

    You can set them in your system's environment variables or create a `.env` file in the project root.

3.  **Build the project:**
    Open the solution in Visual Studio and build the project, or use the `dotnet` CLI:
    ```bash
    dotnet build
    ```

## Usage

To run the application, you can use the `dotnet run` command from the `DbComparer` project directory:

```bash
cd DbComparer
dotnet run
```

The application's behavior is configured directly in the `Program.cs` file. You can modify the `Main` method to change the comparison options.

### Configuration Options

The `DbComparer.CompareProcs` method accepts the following parameters:

-   `sourceServer`: A `DbServer` object representing the source database.
-   `destinationServer`: A `DbServer` object representing the destination database.
-   `outputFolder`: The directory where the HTML reports will be saved.
-   `makeChange`: A `ComparerAction` enum that specifies whether to apply changes (`ApplyChanges`) or not (`DoNotApplyChanges`).
-   `filter`: A `ProcsFilter` enum that determines whether to include unchanged procedures in the report (`ShowUnchangedProcs`) or hide them (`HideUnchangedProcs`).

### Example

Here is the default configuration in `Program.cs`:

```csharp
public static void Main(string[] args)
{
    // ... (Logger configuration)
    // ... (environment variable validation and database connection checks at startup)

        var sw = new Stopwatch();
        sw.Start();
        string procIndexPath = DbComparer.CompareProcs(
            new DbServer(SourceDatabase, SourceConnectionString)
            , new DbServer(DestinationDatabase, DestinationConnectionString)
            , OutputFolder
            , ComparerAction.DoNotApplyChanges  // Set to ApplyChanges to update the destination DB
            , DbObjectFilter.HideUnchanged      // Set to ShowUnchangedProcs for a full report
        );
       string viewIndexPath = DbComparer.CompareViews(
           new DbServer(SourceDatabase, SourceConnectionString)
           , new DbServer(DestinationDatabase, DestinationConnectionString)
           , OutputFolder
           , ComparerAction.DoNotApplyChanges  // Set to ApplyChanges to update the destination DB
           , DbObjectFilter.HideUnchanged      // Set to ShowUnchangedProcs for a full report
       );
        string tableIndexpath = DbComparer.CompareTables(
         new DbServer(SourceDatabase, SourceConnectionString)
         , new DbServer(DestinationDatabase, DestinationConnectionString)
         , OutputFolder
         , ComparerAction.DoNotApplyChanges   // Set to ApplyChanges to update the destination DB
         , DbObjectFilter.HideUnchanged       // Set to ShowUnchangedProcs for a full report
     );
        HtmlReportWriter.WriteIndexSummary(SourceConnectionString,DestinationConnectionString,OutputFolder, procIndexPath, viewIndexPath, tableIndexpath);
        sw.Stop();
        Console.WriteLine($"Elapsed time: {sw} ms");
}
```

The HTML report is generated in the `DbComparer-output` folder by default.

## API Documentation

The core logic of the application is encapsulated in the `DbComparer` class and its helpers.

### `DbComparer.DbComparer` class

This is the main class that orchestrates the comparison process.

-   **`CompareProcs(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, ProcsFilter filter)`**:
    -   Compares stored procedures.
-   **`public static string CompareViews(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter)`**
    -   Compares SQL views.
-   **`public static string CompareTables(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter)`**
    -   Compares table definitions.
      
-   **`Each Method`**:
    -   Fetches all objects from the source and destination databases.
    -   Uses hash-based body comparison to detect changes.
    -   Optionally updates the destination procedure if differences are found.
    -   Generates a summary HTML report and individual HTML files for each procedure.

### `DbComparer.HtmlHelper.HtmlReportWriter` class

This class is responsible for generating the HTML reports.

-   **`WriteIndexSummary(...)`**: Creates a general index HTML file linking to procedure, table, and view reports.
-   **`WriteSummaryReport(...)`**: Generates a comparison summary page for a specific object type (e.g., procedures).
-   **`WriteBodyHtml(...)`**: Writes a simple HTML page showing the full body of a procedure, view, or table.
-   **`DifferencesWriter(...)`**: Generates a side-by-side diff view using the DiffPlex library highlighting differences between source and destination bodies.

### `DbComparer.ProcHelper, ViewHelper, and TableHelper` Namespaces

These namespaces contain classes responsible for fetching stored procedures, views, table schemas, and performing table comparison and update operations.

-   **`Fetcher`**: Retrieves object names and bodies or schema details from both source and destination databases.
    -   **`GetProcedureNames(...), GetViewsNames(...), GetTablesNames(...)`**: Fetch object names.
    -   **`GetProcedureBody(...), GetViewBody(...), GetTableInfo(...)`**: Fetch object bodies or schema details.
      
-   **`TableComparerAndUpdater`**: Compares table schemas between source and destination and updates the destination schema to match.
    -   **`CompareTables(...)`**: Compares column name, data type, nullability, max length, primary and foreign key flags between source and destination tables and optionally alters the destination schema to match.
 
### `DbComparer.DbObjectHandler` class

This class handles logic for comparing and updating database objects (procedures, views, etc.).

-   **`AreBodiesEqual(...)`**: Compares two SQL object bodies by normalizing and hashing them to determine structural equality.
-   **`AlterDbObject(...)`**: Alters or creates a database object on the destination by executing either a CREATE or an ALTER version of the source body.
-   **`dbObjectResult (...)`** (nested class): Holds the result of comparing a source and destination object, including metadata and file paths.

## Contributing

Contributions are welcome! If you have suggestions for improvements or find any issues, please feel free to open an issue or submit a pull request.

### How to Contribute

1.  **Fork the repository.**
2.  **Create a new branch** for your feature or bug fix:
    ```bash
    git checkout -b feature/your-feature-name
    ```
3.  **Make your changes** and commit them with a clear and descriptive message.
4.  **Push your changes** to your forked repository.
5.  **Create a pull request** to the main repository's `main` branch.

### Coding Style

Please adhere to the existing coding style and conventions used in the project. Ensure that your changes are well-documented and that the project builds successfully before submitting a pull request.

## To-Do

The `Program.cs` file contains a detailed to-do list with plans for future enhancements, including:

-   Implementing a command-line interface (CLI) for more flexible configuration.
-   Adding options to filter procedures by schema, name, or other criteria.
-   Extending the tool to support more complex comparison scenarios.

Feel free to pick an item from the to-do list if you're looking for a place to start contributing!
