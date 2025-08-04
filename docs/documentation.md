## Features

-   Compares stored procedures, views and tables between two SQL Server databases.
-   Uses a hash-based comparison to detect changes efficiently.
-   Generates an HTML summary report of the differences.
-   Provides Side-by-side visual diffs of source and destination objects.
-   Optionally applies changes to the destination database to match the source.
-   Filters the report to show all objects or only those with differences.
-   Logs execution details to both the console and a log file.
-   Supports a `.diffignore` file to exclude specific procedures, views, or tables from comparison.

<img width="1678" height="862" alt="image (4)" src="https://github.com/user-attachments/assets/620f8bee-db41-447d-9392-d79a1687ebc0" />

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
