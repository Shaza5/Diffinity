## API Overview

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

