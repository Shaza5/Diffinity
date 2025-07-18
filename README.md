# DbComparer

DbComparer is a .NET tool designed to compare stored procedures between two SQL Server databases. It identifies differences and can optionally apply changes to synchronize the procedures. The tool generates a detailed HTML report summarizing the comparison, with links to view the source and destination procedure bodies.

## Features

-   Compares stored procedures between two SQL Server databases.
-   Generates an HTML summary report of the differences.
-   Provides side-by-side HTML views of the procedure bodies.
-   Optionally applies changes to the destination database to match the source.
-   Filters the report to show all procedures or only those with differences.
-   Logs execution details to both the console and a log file.

## Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-username/DbComparer.git
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

    var sw = new Stopwatch();
    sw.Start();
    DbComparer.CompareProcs(
        new DbServer("Corewell", SourceConnectionString),
        new DbServer("CMH", DestinationConnectionString),
        OutputFolder,
        ComparerAction.DoNotApplyChanges, // Set to ApplyChanges to update the destination DB
        ProcsFilter.HideUnchangedProcs    // Set to ShowUnchangedProcs for a full report
    );
    sw.Stop();
    Console.WriteLine($"Elapsed time: {sw.ElapsedMilliseconds} ms");
}
```

The HTML report is generated in the `DbComparer-output` folder by default.

## API Documentation

The core logic of the application is encapsulated in the `DbComparer` class and its helpers.

### `DbComparer.DbComparer` class

This is the main class that orchestrates the comparison process.

-   **`CompareProcs(DbServer sourceServer, DbServer destinationServer, string outputFolder, ComparerAction makeChange, ProcsFilter filter)`**:
    -   Fetches all stored procedures from the source database.
    -   Compares each procedure's body with the corresponding one in the destination database.
    -   Optionally updates the destination procedure if differences are found.
    -   Generates a summary HTML report and individual HTML files for each procedure.

### `DbComparer.HtmlHelper.HtmlReportWriter` class

This class is responsible for generating the HTML reports.

-   **`WriteSummaryReport(...)`**: Creates the main `index.html` file with a table summarizing the comparison results.
-   **`WriteProcedureBodyHtml(...)`**: Creates individual HTML files for viewing the body of each stored procedure.

### `DbComparer.ProcHelper` Namespace

This namespace contains classes for fetching, comparing, and updating stored procedures.

-   **`ProcedureFetcher`**: Retrieves procedure names and bodies from the databases.
-   **`ProcedureComparer`**: Compares two procedure bodies using a hash-based approach for efficiency.
-   **`ProcedureUpdater`**: Applies changes to the destination database by executing an `ALTER PROCEDURE` statement.
-   **`HashHelper`**: Computes the hash of a string.

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
