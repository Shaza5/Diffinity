# Diffinity
[![NuGet](https://img.shields.io/nuget/v/Diffinity.svg)](https://www.nuget.org/packages/Diffinity/)

Diffinity is a C# library and NuGet package for comparing database objects—such as stored procedures, views, and tables, between two SQL Server databases. It identifies differences and optionally applies changes to synchronize the objects. The library also supports generating detailed HTML reports summarizing the comparison, including links to view the source and destination definitions.

Diffinity can be used as a standalone library in your own applications or through the included console driver (Driver) for out-of-the-box functionality.

## Features

-   Compares stored procedures, views and tables between two SQL Server databases.
-   Uses a hash-based comparison to detect changes efficiently.
-   Generates an HTML summary report of the differences.
-   Provides Side-by-side visual diffs of source and destination objects.
-   Optionally applies changes to the destination database to match the source.
-   Filters the report to show all objects or only those with differences.
-   Logs execution details to both the console and a log file.


## Database Setup

Before using the `Diffinity` library, you must create a few stored procedures in **both** the source and destination SQL Server databases. These procedures are used internally to fetch metadata and object definitions.

You can find the setup script here:

➡️ [sql/setup-required-procs.sql](https://github.com/HelenNow/Diffinity/blob/main/sql/setup-required-procs.sql)

Run the script on both databases to ensure `Diffinity` functions correctly.

## Getting Started
### Option 1: Use the NuGet Package

Install the Diffinity library into your own project:

```bash
dotnet add package Diffinity
```

### Option 2: Use the Console App (Driver)
To use the included console driver (`Driver`), clone the repository and follow instructions in the GitHub README:

📎 [GitHub Repository](https://github.com/HelenNow/Diffinity)


## Usage
### Example Usage of the Library

```csharp
using Diffinity;

var result = DbComparer.CompareProcs(
    new DbServer("Source", sourceCs),
    new DbServer("Dest", destinationCs),
    "output-folder",
    ComparerAction.DoNotApplyChanges,
    DbObjectFilter.ShowUnchanged
);
```

## API Overview

The core logic of the application is encapsulated in the `Diffinity` class and its helpers.

### `DbComparer` class

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


## For Contributors

If you're interested in contributing to Diffinity, visit the GitHub repository:
👉 https://github.com/HelenNow/Diffinity

Developed with :sparkling_heart: in Lebanon