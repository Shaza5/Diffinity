<div align="center">
  <img src="https://github.com/user-attachments/assets/b31874d5-2771-477f-a7e5-cced9af0b257" width="250" height="250" alt="Screenshot_2025-07-30_170511-removebg-preview" />
</div>

# Diffinity
Diffinity is a C# library and NuGet package for comparing database objects—such as stored procedures, views, and tables, between two SQL Server databases. It identifies differences and optionally applies changes to synchronize the objects. The library also supports generating detailed HTML reports summarizing the comparison, including links to view the source and destination definitions.

Diffinity can be used as a standalone library in your own applications or through the included console driver (Driver) for out-of-the-box functionality.

## Getting Started
### Option 1: Use the NuGet Package

[![NuGet](https://img.shields.io/nuget/v/Diffinity.svg)](https://www.nuget.org/packages/Diffinity)

Install the Diffinity library into your own project:

```bash
dotnet add package Diffinity
```

### Option 2: Use the Console App (Driver)

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/HelenNow/Diffinity
    cd Diffinity
    ```

2.  **Set up environment variables:**
    This project uses environment variables to store database connection strings. You need to set the following variables:
    -   `db_v1_cs`: The connection string for the source database.
    -   `db_v2_cs`: The connection string for the destination database.

    You can set them in your system's environment variables or create a `.env` file in the project root.

3.  **Build and run:**
    Open the solution in Visual Studio and build the project, or use the `dotnet` CLI:
    ```bash
    dotnet build
    dotnet run --project Driver
    ```
The application's behavior is configured directly in the `Program.cs` file. You can modify the `Main` method to change the comparison options.


## Example Usage

```csharp
using Diffinity;
using System.Diagnostics;

internal class Program
{
    static void Main(string[] args)
    {
        var MyDbV1 = new DbServer("My Db V1", Environment.GetEnvironmentVariable("db_v1_cs"));
        var MyDbV2 = new DbServer("My Db V2", Environment.GetEnvironmentVariable("db_v2_cs"));
        string IndexPage = DbComparer.Compare(MyDbV1, MyDbV2);
        Process.Start(new ProcessStartInfo { FileName = IndexPage, UseShellExecute = true });
    }
}
```

## Detailed Example Usage
```csharp
using Diffinity;
using System.Diagnostics;

internal class Program
{
    static void Main(string[] args)
    {
        var MyDbV1 = new DbServer("My Db V1", Environment.GetEnvironmentVariable("db_v1_cs"));
        var MyDbV2 = new DbServer("My Db V2", Environment.GetEnvironmentVariable("db_v2_cs"));

        // You can optionally pass any of the following parameters:
        // logger: your custom ILogger instance
        // outputFolder: path to save the results (string)
        // makeChange: whether to apply changes (ComparerAction.ApplyChanges,ComparerAction.DoNotApplyChanges)
        // filter: filter rules (DbObjectFilter.ShowUnchanged,DbObjectFilter.HideUnchanged)
        // run: execute comparison on specific dbObject(Run.Proc,Run.View,Run.Table,Run.ProcView,Run.ProcTable,Run.ViewTable,Run.All)
        string IndexPage = DbComparer.Compare(MyDbV1, MyDbV2, logger: myLogger, outputFolder: "customPath", makeChange: true, run: Run.Proc);
        Process.Start(new ProcessStartInfo { FileName = IndexPage, UseShellExecute = true });
    }
}
```
The HTML report is generated in the `Diffinity-output` folder by default.

<img width="1678" height="862" alt="image (4)" src="https://github.com/user-attachments/assets/620f8bee-db41-447d-9392-d79a1687ebc0" />

## Features

-   Compares stored procedures, views and tables between two SQL Server databases.
-   Uses a hash-based comparison to detect changes efficiently.
-   Generates an HTML summary report of the differences.
-   Provides Side-by-side visual diffs of source and destination objects.
-   Optionally applies changes to the destination database to match the source.
-   Filters the report to show all objects or only those with differences.
-   Logs execution details to both the console and a log file.





### Configuration Options

The `Diffinity.Compare` method accepts the following parameters:

-   `sourceServer`: A `DbServer` object representing the source database.
-   `destinationServer`: A `DbServer` object representing the destination database.
-   `logger` (optional): An `ILogger` instance for logging output. If not provided, a default logger is used.
-   `outputFolder` (optional): The `directory` where the generated HTML comparison reports will be saved. Defaults to a predefined Diffinity-output if not specified.`
-   `makeChange`: A `ComparerAction` enum that specifies whether to apply changes (`ApplyChanges`) or not (`DoNotApplyChanges`).
-   `filter`: A `DbObjectFilter` enum that determines whether to include unchanged procedures in the report (`ShowUnchanged`) or hide them (`HideUnchanged`).
-   `run`: A Run `enum` value indicating which database objects to compare, if not specified, default is All:
    -   `Proc`: Compare only stored procedures.
    -   `View`: Compare only views.
    -   `Table`: Compare only tables.
    -   `ProcView`: Compare stored procedures and views.
    -   `ProcTable`: Compare stored procedures and tables.
    -   `ViewTable`: Compare views and tables.
    -   `All`: Compare procedures, views, and tables.

## API Overview

The core logic of the application is encapsulated in the `Diffinity` class and its helpers.

### `DbComparer` class

This is the main class that orchestrates the comparison process.
-   **`Compare(DbServer sourceServer,DbServer destinationServer, ILogger? logger = null, string? outputFolder = null, ComparerAction? makeChange = ComparerAction.DoNotApplyChanges, DbObjectFilter? filter = DbObjectFilter.HideUnchanged,Run? run=Run.All)`**:
    -   Compares selected database objects based on the Run option and returns a generated HTML summary report.
-   **`CompareProcs(DbServer sourceServer, DbServer destinationServer,string outputFolder , ComparerAction makeChange, DbObjectFilter filter, Run run)`**:
    -   Compares stored procedures.
-   **`CompareViews(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter, Run run)`**
    -   Compares SQL views.
-   **`CompareTables(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, DbObjectFilter filter, Run run)`**
    -   Compares table definitions.
      
-   **`Each Method`**:
    -   Fetches all objects from the source and destination databases.
    -   Uses hash-based body comparison to detect changes.
    -   Optionally updates the destination procedure if differences are found.
    -   Generates a summary HTML report and individual HTML files for each procedure.


### `HtmlHelper.HtmlReportWriter` class

This class is responsible for generating the HTML reports.

-   **`WriteIndexSummary(...)`**: Creates a general index HTML file linking to procedure, table, and view reports.
-   **`WriteSummaryReport(...)`**: Generates a comparison summary page for a specific object type (e.g., procedures).
-   **`WriteBodyHtml(...)`**: Writes a simple HTML page showing the full body of a procedure, view, or table.
-   **`DifferencesWriter(...)`**: Generates a side-by-side diff view using the DiffPlex library highlighting differences between source and destination bodies.


### `ProcHelper, ViewHelper and TableHelper` Namespaces

These namespaces contain classes responsible for fetching stored procedures, views, table schemas, and performing table comparison and update operations.

-   **`Fetcher`**: Retrieves object names and bodies or schema details from both source and destination databases.
    -   **`GetProcedureNames(...), GetViewsNames(...), GetTablesNames(...)`**: Fetch object names.
    -   **`GetProcedureBody(...), GetViewBody(...), GetTableInfo(...)`**: Fetch object bodies or schema details.
      
-   **`TableComparerAndUpdater`**: Compares table schemas between source and destination and updates the destination schema to match.
    -   **`CompareTables(...)`**: Compares column name, data type, nullability, max length, primary and foreign key flags between source and destination tables and optionally alters the destination schema to match.

 
### `DbObjectHandler` class

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

<div align="center">
  <sub><strong>Developed by Nada Husseini and Najat Bdeir with :sparkling_heart: in Lebanon</strong></sub>
</div>
